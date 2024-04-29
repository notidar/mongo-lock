using System;
using System.Threading;

namespace Notidar.MongoDB.Lock.Managers
{
    public interface ILock : IAsyncDisposable
    {
        public CancellationToken HealthToken { get; }
    }
}
