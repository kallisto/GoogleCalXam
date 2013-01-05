using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Globalization;
using GEvents;

namespace Events.Web.Site.Controllers.Home
{
    public class HomeController : Controller
    {
        //
        // GET: /Event/

        public ActionResult Index(int limit = 5)
        {
            try
            {
                WebClient client = new WebClient();
                Uri calendar = new Uri("http://www.google.com/calendar/feeds/xamarin.com_i2oirq9nbe232t657d0kk8e43o@group.calendar.google.com/public/full?alt=json&orderby=starttime&max-results=" + limit + "&singleevents=true&sortorder=ascending&futureevents=true");
                using (Stream rawStream = client.OpenRead(calendar))
                using (BufferedStream stream = new BufferedStream(rawStream))
                {
                    var parsed = JsonReader.Read(stream);
                    List<Event> events = new List<Event>();
                    if (parsed.feed.entry != null)
                    {
                        foreach (var entry in parsed.feed.entry)
                        {
                            Event evnt = new Event();
                            //...get: title
                            IDictionary<string, object> title = (IDictionary<string, object>)entry.title;
                            object titleStr;
                            if (title.TryGetValue("$t", out titleStr))
                            {
                                evnt.title = (string)titleStr;
                            }

                            //...get: description
                            IDictionary<string, object> content = (IDictionary<string, object>)entry.content;
                            object contentStr;
                            if (content.TryGetValue("$t", out contentStr))
                            {
                                evnt.description = (string)contentStr;
                            }

                            //...get: date & location
                            IDictionary<string, object> ent = (IDictionary<string, object>)entry;
                            object when;
                            object where;
                            object start;
                            object end;
                            object location;

                            //...NOTE: the dictionaries here is kind of ugly, I had to do it becuase of 
                            //...the $ in the variable names. You could probably do some regex to get rid
                            //...of the bad characters first and then use is like a proper dynamic object
                            if (ent.TryGetValue("gd$where", out where))
                            {
                                List<object> whereList = (List<object>)where;
                                IDictionary<string, object> _where = (IDictionary<string, object>)whereList[0];
                                if (_where.TryGetValue("valueString", out location))
                                {
                                    evnt.locationWithTimeZone = (string)location;
                                }
                            }

                            if (ent.TryGetValue("gd$when", out when))
                            {
                                List<object> whenList = (List<object>)when;
                                IDictionary<string, object> _when = (IDictionary<string, object>)whenList[0];
                                if (_when.TryGetValue("endTime", out end) && (_when.TryGetValue("startTime", out start)))
                                {
                                    evnt.endtime =
                                    DateTime.Parse(
                                        (string)end,
                                        null,
                                        DateTimeStyles.RoundtripKind)
                                            .ToUniversalTime();

                                    evnt.starttime =
                                    DateTime.Parse(
                                        (string)start,
                                        null,
                                        DateTimeStyles.RoundtripKind)
                                            .ToUniversalTime();
                                }
                            }

                            events.Add(encodeLocation(evnt));
                        }
                    }
                    return View(events);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //...here we look up the local time for location, and add the time zone
        Event encodeLocation(Event evnt)
        {
            WebClient client = new WebClient();
            var api_key = "f725da505b163533122008";
            Uri request = new Uri("http://www.worldweatheronline.com/feed/tz.ashx?key=" + api_key + "&format=json&q=" + evnt.location);

            using (Stream rawStream = client.OpenRead(request))
            using (BufferedStream stream = new BufferedStream(rawStream))
            {
                try
                {
                    //..if location is real, get time zone from location
                    var parsed = JsonReader.Read(stream);
                    evnt.starttime = evnt.starttime.AddHours(Convert.ToDouble(parsed.data.time_zone[0].utcOffset));
                    evnt.endtime = evnt.endtime.AddHours(Convert.ToDouble(parsed.data.time_zone[0].utcOffset));
                }
                catch
                {
                    //...if location is unreal, then do nothing
                }
                return evnt;
            }
        } 


    }
}
