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

You can pass a Windows time zone id instead, if you like:
```
var names = TimeZoneNames.GetNamesForTimeZone("Romance Standard Time", "en-GB");

names.Generic == "Central European Time"
names.Standard == "Central European Standard Time"
names.Daylight == "Central European Summer Time"
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

Get the time zone names for a specific country:
```
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
