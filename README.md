TimeZoneNames
=============

[![Join the chat at https://gitter.im/mj1856/TimeZoneNames](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/mj1856/TimeZoneNames?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Simple portable class library that provides localized time zone names using CLDR and TZDB sources.

Why?  Because .NET's usual time zone display names are not localized properly, and are often wrong.
Read [this blog post](http://codeofmatt.com/2014/12/26/localized-time-zone-names-in-net/) for more details.

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
names.Daylight == "heure avanc�e du Pacifique"
```

You can pass a Windows time zone id instead, if you like:
```
var names = TimeZoneNames.GetNamesForTimeZone("Romance Standard Time", "en-GB");

names.Generic == "Central European Time"
names.Standard == "Central European Standard Time"
names.Daylight == "Central European Summer Time"
```

Abbreviations are also avaialble:
```
var abbreviations = TimeZoneNames.GetAbbreviationsForTimeZone("America/Los_Angeles", "en-US");

abbreviations.Generic == "PT"
abbreviations.Standard == "PST"
abbreviations.Daylight == "PDT"
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
Generic  = Hor�rio de Fernando de Noronha
Standard = Hor�rio Padr�o de Fernando de Noronha
Daylight = Hor�rio de Ver�o de Fernando de Noronha

America/Belem
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Fortaleza
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Recife
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Araguaina
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Maceio
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Bahia
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Sao_Paulo
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Campo_Grande
Generic  = Hor�rio do Amazonas
Standard = Hor�rio Padr�o do Amazonas
Daylight = Hor�rio de Ver�o do Amazonas

America/Cuiaba
Generic  = Hor�rio do Amazonas
Standard = Hor�rio Padr�o do Amazonas
Daylight = Hor�rio de Ver�o do Amazonas

America/Santarem
Generic  = Hor�rio de Bras�lia
Standard = Hor�rio Padr�o de Bras�lia
Daylight = Hor�rio de Ver�o de Bras�lia

America/Porto_Velho
Generic  = Hor�rio do Amazonas
Standard = Hor�rio Padr�o do Amazonas
Daylight = Hor�rio de Ver�o do Amazonas

America/Boa_Vista
Generic  = Hor�rio do Amazonas
Standard = Hor�rio Padr�o do Amazonas
Daylight = Hor�rio de Ver�o do Amazonas

America/Manaus
Generic  = Hor�rio do Amazonas
Standard = Hor�rio Padr�o do Amazonas
Daylight = Hor�rio de Ver�o do Amazonas

America/Eirunepe
Generic  = Hor�rio do Acre
Standard = Hor�rio Padr�o do Acre
Daylight = Hor�rio de Ver�o do Acre

America/Rio_Branco
Generic  = Hor�rio do Acre
Standard = Hor�rio Padr�o do Acre
Daylight = Hor�rio de Ver�o do Acre
```
