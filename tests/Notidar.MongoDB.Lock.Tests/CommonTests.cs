using FluentAssertions;

namespace Notidar.MongoDB.Lock.Tests
{
    public class CommonTests : BaseProviderTests
    {
        [Fact]
        [Trait("Category", "Manual")]
        public async Task GetResourceAsync_WhenResourceMissing_ReturnsNull()
        {
            var resource = await LockProvider.GetResourceAsync("1");
            resource.Should().BeNull();
        }
    }
}