using System;

namespace Notidar.MongoDB.Lock.Managers
{
    public sealed class LockOptions
    {
        public TimeSpan LockRetryDelay { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan LockExpiration { get; set; } = TimeSpan.FromSeconds(180);
        public TimeSpan LockRenewalPeriod { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public TimeSpan ClockSafePeriod { get; set; } = TimeSpan.FromSeconds(10);
        public int MaxSharedLocksPerResource { get; set; } = int.MaxValue;
    }
}
