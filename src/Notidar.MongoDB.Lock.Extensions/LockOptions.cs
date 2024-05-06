using Notidar.MongoDB.Lock.Services;

namespace Notidar.MongoDB.Lock.Extensions
{
    public sealed class LockOptions : LockSettings
    {
        /// <summary>
        /// Optional collection name to use. Default is "locks" defined <see cref="Constants.DefaultCollectionName"/>
        /// </summary>
        public string? CollectionName { get; set; } = Constants.DefaultCollectionName;
        public bool CollectionCleanup { get; set; } = false;
    }
}
