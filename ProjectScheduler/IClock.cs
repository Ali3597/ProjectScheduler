using System;

namespace ProjectScheduler
{
    public interface IClock
    {
        public DateTime Now { get; }

        public static IClock Current { get; private set; } = new SystemClock();

        public static void SetTestClock(IClock clock)
        {
            Current = clock;
        }
    }
}
