SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRAN
    
    IF EXISTS ( SELECT [CheckpointId] FROM [dbo].[Checkpoints] WITH (UPDLOCK) WHERE [CheckpointId] = @CheckpointId)
        UPDATE [dbo].[Checkpoints]
        SET [Checkpoint] = @Checkpoint
        WHERE [CheckpointId] = @CheckpointId;
    ELSE
        INSERT INTO [dbo].[Checkpoints] ([CheckpointId], [Checkpoint])
        VALUES (@CheckpointId, @Checkpoint);
        
COMMIT