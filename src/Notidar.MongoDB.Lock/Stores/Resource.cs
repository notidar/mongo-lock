using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Notidar.MongoDB.Lock.Stores
{
    public sealed record Resource
    {
        [BsonId]
        public string? ResourceId { get; set; }
        public ExclusiveLock? ExclusiveLock { get; set; }
        public SharedLock[]? SharedLocks { get; set; }
        public int InfiniteLockCount { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTimeOffset? MaxExpiration { get; set; }
    }

    public sealed record SharedLock
    {
        public string? LockId { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTimeOffset? Expiration { get; set; }
    }

    public sealed record ExclusiveLock
    {
        public string? LockId { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTimeOffset? Expiration { get; set; }
    }
}
