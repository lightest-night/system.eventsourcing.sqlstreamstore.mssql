DECLARE @DBName sysname;
SET @DBName = (SELECT db_name());
DECLARE @SQL nvarchar(max);
SET @SQL = 'ALTER DATABASE [' + @DBName + '] SET ALLOW_SNAPSHOT_ISOLATION ON; ALTER DATABASE [' + @DBName + '] SET READ_COMMITTED_SNAPSHOT ON;';
exec(@SQL)

IF OBJECT_ID('dbo.Checkpoints', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[Checkpoints](
            Id              BIGINT  IDENTITY(1,1)   NOT NULL,
            CheckpointId    NVARCHAR(500)           NOT NULL,
            [Checkpoint]    BIGINT
            CONSTRAINT PK_Checkpoints PRIMARY KEY CLUSTERED (Id)
        );
    END

IF NOT EXISTS(
    SELECT *
    FROM sys.indexes
    WHERE [name] = 'IX_Checkpoints_CheckpointId' AND [object_id] = OBJECT_ID('dbo.Checkpoints', 'U'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Checkpoints_CheckpointId ON dbo.Checkpoints (CheckpointId);
END

BEGIN
    IF NOT EXISTS (SELECT NULL FROM SYS.EXTENDED_PROPERTIES WHERE [major_id] = OBJECT_ID('dbo.Checkpoints') AND [name] = N'version' AND [minor_id] = 0)
    EXEC sys.sp_addextendedproperty
    @name = N'version',
    @value = N'1',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE', @level1name = 'Checkpoints';
END