using System.Collections.Generic;
using Google.Apis.Calendar.v3.Data;


namespace NaftaScheduler
{
    class GCEventGenerator
    {
        public static Event GenerateEvent(EventConfig e, List<UserConfig> users = null)
        {
            Event config = new Event
            {
                Summary = e.summary,
                Location = e.location,
                Start = new EventDateTime()
                {
                    DateTime = e.startDate,
                    TimeZone = "Europe/Kiev"
                },
                End = new EventDateTime()
                {
                    DateTime = e.endDate,
                    TimeZone = "Europe/Kiev"
                },
            };
            if (users != null)
            {
                List<EventAttendee> attendees = new List<EventAttendee>();
                foreach (var user in users)
                {
                    attendees.Add(new EventAttendee() { Email = user.Name });
                }
                config.Attendees = attendees;
            }
            return config;
        }
    }
}