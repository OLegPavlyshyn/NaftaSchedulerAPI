using AngleSharp;
using System.Threading.Tasks;
using System.Linq;
using System;
using AngleSharp.Io;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NaftaScheduler
{
    class Parser
    {
        private static readonly string URL = "http://194.44.112.6/cgi-bin/timetable.cgi?n=700&group=3199";
        // private static readonly Dictionary<string, string> requestFields = new Dictionary<string, string>() { { "body", "faculty=0&teacher=&group=%CA%B2%EC-20-1&sdate=26.10.2020&edate=31.10.2020&n=700" } };

        public static Func<string, string, DateTime> concatDate = (date, time) =>
            DateTime.ParseExact(date + time, "dd.MM.yyyyHH:mm", null);
        public static async Task<List<EventConfig>> GetEvents()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            DocumentRequest req = DocumentRequest.Get(new Url(URL));
            var document = await context.OpenAsync(req);
            var cells = document.QuerySelectorAll("td");
            var dates = document.QuerySelectorAll(".col-md-6 > h4");
            // setuping indexes to look up every third column      
            int length = cells.Length / 3;
            int[] indexes = new int[length];
            for (int i = 1; i <= length; i++)
            {
                indexes[i - 1] = i * 3 - 1;
            }
            // end
            var eventList = new List<EventConfig>();
            int c = 0;
            string date = "";

            foreach (int j in indexes)
            {
                if (cells[j - 2].TextContent == "1")
                {
                    date = dates[c].TextContent.Split(' ')[0];
                    c++;
                }
                if (cells[j].TextContent != " ")
                {
                    EventConfig e = new EventConfig();
                    List<string> classData = cells[j].InnerHtml.Split("<br>").ToList();
                    Regex regex = new Regex(@"(підгр. [0-9])");
                    Regex urlx = new Regex(@"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)");
                    if (classData.Exists(c => regex.Match(c).Success))
                    {
                        int index = classData.FindIndex(c => new Regex(@"(підгр. 2)").Match(c).Success);
                        if (index != -1)
                        {
                            e.summary = classData[index - 1];
                            e.description = $"{classData[index - 1].Trim(' ')} {classData[index].Trim(' ')}\n\r{classData[index + 1].Trim(' ')}";
                            var eventURL = urlx.Match($"{classData[index + 2]} {classData[classData.Count - 1]}");
                            e.location = eventURL.Success ? eventURL.ToString() : "";
                        }
                        else { continue; }
                    }
                    else
                    {
                        int imgIndex = classData.FindIndex(c => new Regex("remote_work").IsMatch(c));
                        e.summary = classData[imgIndex + 1].Trim(' ');
                        e.description = $"{e.summary}\n\r{classData[imgIndex + 2].Trim(' ')}";
                        Match eventURL = urlx.Match($"{classData[imgIndex + 2]} {classData[classData.Count - 1]}");
                        e.location = eventURL.Success ? eventURL.ToString() : "";
                    }
                    e.startDate = concatDate(date, cells[j - 1].TextContent.Substring(0, 5));
                    e.endDate = concatDate(date, cells[j - 1].TextContent.Substring(5, 5));
                    eventList.Add(e);
                }
            }
            return eventList;
        }
    }
}