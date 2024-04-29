using Notidar.MongoDB.Lock.Managers;

namespace Notidar.MongoDB.Lock.Extensions
{
    public sealed class LockOptions : LockSettings
    {
        /// <summary>
        /// Optional collection name to use. Default is "locks" defined <see cref="Constants.DefaultCollectionName"/>
        /// </summary>
        public string? CollectionName { get; set; } = Constants.DefaultCollectionName;
    }
}
