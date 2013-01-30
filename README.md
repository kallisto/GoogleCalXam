Xalendar: a Google Calendar-Powered Events Widget Thingy
========================================================

The Xamarin Events Calendar engine takes json provided by our google events calendar, and transforms it into events on the Xamarin documentation homepage.

The calendar can be found [here](https://www.google.com/calendar/embed?src=xamarin.com_i2oirq9nbe232t657d0kk8e43o@group.calendar.google.com&ctz=America/Los_Angeles). You must be logged in with your xamarin account to view.

Adding events is uncomplicated, but you do have to keep a few things in mind, so we're going to go over them.

Links
-------
Google json only passes along the link to the calendar event itself, so any external link, like a link to a blog post or event website, has to be added manually. We do this in the event description, and add *s as delimiters. We put the link at the very end of the event description.

So like this:

Description: This is an awesome event! Everyone should come drink sake with Xamarin! * http://www.sakeisawesome.com *

Time Zonez
----------------
Okay. Time zones are a bit complicate so pay attention.

The Xamarin Events Calendar is set to GMT by default. Google Cal lets you display another time zone alongside this, so you can see the time in GMT and Pacific, or GMT and Eastern. This may be useful to you.

The biggest problem with the GoogleCal json is that it carries no time zone data. Like, absolutely none. Doesn't matter if we set a time zone for the event, or for the entire calendar, Google will ignore it and we will get everything as UTC/GMT. So we have two options to get the correct time for events. The first is to enter the time in GMT, and let the widget translate that to the correct time at the event location using the event location data specified. The second is to ignore time zones altogether and display the GMT time AS IF it were in the correct time zone. Let's do a quick example of both.

Here's how to set up an event happening in SF at 7:30pm PST.

 - Title: Sake Drinkup
 - When: April 23rd, 2:30am (GMT)
 - Where: San Francisco, CA
 - Description: This is an awesome event! Everyone should come drink sake with Xamarin! * http://www.sakeisawesome.com *

2:30am GMT == 7:30pm PST, and the calendar, knowing that this is happening in SF, will translate the GMT time into Pacific.

Here's the other option, where we use the GMT time as the absolute time and don't provide a location:

 - Title: Sake Webinar
 - When: April 23rd, 7:30pm (GMT)
 - Where: online seminar
 - Description: Learn all about sake from Bryan Costanich * http://www.sakeisawesome.com *

This event will simply display as happening at 7:30pm.

The widget can recognize all-day and multi-day events and display them just fine, so feel free to add those in. Happy calendar editing! Hope it's eventful.

Authors
----------
Nina Vyedin (nina.vyedin@xamarin.com)
