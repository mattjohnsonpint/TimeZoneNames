TimeZoneNames
=============

[![Join the chat at https://gitter.im/mj1856/TimeZoneNames](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/mj1856/TimeZoneNames?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Simple portable class library that provides localized time zone names using CLDR and TZDB sources.

Why?  Because .NET's usual time zone display names are not localized properly, and are often wrong.
Read [this blog post](http://codeofmatt.com/2014/12/26/localized-time-zone-names-in-net/) for more details.

**Nuget Installation**
```powershell
PM> Install-Package TimeZoneNames
```

**Usage**

Look up the localized names for a specific time zone:
```csharp
var names = TimeZoneNames.GetNamesForTimeZone("America/Los_Angeles", "en-US");

names.Generic == "Pacific Time"
names.Standard == "Pacific Standard Time"
names.Daylight == "Pacific Daylight Time"
```

```csharp
var names = TimeZoneNames.GetNamesForTimeZone("America/Los_Angeles", "fr-CA");

names.Generic == "heure du Pacifique"
names.Standard == "heure normale du Pacifique"
names.Daylight == "heure avancée du Pacifique"
```

You can pass a Windows time zone id instead, if you like:
```csharp
var names = TimeZoneNames.GetNamesForTimeZone("Romance Standard Time", "en-GB");

names.Generic == "Central European Time"
names.Standard == "Central European Standard Time"
names.Daylight == "Central European Summer Time"
```

Abbreviations are also avaialble:
```csharp
var abbreviations = TimeZoneNames.GetAbbreviationsForTimeZone("America/Los_Angeles", "en-US");

abbreviations.Generic == "PT"
abbreviations.Standard == "PST"
abbreviations.Daylight == "PDT"
```

Look up the time zones for a specific country:
```csharp
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

Get the time zone names for a specific country:
```csharp
var zones = TimeZoneNames.GetTimeZonesForCountry("BR", "pt-BR");
```
*Output*
```
America/Noronha
Generic  = Horário de Fernando de Noronha
Standard = Horário Padrão de Fernando de Noronha
Daylight = Horário de Verão de Fernando de Noronha

America/Belem
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Fortaleza
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Recife
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Araguaina
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Maceio
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Bahia
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Sao_Paulo
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Campo_Grande
Generic  = Horário do Amazonas
Standard = Horário Padrão do Amazonas
Daylight = Horário de Verão do Amazonas

America/Cuiaba
Generic  = Horário do Amazonas
Standard = Horário Padrão do Amazonas
Daylight = Horário de Verão do Amazonas

America/Santarem
Generic  = Horário de Brasília
Standard = Horário Padrão de Brasília
Daylight = Horário de Verão de Brasília

America/Porto_Velho
Generic  = Horário do Amazonas
Standard = Horário Padrão do Amazonas
Daylight = Horário de Verão do Amazonas

America/Boa_Vista
Generic  = Horário do Amazonas
Standard = Horário Padrão do Amazonas
Daylight = Horário de Verão do Amazonas

America/Manaus
Generic  = Horário do Amazonas
Standard = Horário Padrão do Amazonas
Daylight = Horário de Verão do Amazonas

America/Eirunepe
Generic  = Horário do Acre
Standard = Horário Padrão do Acre
Daylight = Horário de Verão do Acre

America/Rio_Branco
Generic  = Horário do Acre
Standard = Horário Padrão do Acre
Daylight = Horário de Verão do Acre
```
