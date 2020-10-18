using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace NaftaScheduler
{
    class Calendar
    {
        private string[] Scopes = { CalendarService.Scope.Calendar };
        private string ApplicationName = "IFNTUONG Scheduler";
        private UserCredential Credential;
        public CalendarService Service;
        public Calendar(string appDir)
        {
            using (var stream =
                new FileStream($"{appDir}/configs/Credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = $"{appDir}/configs/token.json";
                Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Calendar API service.
            Service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = ApplicationName,
            });
        }
        public string CreateEvent(EventConfig Event, List<UserConfig> Users)
        {
            Event e = GCEventGenerator.GenerateEvent(Event, Users);
            Event createdEvent = Service.Events.Insert(e, "primary").Execute();
            return createdEvent.Id;
        }
        public Event getEvent(string id)
        {
            var response = this.Service.Events.Get("primary", id).Execute();
            return response;
        }
        public void AttendUser(string id, UserConfig user)
        {
            var e = this.getEvent(id);
            e.Attendees.Add(new EventAttendee() { Email = user.Name });
            this.Service.Events.Patch(e, "primary", id).Execute();
        }
        public void UpdateEvent(string id, EventConfig e)
        {
            Event config = GCEventGenerator.GenerateEvent(e);
            this.Service.Events.Patch(config, "primary", id).Execute();
        }
        public void DeleteEvent(string id)
        {
            this.Service.Events.Delete("primary", id).Execute();
        }
    }
}