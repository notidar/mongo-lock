namespace Notidar.MongoDB.Lock.Extensions
{
    public sealed class DatabaseOptions
    {
        /// <summary>
        /// MongoDB connection string.
        /// </summary>
        public string? ConnectionString { get; set; } = null;
        /// <summary>
        /// Optional database name to use. Overrides connection string database name.
        /// </summary>
        public string? DatabaseName { get; set; } = null;
    }
}
