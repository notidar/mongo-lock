using System;
using System.Threading;

namespace Notidar.MongoDB.Lock.Services
{
    public interface ILock : IAsyncDisposable
    {
        public CancellationToken HealthToken { get; }
    }
}
