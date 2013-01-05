using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace GEvents
{
    public class Event
    {
        public Event()
        {
        }

        public string title { get; set; }
        public string description { get; set; }
        public string link
        {
            get
            {
                //...Google json doesn't provide us a link url, 
                //...so link is included at the bottom of the event description, 
                //...enclosed in **
                return Regex.Match(this.description, @"\*([^*]*)\*").Groups[1].Value;
            }
        }
        public string content
        {
            //...the content is everything before the ** that surrounds the link
            get
            {
                var index = this.description.IndexOf("*");
                var c = this.description;
                if(index > 0) {
                     c = this.description.Substring(0, index);
                }
                return c;
            }
        }
        
        //...100 char version of description, with ellipses
        public string summary 
        {
            get
            {
                var s = this.content;
                if (s.Length > 100)
                {
                    var regex = Regex.Match(s.Substring(100), @"^.*?(?= )");
                    s = s.Substring(0, 100) + regex + " ...";
                }
                return s;
            }
        }


        public string locationWithTimeZone { get; set; }
        public string location
        {
            //...Google does NOT provide time zone information, so we include
            //...the time zone in () after the location i.e. --> Boston, MA (EDT)
            get
            {
                var l = this.locationWithTimeZone;
                var index = l.IndexOf("(");
                if (index > 0)
                {
                    l = l.Substring(0, index);
                }
                return l;
            }
        }
        public string timezone
        {
            get
            {
                return Regex.Match(this.locationWithTimeZone, @"\(([^*]*)\)").Groups[1].Value;
            }
        }


        public DateTime starttime { get; set; }
        public DateTime endtime { get; set; }

        public string displaytime
        //...lots of string styling, hurrah
        {
            get
            {
                //...display a one-day all-day event
                if (daysLong() != 1 && !(daysLong() > 1))
                {
                    return this.starttime.ToString(
                        "@ " +
                        GetMinuteFormat(this.starttime) +
                        GetTT(this.starttime, this.endtime),
                        CultureInfo.CreateSpecificCulture("en")) +
                    this.endtime.ToString(
                        GetMinuteFormat(this.endtime) +
                        " tt ",
                        CultureInfo.CreateSpecificCulture("en")) +
                        formatTimezone(this.timezone);
                }
                else
                {
                    return formatTimezone(this.timezone);
                }
            }
        }

        public string month
        {
            get
            {
                return this.starttime.ToString(
                        "MMM",
                        CultureInfo.CreateSpecificCulture("en"));
            }
        }

        public string day
        {
            get
            {
                if (daysLong() > 1)
                {
                    return this.starttime.ToString(
                              "dd - ",
                              CultureInfo.CreateSpecificCulture("en")) +
                           this.endtime.ToString(
                              "dd",
                              CultureInfo.CreateSpecificCulture("en"));
                }
                else
                {
                    return this.starttime.ToString(
                        "dd",
                        CultureInfo.CreateSpecificCulture("en"));
                }
            }
        }


#region Event Helpers
        //...checks if an event is an all day event
        double daysLong()
        {
            return (this.endtime - this.starttime).TotalDays;
        }

        //...remove the minutes if on the hour
        string GetMinuteFormat(DateTime time)
        {
            return (time.Minute > 0) ? "h:mm" : "h";
        }

        //...remove first am/pm marker if unnecessary
        string GetTT(DateTime time1, DateTime time2)
        {
            return (time1.ToString("tt") == time2.ToString("tt") ? " - " : " tt - ");
        }

        string formatTimezone(string timezone)
        {
            return (timezone != string.Empty ? string.Format("({0})", this.timezone) : string.Empty);
        }

#endregion
    }
}