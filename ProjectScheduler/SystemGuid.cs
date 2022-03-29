using System;

namespace ProjectScheduler
{
    public class SystemGuid : IGuid
    {
        public Guid id => Guid.NewGuid();
    }
}
