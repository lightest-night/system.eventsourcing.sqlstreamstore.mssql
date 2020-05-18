namespace LightestNight.System.EventSourcing.SqlStreamStore.MsSql
{
    public class MsSqlEventSourcingOptions : EventSourcingOptions
    {
        /// <summary>
        /// Whether to create the database schema if it doesn't already exist
        /// </summary>
        /// <remarks>Default: true</remarks>
        public bool CreateSchemaIfNotExists { get; set; } = true;

        /// <summary>
        /// The SQL Server Connection String
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// The Microsoft Sql Server database schema to use
        /// </summary>
        /// <remarks>Default: dbo</remarks>
        public string Schema { get; set; } = "dbo";

        /// <summary>
        /// The wait time to execute a command
        /// </summary>
        /// <remarks>Default: 30</remarks>
        public int CommandTimeout { get; set; } = 30;
    }
}