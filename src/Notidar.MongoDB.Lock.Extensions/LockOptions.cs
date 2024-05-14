using Notidar.MongoDB.Lock.Services;

namespace Notidar.MongoDB.Lock.Extensions
{
    public sealed class LockOptions : LockSettings
    {
        /// <summary>
        /// Optional. Collection name to use. Default is "locks" defined in <see cref="Constants.DefaultCollectionName"/>
        /// </summary>
        public string? CollectionName { get; set; } = Constants.DefaultCollectionName;
        /// <summary>
        /// Optional. Flag to enable or disable cleanup index. Default is false.
        /// </summary>
        public bool CleanupEnabled { get; set; } = false;
    }
}
