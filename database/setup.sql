-- Code-Room database setup
-- This script creates the empty MySQL database used by the ASP.NET Core application.
-- The application creates the tables and seeds the baseline content on first run.

CREATE DATABASE IF NOT EXISTS coderoom
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
