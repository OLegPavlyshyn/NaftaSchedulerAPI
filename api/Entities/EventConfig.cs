using System;

namespace NaftaScheduler
{
    class EventConfig
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public string summary { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool toProcess { get; set; } = true;
        public string gcEventId { get; set; } = null;
        public bool IsActive { get; set; } = true;
        public static bool Compare(EventConfig sEven, EventConfig tEvent)
        {
            bool result = true;
            if (sEven.summary != tEvent.summary) result = false;
            if (sEven.description != tEvent.description) result = false;
            if (sEven.location != tEvent.location) result = false;
            if (sEven.startDate != tEvent.startDate) result = false;
            if (sEven.endDate != tEvent.endDate) result = false;
            return result;
        }

        public override string ToString() =>
            "{"
            + $"\n\r\tid: {this.id},"
            + $"\n\r\tsummary: {this.summary},"
            + $"\n\r\tdescription: {this.description},"
            + $"\n\r\tlocation: {this.location},"
            + $"\n\r\tstartDate: {this.startDate},"
            + $"\n\r\tendDate: {this.endDate},"
            + $"\n\r\tIsActive: {this.IsActive},"
            + "\n\r}";
    }
}