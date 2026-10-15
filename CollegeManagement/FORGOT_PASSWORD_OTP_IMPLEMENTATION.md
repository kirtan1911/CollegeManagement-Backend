# FORGOT_PASSWORD_OTP_IMPLEMENTATION.md

## Project

Existing ASP.NET Core Web API + HTML + JavaScript College Management System

---

# Objective

Implement **Forgot Password with Email OTP Verification** inside the existing project.

This is **NOT** a new project.

This feature must be integrated into the current codebase without breaking any existing functionality.

---

# Existing Project Information

## Backend

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server (Database already exists)
* No EF Migration is used.
* Database changes are done manually using SQL scripts.
* Passwords are already stored using **BCrypt Password Hashing**.
* Existing login works perfectly.
* JWT Authentication already implemented.

---

## Existing User Model

User table already contains

* UserId
* Name
* Email
* PasswordHash
* RoleId
* Status
* CreatedDate

Do NOT modify existing fields unless absolutely necessary.

---

## Existing Login Flow

Current Login works like this

Email

↓

Password

↓

BCrypt Verify

↓

JWT Token

↓

Redirect

This login flow must remain exactly the same.

Do NOT add OTP during Login.

Do NOT change Login API.

Do NOT break existing authentication.

---

# Required Feature

Only implement

Forgot Password using Email OTP.

Nothing else.

---

# Login Behaviour

Current Login must continue exactly like this

Admin

Faculty

Student

↓

Email + Password

↓

Login Successfully

↓

NO OTP REQUIRED

OTP must never be requested during Login.

---

# Forgot Password Flow

User clicks

Forgot Password

↓

Enter Registered Email

↓

Backend checks if email exists

↓

Generate 6 digit OTP

↓

Store OTP temporarily

↓

Send OTP to user's registered email using Gmail SMTP

↓

User enters OTP

↓

Backend verifies OTP

↓

If OTP valid

↓

Allow user to create new password

↓

Hash password using existing BCrypt logic

↓

Update PasswordHash

↓

Delete OTP / expire OTP

↓

Success

---

# Existing Email Service

Use existing EmailService.cs.

Do NOT rewrite it.

Reuse existing Gmail SMTP implementation.

Existing EmailSettings inside appsettings.json must continue to work.

---

# BCrypt

Project already uses BCrypt.

Use the same BCrypt implementation while changing password.

Never save plain password.

Never replace BCrypt.

---

# Database

Currently there is NO OTP table.

No migration is used.

Create required SQL script manually.

Generate SQL script for required table.

Include

Primary Key

Foreign Key if needed

Indexes if useful

Expiry Time

Created Time

Used flag

Everything required for production.

---

# OTP Requirements

Generate random 6 digit OTP.

OTP expires after 5 minutes.

OTP can be used only once.

After successful verification

OTP becomes invalid immediately.

Expired OTP cannot be reused.

---

# Resend OTP

Implement Resend OTP.

Old OTP must become invalid.

Generate completely new OTP.

Send new email.

---

# Security Rules

Do not reveal whether email exists or not through insecure responses.

Avoid OTP reuse.

Validate expiry.

Validate one-time use.

Hash password using existing BCrypt.

Never expose PasswordHash.

Never expose OTP in API response.

OTP must only be sent through Email.

---

# API Design

Create only required APIs.

Suggested endpoints

POST

/api/Auth/forgot-password

POST

/api/Auth/verify-otp

POST

/api/Auth/reset-password

POST

/api/Auth/resend-otp

Reuse existing AuthController whenever possible.

Do not create duplicate authentication logic.

---

# DTOs

Create only required DTO classes.

Keep naming consistent with existing project.

---

# Frontend

Existing Login.html already contains

Login

Register

Add only Forgot Password feature.

Do NOT redesign entire page.

Maintain same UI theme.

Maintain existing animations.

Maintain existing CSS.

---

# Forgot Password UI

Add

Forgot Password?

↓

Email

↓

Send OTP

↓

OTP Verification

↓

New Password

↓

Confirm Password

↓

Reset Password

Use existing alert system.

Use existing jQuery AJAX style.

Do not replace existing JavaScript structure.

---

# Validation

Email required

OTP required

Password required

Confirm Password required

Passwords must match

OTP expiry validation

Invalid OTP validation

Email validation

---

# Existing Project Rules

Very Important

Do NOT change

Existing Login API

Existing Register API

JWT logic

Role logic

Dashboard

Admin

Faculty

Student

Exam Module

Reports

Attendance

Courses

Departments

Notifications

Fees

Study Materials

Everything must continue working exactly the same.

Only extend project.

Do not refactor unrelated code.

---

# SQL Script

Generate complete SQL script separately.

Remember

Project does NOT use EF Migration.

Database is updated manually.

---

# File-wise Implementation

For every modification mention

File Location

File Name

Reason

What changed

Do not give only code.

Explain where every code block should be added.

---

# Testing Checklist

Include complete testing checklist.

Test

Forgot Password

Invalid Email

Invalid OTP

Expired OTP

Resend OTP

Password Reset

Login using new password

Old password should fail

Existing Login should still work

Register should still work

JWT Login should still work

Admin Login

Faculty Login

Student Login

Everything should remain functional.

---

# Important Instructions

Do not modify unrelated modules.

Reuse existing architecture.

Reuse existing EmailService.

Reuse existing BCrypt.

Reuse existing AuthController whenever possible.

Keep implementation clean.

Avoid duplicate code.

Follow current project coding style.

Generate production-ready implementation.
