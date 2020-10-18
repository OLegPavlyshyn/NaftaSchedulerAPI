using System;

namespace NaftaScheduler
{
    class UserConfig
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string GroupId { get; set; }

        public override string ToString() => $"Id: {this.id} \n\r Name: {this.Name} GroupId: {this.GroupId};";
    }
}