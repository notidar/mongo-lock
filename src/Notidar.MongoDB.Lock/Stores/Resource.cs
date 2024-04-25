using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Notidar.MongoDB.Lock.Stores
{
    public sealed record Resource
    {
        [BsonId]
        public required string ResourceId { get; set; }
        public ExclusiveLock? ExclusiveLock { get; set; }
        public SharedLock[]? SharedLocks { get; set; }
        public int InfiniteLockCount { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTimeOffset? MaxExpiration { get; set; }
    }

    public sealed record SharedLock
    {
        public required string LockId { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTimeOffset? Expiration { get; set; }
    }

    public sealed record ExclusiveLock
    {
        public required string LockId { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTimeOffset? Expiration { get; set; }
    }
}
