-- ============================================================
-- Script  : PasswordResetOtps_Create.sql
-- Purpose : Create the OTP table for Forgot Password feature
-- Run On  : CollegeManagement database
-- NOTE    : No EF Migration — run manually in SSMS / Azure DS
-- ============================================================

USE [CollegeManagement];
GO

-- ── Safety check: only create if table does not exist ──────
IF NOT EXISTS (
    SELECT 1
    FROM   INFORMATION_SCHEMA.TABLES
    WHERE  TABLE_SCHEMA = 'dbo'
      AND  TABLE_NAME   = 'PasswordResetOtps'
)
BEGIN
    CREATE TABLE [dbo].[PasswordResetOtps]
    (
        -- Primary key
        [OtpId]     INT          IDENTITY(1,1) NOT NULL,

        -- FK → Users table
        [UserId]    INT          NOT NULL,

        -- 6-digit numeric OTP stored as string
        [OtpCode]   NVARCHAR(6)  NOT NULL,

        -- Timestamps (UTC)
        [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_PasswordResetOtps_CreatedAt] DEFAULT (GETUTCDATE()),
        [ExpiresAt] DATETIME2(7) NOT NULL,

        -- One-time-use flag; set to 1 immediately after successful verification
        [IsUsed]    BIT          NOT NULL CONSTRAINT [DF_PasswordResetOtps_IsUsed]    DEFAULT (0),

        -- ── Constraints ──────────────────────────────────────
        CONSTRAINT [PK_PasswordResetOtps]
            PRIMARY KEY CLUSTERED ([OtpId] ASC),

        CONSTRAINT [FK_PasswordResetOtps_Users]
            FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users] ([UserId])
            ON DELETE CASCADE
    );

    -- Index on UserId for fast per-user lookups
    CREATE NONCLUSTERED INDEX [IX_PasswordResetOtps_UserId]
        ON [dbo].[PasswordResetOtps] ([UserId] ASC);

    -- Index on OtpCode for fast OTP verification lookups
    CREATE NONCLUSTERED INDEX [IX_PasswordResetOtps_OtpCode]
        ON [dbo].[PasswordResetOtps] ([OtpCode] ASC);

    PRINT 'Table [PasswordResetOtps] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [PasswordResetOtps] already exists — skipped.';
END
GO
