TimeZoneNames  [![NuGet Version](https://img.shields.io/nuget/v/TimeZoneNames.svg?style=flat)](https://www.nuget.org/packages/TimeZoneNames/) 
=============

A simple library that provides localized time zone names using CLDR and TZDB sources.

Why?  Because .NET's usual time zone display names are not localized properly, and are often wrong or unsuitable for various scenarios.
Read [this blog post](http://codeofmatt.com/2014/12/26/localized-time-zone-names-in-net/) for more details.

Nuget Installation
=============================================================================
```powershell
PM> Install-Package TimeZoneNames
```

This library is targeting .NET Standard 2.0, 1.1, and .NET Framework 4.5, 4.0 and 3.5.
See the [.NET Standard Platform Support Matrix](https://docs.microsoft.com/en-us/dotnet/articles/standard/library) for further details.

Demo
=============================================================================
One possible scenario for this library is to build a localized time zone selection control.
[Click here for a live demonstration](http://timezonepickerdemo.azurewebsites.net/).

Usage
=============================================================================

First, import the `TimeZoneNames` namespace:
```csharp
using TimeZoneNames;
```
All functionality is provided as static methods from the `TZNames` class.

## Methods for localizing a single time zone

### GetNamesForTimeZone

Look up the localized names for a specific time zone:
```csharp
var names = TZNames.GetNamesForTimeZone("America/Los_Angeles", "en-US");

names.Generic == "Pacific Time"
names.Standard == "Pacific Standard Time"
names.Daylight == "Pacific Daylight Time"
```

```csharp
var names = TZNames.GetNamesForTimeZone("America/Los_Angeles", "fr-CA");

names.Generic == "heure du Pacifique"
names.Standard == "heure normale du Pacifique"
names.Daylight == "heure avancée du Pacifique"
```

You can pass a Windows time zone id instead, if you like:
```csharp
var names = TZNames.GetNamesForTimeZone("Romance Standard Time", "en-GB");

names.Generic == "Central European Time"
names.Standard == "Central European Standard Time"
names.Daylight == "Central European Summer Time"
```

### GetAbbreviationsForTimeZone
Look up the localized abbreviations for a specific time zone:
```csharp
var abbreviations = TZNames.GetAbbreviationsForTimeZone("America/Los_Angeles", "en-US");

abbreviations.Generic == "PT"
abbreviations.Standard == "PST"
abbreviations.Daylight == "PDT"
```

You can pass a Windows time zone id instead, if you like:
```csharp
var abbreviations = TZNames.GetAbbreviationsForTimeZone("Romance Standard Time", "en-GB");

names.Generic == "CET"
abbreviations.Standard == "CET"
abbreviations.Daylight == "CEST"
```

**Note:** Time zone abbreviations are sometimes inconsistent, and are not necessarily
localized correctly for every time zone.  In most cases, you should use abbreviations
for end-user display output only.  Do not attempt to use abbreviations when parsing input.

### GetLocationNamesForTimeZone

Get the countr(ies) and city name representative of an IANA time zone ID.  This can
be useful when displaying a user interface showing a previously selected time zone,
or allowing selection between two time zones that have the same name, when the
selectable time zones are not based on a selected country.

Note that a single time zone identifier may have multiple countries but will only
have a single city; specifically, the one referred to within the IANA identifier itself.
Note also that some time zones have _no_ representative countries; for example the
adminstrative time zones (those starting with "Etc/").  These will return a an empty
array for `Country` and a value for `City` which is derived from the time zone
identifier.  In practice these time zones are not ambiguous and thus do not need
additional country/city information.

```csharp
// These two time zones have the same name, so a user would have difficulty
// selecting between them in a user interface.
var tz1 = TZNames.GetNamesForTimeZone("America/Tijuana", "en-US");
var tz2 = TZNames.GetNamesForTimeZone("America/Los_Angeles", "en-US");
Assert.Equal(tz1.Generic, tz2.Generic); // "Pacific Time"

// They can be differentiated for the user by showing the countr(ies)
// and/or city associated with each one
var loc1 = TZNames.GetLocationNamesForTimeZone("America/Tijuana", "en-US");
var loc2 = TZNames.GetLocationNamesForTimeZone("America/Los_Angeles", "en-US");

// An example of a time zone with multiple countries
var loc3 = TZNames.GetLocationNamesForTimeZone("Atlantic/St_Helena", "en-US");

// An example of a time zone with no countries
var loc4 = TZNames.GetLocationNamesForTimeZone("Etc/UTC", "en-US");
```
*Output*

Input               | Countries                                      | City
--------------------|------------------------------------------------|----------
America/Tijuana     | Mexico                                         | Tijuana
America/Los_Angeles | United States                                  | Los Angeles
Atlantic/St_Helena  | St. Helena, Ascension Island, Tristan da Cunha | St Helena
Etc/UTC             | _[empty list]_                                 | UTC


## Methods for listing time zones

### GetTimeZonesForCountry

Get a list of time zone names for a specific country, suitable for user time zone
selection.

Returns a dictionary whose key is the IANA time zone identifier, and whose value
is the localized generic time zone name.  When more than one entry in the result
set shares the same name, then a localized city name is appended in parenthesis
to disambiguate.  (This is usually due to historical differences.)

```csharp
var zones = TZNames.GetTimeZonesForCountry("BR", "pt-BR");
```
*Output*

Key                       | Value
--------------------------|--------------------------
America/Eirunepe          | Horário do Acre (Eirunepé)
America/Rio_Branco        | Horário do Acre (Rio Branco)
America/Porto_Velho       | Horário do Amazonas (Porto Velho)
America/Boa_Vista         | Horário do Amazonas (Boa Vista)
America/Manaus            | Horário do Amazonas (Manaus)
America/Campo_Grande      | Horário do Amazonas (Campo Grande)
America/Cuiaba            | Horário do Amazonas (Cuiabá)
America/Belem             | Horário de Brasília (Belém)
America/Fortaleza         | Horário de Brasília (Fortaleza)
America/Recife            | Horário de Brasília (Recife)
America/Araguaina         | Horário de Brasília (Araguaína)
America/Maceio            | Horário de Brasília (Maceió)
America/Bahia             | Horário de Brasília (Bahia)
America/Santarem          | Horário de Brasília (Santarém)
America/Sao_Paulo         | Horário de Brasília (São Paulo)
America/Noronha           | Horário de Fernando de Noronha

Many scenarios don't require all time zones, so you can specify a `threshold`
date as an optional parameter:
```csharp
var zones = TZNames.GetTimeZonesForCountry("BR", "pt-BR", new DateTime(2010, 1, 1));
```
*Output*

Key                       | Value
--------------------------|--------------------------
America/Rio_Branco        | Horário do Acre
America/Manaus            | Horário do Amazonas (Manaus)
America/Cuiaba            | Horário do Amazonas (Cuiabá)
America/Fortaleza         | Horário de Brasília (Fortaleza)
America/Araguaina         | Horário de Brasília (Araguaína)
America/Bahia             | Horário de Brasília (Bahia)
America/Sao_Paulo         | Horário de Brasília (São Paulo)
America/Noronha           | Horário de Fernando de Noronha

If you are not concerned with historical time zone differences at all, then
pass `DateTimeOffset.UtcNow` to return only time zones that differ in the future.
```csharp
var zones = TZNames.GetTimeZonesForCountry("BR", "pt-BR", DateTimeOffset.UtcNow);
```
*Output*

Key                       | Value
--------------------------|--------------------------
America/Rio_Branco        | Horário do Acre
America/Manaus            | Horário do Amazonas (Manaus)
America/Cuiaba            | Horário do Amazonas (Cuiabá)
America/Bahia             | Horário de Brasília (Bahia)
America/Sao_Paulo         | Horário de Brasília (São Paulo)
America/Noronha           | Horário de Fernando de Noronha

### GetTimeZoneIdsForCountry

Get a list of time zone identifiers for a specific country.  Similar to the
`GetTimeZonesForCountry` method, but without localized names.
```csharp
string[] zones = TZNames.GetTimeZoneIdsForCountry("AU");
```
*Output*
```
Australia/Perth
Australia/Eucla
Australia/Darwin
Australia/Broken_Hill
Australia/Adelaide
Australia/Brisbane
Australia/Lindeman
Australia/Hobart
Australia/Currie
Australia/Melbourne
Australia/Sydney
Australia/Lord_Howe
Antarctica/Macquarie
```

Like the `GetTimeZonesForCountry` method, an optional `threshold` parameter is
supported for limiting the list to those zones that vary only after a specific
date.
```csharp
string[] zones = TZNames.GetTimeZoneIdsForCountry("AU", DateTimeOffset.Now);
```
*Output*
```
Australia/Perth
Australia/Eucla
Australia/Darwin
Australia/Adelaide
Australia/Brisbane
Australia/Sydney
Australia/Lord_Howe
Antarctica/Macquarie
```

### GetFixedTimeZoneIds

Gets a list of time zone IDs that represent fixed offset from UTC, including UTC itself.
Note that time zones of the form `Etc/GMT[+/-]n` use an inverted sign from the usual
conventions.

TODO: Add examples for this method.

### GetFixedTimeZoneNames

Gets the same list of zones as `GetFixedTimeZoneIds`, but includes localized names.

TODO: Add examples for this method.

### GetFixedTimeZoneAbbreviations

Gets the same list of zones as `GetFixedTimeZoneIds`, but includes localized abbreviations.

TODO: Add examples for this method.

## Additional supporting methods

### GetCountryNames

Gets a localized list of country names, suitable for selecting a country before
selecting a time zone in a two-dropdown time zone selection control.

TODO: Add examples for this method.

### GetLanguageCodes

Gets a list of all language codes supported by this library.  Useful for testing
and validation.

TODO: Add examples for this method.
