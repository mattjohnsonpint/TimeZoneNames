TimeZoneNames
=============

Provides localized time zone names using CLDR and TZDB sources.

**Nuget Installation**
```
PM> Install-Package TimeZoneNames
```

**Usage**

Look up the localized names for a specific time zone:
```
var names = TimeZoneNames.GetNamesForTimeZone("America/Los_Angeles", "en-US");

names.Generic == "Pacific Time"
names.Standard == "Pacific Standard Time"
names.Daylight == "Pacific Daylight Time"
```

```
var names = TimeZoneNames.GetNamesForTimeZone("America/Los_Angeles", "fr-CA");

names.Generic == "heure du Pacifique"
names.Standard == "heure normale du Pacifique"
names.Daylight == "heure avancée du Pacifique"
```

Look up the time zones for a specific country:
```
string[] zones = TimeZoneNames.GetTimeZoneIdsForCountry("AU");
```
*Output*
```
Australia/Lord_Howe
Antarctica/Macquarie
Australia/Hobart
Australia/Currie
Australia/Melbourne
Australia/Sydney
Australia/Broken_Hill
Australia/Brisbane
Australia/Lindeman
Australia/Adelaide
Australia/Darwin
Australia/Perth
Australia/Eucla
```