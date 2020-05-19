using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using LightestNight.System.EventSourcing.Checkpoints;
using LightestNight.System.Utilities.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LightestNight.System.EventSourcing.SqlStreamStore.MsSql.Checkpoints
{
    public class MsSqlCheckpointManager : ICheckpointManager
    {
        private readonly Func<SqlConnection> _createConnection;
        private readonly MsSqlEventSourcingOptions _options;
        private readonly Scripts.Scripts _scripts;
        private readonly ILogger<MsSqlCheckpointManager> _logger;

        public MsSqlCheckpointManager(IOptions<MsSqlEventSourcingOptions> options, ILogger<MsSqlCheckpointManager> logger)
        {
            _options = options.ThrowIfNull(nameof(options)).Value;
            _logger = logger.ThrowIfNull(nameof(logger));
            _scripts = new Scripts.Scripts(_options.Schema);

            _createConnection = () => new SqlConnection(_options.ConnectionString);
        }
        
        [SuppressMessage("ReSharper", "CA2100")]
        public async Task<int?> GetCheckpoint(string checkpointId, CancellationToken cancellationToken = new CancellationToken())
        {
            _logger.LogTrace(new EventId(3, "Get Checkpoint"), $"Getting Checkpoint with Id '{checkpointId}'");
            await using var connection = _createConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var command = new SqlCommand(_scripts.GetCheckpoint, connection)
            {
                CommandTimeout = _options.CommandTimeout
            };
            command.Parameters.Add(new SqlParameter("@CheckpointId", SqlDbType.NVarChar, 500)
            {
                Value = checkpointId
            });

            return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) as int?;
        }

        [SuppressMessage("ReSharper", "CA2100")]
        public async Task<long?> GetGlobalCheckpoint(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace(new EventId(3, "Get Checkpoint"), $"Getting Checkpoint with Id '{Constants.GlobalCheckpointId}'");
            await using var connection = _createConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var command = new SqlCommand(_scripts.GetCheckpoint, connection)
            {
                CommandTimeout = _options.CommandTimeout
            };
            command.Parameters.Add(new SqlParameter("@CheckpointId", SqlDbType.NVarChar, 500)
            {
                Value = Constants.GlobalCheckpointId
            });

            return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) as long?;
        }

        [SuppressMessage("ReSharper", "CA2100")]
        public async Task SetCheckpoint<TCheckpoint>(string checkpointId, TCheckpoint checkpoint, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace(new EventId(2, "Set Checkpoint"), $"Setting Checkpoint with Id '{checkpointId}' and Checkpoint '{checkpoint}'");
            await using var connection = _createConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var command = new SqlCommand(_scripts.SetCheckpoint, connection)
            {
                CommandTimeout = _options.CommandTimeout
            };
            command.Parameters.Add(new SqlParameter("@CheckpointId", SqlDbType.NVarChar, 500)
            {
                Value = checkpointId
            });
            command.Parameters.AddWithValue("@Checkpoint", checkpoint);

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a schema that will hold checkpoints, if the schema does not exist.
        /// Calls to this should be part of an application's deployment/upgrade process and
        /// not every time your application boots up.
        /// </summary>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> used to marshall the operation</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "CA2100")]
        public async Task CreateSchemaIfNotExists(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace(new EventId(1, "Create Schema"), "Creating Checkpoint Schema");
            await using var connection = _createConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            if (_options.Schema != "dbo")
            {
                await using var command = new SqlCommand($@"
IF NOT EXISTS (
SELECT schema_name
FROM   information_schema.schemata
WHERE  schema_name = '{_options.Schema}' )

BEGIN
  EXEC sp_executesql N'CREATE SCHEMA {_options.Schema}'
END", connection)
                {
                    CommandTimeout = _options.CommandTimeout
                };
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (var command = new SqlCommand(_scripts.CreateSchema, connection))
            {
                command.CommandTimeout = _options.CommandTimeout;
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}