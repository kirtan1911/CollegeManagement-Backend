/* glass-theme.js — Shared utility functions for College Management System Glass Dashboard */
/* Include after jQuery but before page-specific scripts */

const API_BASE = 'https://localhost:7142/api';
const FILE_BASE = 'https://localhost:7142';

// ── Auth helpers ──────────────────────────────────────────────────────
function getToken()  { return localStorage.getItem('token'); }
function getRole()   { return localStorage.getItem('role'); }
function getUser()   { return localStorage.getItem('user') || 'User'; }

function authHeader() {
  return { 'Authorization': 'Bearer ' + getToken(), 'Content-Type': 'application/json' };
}

function authHeaderNoContentType() {
  return { 'Authorization': 'Bearer ' + getToken() };
}

function requireAuth(allowedRoles) {
  const token = getToken();
  const role  = getRole();
  if (!token || (allowedRoles && !allowedRoles.includes(role))) {
    localStorage.clear();
    location.href = 'Login.html';
    return false;
  }
  return true;
}

function logout() { localStorage.clear(); location.href = 'Login.html'; }

// ── Toast ─────────────────────────────────────────────────────────────
let _toastContainer = null;

function _ensureToastContainer() {
  if (!_toastContainer) {
    _toastContainer = document.createElement('div');
    _toastContainer.className = 'glass-toast-container';
    document.body.appendChild(_toastContainer);
  }
  return _toastContainer;
}

function toast(msg, type = 'success', duration = 3000) {
  const container = _ensureToastContainer();
  const icons = { success: '✅', error: '❌', info: 'ℹ️', warn: '⚠️' };
  const classMap = { success: 'toast-success', error: 'toast-error', info: 'toast-info', warn: 'toast-warn' };

  const el = document.createElement('div');
  el.className = `glass-toast ${classMap[type] || 'toast-info'}`;
  el.innerHTML = `<span style="font-size:15px;flex-shrink:0">${icons[type] || '🔔'}</span><span>${msg}</span>`;
  container.appendChild(el);

  setTimeout(() => {
    el.style.animation = 'toastFadeOut 0.35s ease forwards';
    setTimeout(() => el.remove(), 360);
  }, duration);
}

// ── Glass Badge HTML ──────────────────────────────────────────────────
function badgeHtml(status, opts = {}) {
  const statusMap = {
    'Paid':         'badge-success',
    'Unpaid':       'badge-danger',
    'Pending':      'badge-warning',
    'Approved':     'badge-success',
    'Rejected':     'badge-danger',
    'Applied':      'badge-info',
    'Shortlisted':  'badge-orange',
    'Selected':     'badge-success',
    'Pass':         'badge-success',
    'Fail':         'badge-danger',
    'Active':       'badge-success',
    'Inactive':     'badge-muted',
  };
  const cls = opts.cls || statusMap[status] || 'badge-muted';
  const icon = opts.icon || '';
  return `<span class="glass-badge ${cls}">${icon}${status}</span>`;
}

// ── Grade pill HTML ───────────────────────────────────────────────────
function gradePill(grade) {
  const map = {
    'A+': 'grade-a', 'A': 'grade-a',
    'B+': 'grade-b', 'B': 'grade-b',
    'C+': 'grade-c', 'C': 'grade-c',
    'Fail': 'grade-f'
  };
  const cls = map[grade] || 'grade-c';
  return `<span class="glass-badge ${cls}">${grade}</span>`;
}

// ── Confirm Modal using Bootstrap ─────────────────────────────────────
function glassConfirm(title, body, onConfirm, opts = {}) {
  // Remove existing confirm modal if any
  const existing = document.getElementById('_glassConfirmModal');
  if (existing) existing.remove();

  const btnLabel = opts.btnLabel || 'Confirm';
  const btnCls   = opts.danger   ? 'glass-btn-danger' : 'glass-btn-primary';

  const html = `
  <div class="modal fade" id="_glassConfirmModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered modal-sm">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">${title}</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body"><p style="color:var(--text-muted);font-size:14px;margin:0">${body}</p></div>
        <div class="modal-footer">
          <button class="glass-btn-secondary glass-btn-sm" data-bs-dismiss="modal">Cancel</button>
          <button class="${btnCls} glass-btn-sm" id="_glassConfirmBtn">${btnLabel}</button>
        </div>
      </div>
    </div>
  </div>`;
  document.body.insertAdjacentHTML('beforeend', html);
  const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('_glassConfirmModal'));
  document.getElementById('_glassConfirmBtn').onclick = () => { modal.hide(); onConfirm(); };
  modal.show();
}

// ── Empty State HTML ──────────────────────────────────────────────────
function emptyStateHtml(icon = '📭', title = 'No data yet', sub = '') {
  return `
  <div class="glass-empty">
    <div class="empty-icon">${icon}</div>
    <div class="empty-title">${title}</div>
    ${sub ? `<div class="empty-sub">${sub}</div>` : ''}
  </div>`;
}

// ── Table empty row helper ────────────────────────────────────────────
function emptyTableRow(cols, icon, msg) {
  return `<tr><td colspan="${cols}"><div class="glass-empty" style="padding:40px 20px"><div class="empty-icon">${icon}</div><div class="empty-title">${msg}</div></div></td></tr>`;
}

// ── Date formatter ────────────────────────────────────────────────────
function fmtDate(iso, opts = { day: '2-digit', month: 'short', year: 'numeric' }) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('en-IN', opts);
}

function fmtDateTime(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('en-IN', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

// ── Sidebar avatar initial ────────────────────────────────────────────
function setAvatar(selector) {
  const u = getUser();
  $(selector).text(u.charAt(0).toUpperCase());
}

// ── Sidebar active link setter ────────────────────────────────────────
function setSidebarActive(href) {
  $(`.sb-item[href="${href}"]`).addClass('active');
}

// ── Init sidebar user display ─────────────────────────────────────────
function initSidebar() {
  const user = getUser();
  const role = getRole();
  const letter = user.charAt(0).toUpperCase();
  const avatarEl = document.getElementById('sbAvatar');
  if (avatarEl) {
    avatarEl.textContent = letter;
    avatarEl.title = `${user} (${role})`;
  }
}

// Run init when DOM ready
document.addEventListener('DOMContentLoaded', () => {
  initSidebar();

  // Hijack focus-trap aria errors from Bootstrap modals
  document.querySelectorAll('.modal').forEach(m => {
    m.addEventListener('hide.bs.modal', () => {
      if (document.activeElement && m.contains(document.activeElement)) {
        document.activeElement.blur();
      }
    });
  });
});
