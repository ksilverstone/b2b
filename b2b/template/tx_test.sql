-- Transactional test run for template/script.sql
-- Executes inside an explicit transaction and then rolls back.
-- Use for verifying runtime errors without persisting changes.

BEGIN TRANSACTION;
BEGIN TRY
    :r .\template\script.sql
    PRINT 'Script executed successfully; rolling back as this is a test.';
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    PRINT 'Error number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(20));
    PRINT 'Message: ' + ERROR_MESSAGE();
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR(20));
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW; -- rethrow to surface error to caller
END CATCH;



