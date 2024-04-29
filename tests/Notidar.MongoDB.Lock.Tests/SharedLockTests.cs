using FluentAssertions;

namespace Notidar.MongoDB.Lock.Tests
{
    public class SharedLockTests : BaseProviderTests
    {
        [Fact]
        [Trait("Category", "Manual")]
        public async Task LockProvider_NormalizeResource_NotFound()
        {
            var resource = await LockProvider.GetResourceAsync("resource1");
            resource.Should().BeNull();
        }

        //[Fact]
        //public async Task LockProvider_SoftLock_Success()
        //{
        //    var resource = await LockProvider.SharedLockAsync("resource1", "lock1");
        //    resource.Should().NotBeNull();
        //}

        //[Fact]
        //public async Task LockProvider_SoftLock_DoubleLock_Failure()
        //{
        //    var resource1 = await LockProvider.SharedLockAsync("resource1", "lock1");
        //    var resource2 = await LockProvider.SharedLockAsync("resource1", "lock1");
        //    resource1.Should().NotBeNull();
        //    resource2.Should().BeNull();
        //}
    }
}