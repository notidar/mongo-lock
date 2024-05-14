using FluentAssertions;

namespace Notidar.MongoDB.Lock.Tests
{
    public class ExclusiveLockTests : BaseProviderTests
    {
        [Fact]
        [Trait("Category", "Manual")]
        public async Task ExclusiveUnlockAsync_WhenResourceMissing_ReturnsNull()
        {
            var resource = await LockProvider.ExclusiveUnlockAsync("1", "1");
            resource.Should().BeNull();
        }

        //[Fact]
        //public async Task ExclusiveLockAsync_WithInfiniteAndBlock_WhenResourceMissing_ReturnsLockedResource()
        //{
        //    var resourceId = "1";
        //    var lockId = "2";
        //    var resource = await LockProvider.ExclusiveLockAsync(resourceId, lockId);
        //    resource.Should().NotBeNull();
        //    resource!.ResourceId.Should().Be(resourceId);
        //    resource!.InfiniteLockCount.Should().Be(1);
        //    resource!.MaxExpiration.Should().BeNull();
        //    resource!.SharedLocks.Should().BeNullOrEmpty();
        //    resource!.ExclusiveLock.Should().NotBeNull();
        //    resource!.ExclusiveLock!.LockId.Should().Be(lockId);
        //    resource!.ExclusiveLock!.Expiration.Should().BeNull();
        //}

        //[Fact]
        //public async Task ExclusiveLockAsync_WithInfiniteAndBlock_WhenAlreadyAnotherExclusiveLocked_ReturnsNull()
        //{
        //    var resourceId = "1";
        //    var existingLockId = "1";
        //    var lockId = "2";
        //    var _ = await LockProvider.ExclusiveLockAsync(resourceId, existingLockId);
        //    var resource = await LockProvider.ExclusiveLockAsync(resourceId, lockId);
        //    resource.Should().BeNull();
        //}

        [Fact]
        [Trait("Category", "Manual")]
        public async Task ExclusiveLockAsync_WithExpirationAndBlock_WhenResourceMissing_ReturnsLockedResource()
        {
            var expiration = TimeSpan.FromSeconds(30);
            var expirationDateTime = FakeTimeProvider.GetUtcNow().Add(expiration);
            var resourceId = "1";
            var lockId = "2";
            var resource = await LockProvider.ExclusiveLockAsync(resourceId, lockId, expiration);
            resource.Should().NotBeNull();
            resource!.ResourceId.Should().Be(resourceId);
            resource!.InfiniteLockCount.Should().Be(0);
            resource!.MaxExpiration.Should().BeCloseTo(expirationDateTime, Precision);
            resource!.SharedLocks.Should().BeNullOrEmpty();
            resource!.ExclusiveLock.Should().NotBeNull();
            resource!.ExclusiveLock!.LockId.Should().Be(lockId);
            resource!.ExclusiveLock!.Expiration.Should().BeCloseTo(expirationDateTime, Precision);
        }

        //[Fact]
        //public async Task ExclusiveUnlockAsync_WithInfiniteAndBlock_WhenAlreadyAnotherExclusiveLocked_ReturnsNull()
        //{
        //    var resourceId = "1";
        //    var existingLockId = "1";
        //    var lockId = "2";
        //    var _ = await LockProvider.ExclusiveLockAsync(resourceId, existingLockId);
        //    var resource = await LockProvider.ExclusiveUnlockAsync(resourceId, lockId);
        //    resource.Should().BeNull();
        //}
    }
}