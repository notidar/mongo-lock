using System;

namespace Notidar.MongoDB.Lock.Services
{
    public class LockSettings
    {
        /// <summary>
        /// Optional. Delay between lock aquiring attempts. Default is 10 seconds.
        /// </summary>
        public TimeSpan LockRetryDelay { get; set; } = TimeSpan.FromSeconds(10);
        /// <summary>
        /// Optional. Delay for lock expiration if no renewal requested. Default is 180 seconds.
        /// </summary>
        public TimeSpan LockExpirationPeriod { get; set; } = TimeSpan.FromSeconds(180);
        /// <summary>
        /// Optional. Delay between lock renewal attempts. Default is 60 seconds.
        /// </summary>
        public TimeSpan LockRenewalPeriod { get; set; } = TimeSpan.FromSeconds(60);
        /// <summary>
        /// Optional. Database operation timeout. Default is 20 seconds.
        /// </summary>
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(20);
        /// <summary>
        /// Optional. Clock safe period to prevent desync because of system time delay. Default is 10 seconds.
        /// </summary>
        public TimeSpan ClockSafePeriod { get; set; } = TimeSpan.FromSeconds(10);
        /// <summary>
        /// Optional. Default maximum number of shared locks per resource. Default is unlimited and value is null.
        /// </summary>
        public int? MaxSharedLocksPerResource { get; set; } = null;
    }
}
