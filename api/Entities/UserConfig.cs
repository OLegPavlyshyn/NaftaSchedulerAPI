using System;

namespace NaftaScheduler
{
    class UserConfig
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string groupID { get; set; }

        public override string ToString() => $"Id: {this.id} \n\r Name: {this.Name} groupID: {this.groupID};";
    }
}