using System;

namespace ProjectScheduler
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}
