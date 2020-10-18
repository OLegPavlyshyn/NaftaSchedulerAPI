using System;

namespace NaftaScheduler
{
    class UserConfig
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string GroupId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}