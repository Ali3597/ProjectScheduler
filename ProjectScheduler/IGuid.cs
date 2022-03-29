using System;

namespace ProjectScheduler
{
    public interface IGuid
    {
        public Guid id { get; }

        public static IGuid Current { get; private set; } = new SystemGuid();

        public static void SetTestGuid(IGuid guid)
        {
            Current =guid;
        }
    }
}
