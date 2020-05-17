namespace LightestNight.System.EventSourcing.SqlStreamStore.MSSql
{
    public class MsSqlEventSourcingOptions : EventSourcingOptions
    {
        /// <summary>
        /// Whether to create the database schema if it doesn't already exist
        /// </summary>
        public bool CreateSchemaIfNotExists { get; set; } = true;

        /// <summary>
        /// The SQL Server Connection String
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// The Microsoft Sql Server database schema to use
        /// </summary>
        public string Schema { get; set; } = "dbo";
    }
}