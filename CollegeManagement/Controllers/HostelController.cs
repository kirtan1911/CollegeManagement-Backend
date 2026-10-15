using CollegeManagementSystem.Data;
using CollegeManagementSystem.DTOs;
using CollegeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostelController : ControllerBase
    {
        private readonly AppDbContext _context;
        public HostelController(AppDbContext context) => _context = context;

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── GET: api/Hostel ── List all hostels
        [Authorize(Roles = "Admin,Staff,Faculty,Student")]
        [HttpGet]
        public async Task<IActionResult> GetHostels()
        {
            var hostels = await _context.Hostels
                .Select(h => new
                {
                    h.HostelId,
                    h.Name,
                    h.Type,
                    h.TotalRooms,
                    RoomCount = h.Rooms.Count
                })
                .ToListAsync();
            return Ok(hostels);
        }

        // ── POST: api/Hostel ── Admin creates a hostel
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateHostel([FromBody] HostelDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Hostel name required che." });

            var hostel = new Hostel
            {
                Name = dto.Name,
                Type = dto.Type,
                TotalRooms = dto.TotalRooms
            };
            _context.Hostels.Add(hostel);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Hostel create thayo!", hostel.HostelId });
        }

        // ── GET: api/Hostel/rooms ── List all rooms
        [Authorize(Roles = "Admin,Staff,Faculty,Student")]
        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms([FromQuery] int? hostelId)
        {
            var query = _context.HostelRooms
                .Include(r => r.Hostel)
                .AsQueryable();

            if (hostelId.HasValue)
                query = query.Where(r => r.HostelId == hostelId.Value);

            var rooms = await query
                .Select(r => new
                {
                    r.RoomId,
                    r.RoomNumber,
                    r.Capacity,
                    r.OccupiedCount,
                    AvailableSlots = r.Capacity - r.OccupiedCount,
                    HostelName = r.Hostel!.Name,
                    r.HostelId
                })
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            return Ok(rooms);
        }

        // ── POST: api/Hostel/rooms ── Admin adds a room
        [Authorize(Roles = "Admin")]
        [HttpPost("rooms")]
        public async Task<IActionResult> AddRoom([FromBody] HostelRoomDTO dto)
        {
            var hostelExists = await _context.Hostels.AnyAsync(h => h.HostelId == dto.HostelId);
            if (!hostelExists)
                return NotFound(new { message = "Hostel maldo nahi." });

            var room = new HostelRoom
            {
                HostelId = dto.HostelId,
                RoomNumber = dto.RoomNumber,
                Capacity = dto.Capacity,
                OccupiedCount = 0
            };
            _context.HostelRooms.Add(room);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Room add thayo!", room.RoomId });
        }

        // ── POST: api/Hostel/allot ── Allot a room to a student
        [Authorize(Roles = "Admin")]
        [HttpPost("allot")]
        public async Task<IActionResult> AllotRoom([FromBody] HostelAllotmentDTO dto)
        {
            var room = await _context.HostelRooms
                .Include(r => r.Hostel)
                .FirstOrDefaultAsync(r => r.RoomId == dto.RoomId);

            if (room == null)
                return NotFound(new { message = "Room maldo nahi." });

            if (room.OccupiedCount >= room.Capacity)
                return BadRequest(new { message = "Room full che. Biji room select karo." });

            var profileExists = await _context.StudentProfiles
                .AnyAsync(sp => sp.StudentProfileId == dto.StudentProfileId);
            if (!profileExists)
                return NotFound(new { message = "Student profile maldo nahi." });

            // Check already allotted (active allotment = VacatedDate == null)
            var existing = await _context.HostelAllotments
                .AnyAsync(a => a.StudentProfileId == dto.StudentProfileId && a.VacatedDate == null);
            if (existing)
                return Conflict(new { message = "Student ne pahela thi j hostel allot thayeli che." });

            var allotment = new HostelAllotment
            {
                StudentProfileId = dto.StudentProfileId,
                RoomId = dto.RoomId,
                AllotmentDate = DateTime.UtcNow,
                FeeStatus = "Pending"
            };
            _context.HostelAllotments.Add(allotment);

            room.OccupiedCount++;

            // Note: Admin must separately add the hostel fee record via POST /api/Fee/add
            // with the correct amount after allotment. Do NOT auto-create a zero-amount
            // fee here as it would create an unpaid record that blocks exam access.

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Room allot thayo!",
                allotment.AllotmentId,
                Room = room.RoomNumber,
                Hostel = room.Hostel!.Name
            });
        }

        // ── DELETE: api/Hostel/vacate/{allotmentId} ── Vacate a room
        [Authorize(Roles = "Admin")]
        [HttpDelete("vacate/{allotmentId}")]
        public async Task<IActionResult> VacateRoom(int allotmentId)
        {
            var allotment = await _context.HostelAllotments
                .Include(a => a.Room)
                .FirstOrDefaultAsync(a => a.AllotmentId == allotmentId);

            if (allotment == null)
                return NotFound(new { message = "Allotment maldo nahi." });

            if (allotment.VacatedDate != null)
                return BadRequest(new { message = "Room pahela thi j vacate thai gayo che." });

            allotment.VacatedDate = DateTime.UtcNow;
            if (allotment.Room != null && allotment.Room.OccupiedCount > 0)
                allotment.Room.OccupiedCount--;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Room vacate thayu." });
        }

        // ── GET: api/Hostel/my-allotment ── Student's own allotment
        [Authorize(Roles = "Student")]
        [HttpGet("my-allotment")]
        public async Task<IActionResult> GetMyAllotment()
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var allotment = await _context.HostelAllotments
                .Include(a => a.Room).ThenInclude(r => r!.Hostel)
                .Where(a => a.StudentProfileId == profile.StudentProfileId && a.VacatedDate == null)
                .Select(a => new
                {
                    a.AllotmentId,
                    RoomNumber = a.Room!.RoomNumber,
                    HostelName = a.Room.Hostel!.Name,
                    HostelType = a.Room.Hostel.Type,
                    a.AllotmentDate,
                    a.FeeStatus
                })
                .FirstOrDefaultAsync();

            if (allotment == null)
                return Ok(new { message = "Koi active hostel allotment nathi." });

            return Ok(allotment);
        }

        // ── GET: api/Hostel/occupancy ── Admin occupancy summary
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("occupancy")]
        public async Task<IActionResult> GetOccupancy()
        {
            var summary = await _context.Hostels
                .Include(h => h.Rooms)
                .Select(h => new
                {
                    h.HostelId,
                    h.Name,
                    h.Type,
                    TotalCapacity = h.Rooms.Sum(r => r.Capacity),
                    TotalOccupied = h.Rooms.Sum(r => r.OccupiedCount),
                    Rooms = h.Rooms.Select(r => new
                    {
                        r.RoomId,
                        r.RoomNumber,
                        r.Capacity,
                        r.OccupiedCount,
                        Available = r.Capacity - r.OccupiedCount
                    }).OrderBy(r => r.RoomNumber).ToList()
                })
                .ToListAsync();

            return Ok(summary);
        }
    }
}
