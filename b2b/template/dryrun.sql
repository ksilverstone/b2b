-- Compile-only dry run for template/script.sql
-- This will compile statements and report errors without executing any changes.
-- Make sure to target the correct database using the sqlcmd -d parameter.

SET NOEXEC ON;
GO

:r .\template\script.sql
GO

SET NOEXEC OFF;
GO



