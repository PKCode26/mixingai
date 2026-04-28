# Architekturentscheidungen

Dieses Dokument beschreibt die wesentlichen Architekturentscheidungen, die fuer MixingAI getroffen wurden, und die Begruendungen dahinter.

## Auth und Sessions

Umgesetzt:

- Passwort-Hashing mit ASP.NET `PasswordHasher`
- Session-Token als zufaelliges Byte-Array, gespeichert als SHA256-Hash in PostgreSQL
- HttpOnly-Cookie `mixingai_auth` (kein direkter JS-Zugriff)
- Login / Logout / Me-Endpunkte unter `/api/auth`
- `CurrentUserService` prueft Token, Session-Gueltigkeit und User-Status bei jedem Request
- kein JWT, kein Bearer-Token-Overhead fuer eine interne App

Bewusst nicht umgesetzt (MVP):

- Rollenverwaltung mit granularen Rechten
- Gruppen oder Abteilungsbezuege
- LDAP / AD-Anbindung
- Two-Factor-Auth

Erweiterbarkeit:

- `IsAdmin` als einfacher Unterschied genuegt fuer den MVP
- alle angemeldeten Nutzer koennen fachlich erstmal alles
- spaetere Rechteerweiterung bleibt moeglich, ist aber kein MVP-Ziel

## Auditing

Umgesetzt:

- `AuditableEntity` als Basisklasse mit `CreatedAtUtc`, `CreatedByUserId`, `UpdatedAtUtc`, `UpdatedByUserId`
- alle fachlichen Entities erben davon

Spaeter ergaenzen:

- Audit-Events fuer Import, Review, Freigabe und Rezeptaenderungen

## Document Vault

Umgesetzt:

- `Document`-Entity mit Metadaten (Originalname, MIME-Typ, Dateigroesse, SHA256-Hash, StoragePath)
- lokaler App-Storage, konfigurierbar ueber `Storage:DocumentRootPath`
- Datei-Hash wird beim Upload berechnet; Duplikate werden per 409 abgelehnt
- Archivieren statt hart loeschen
- `DocumentType` (PDF, Excel, Other) fuer spezifische Verarbeitungspfade

Bewusst nicht umgesetzt:

- Dokumentversionen (kein `DocumentVersion`-Entity im MVP)
- QM-/ISO-Felder
- komplexe DMS-Freigabe-Workflows

Storage-Pfad:

```text
{ContentRootPath}/{Storage:DocumentRootPath}/{Jahr}/{Monat}/{Guid}.{Ext}
```

Der Pfad wird relativ in der DB gespeichert; der Storage-Root ist konfigurierbar.

## Staging-Modell (Import Gate)

Umgesetzt:

- `ImportRun` pro Dokument pro Importversuch; ein Dokument kann mehrfach importiert werden
- `StagedField` als flexibles Key-Value-Modell (kein festes Schema); Felder koennen mit echten Daten erweitert werden
- `ValidationIssue` fuer Warnungen und Fehler aus der Extraktion
- `Confidence` und `SourceRef` pro Feld fuer Rueckverfolgbarkeit
- `IsConfirmed` als Reviewer-Zustimmung pro Feld

Statusmodell fuer `ImportRun`:

```
Queued -> Extracting -> NeedsReview -> Approved -> Published
                     -> Failed
                     -> Rejected
                     -> NeedsRework -> NeedsReview ...
```

Warum kein festes Rezept-Schema sofort:

Das Feldmodell der Versuchsprotokolle ist erst mit echten Daten final definierbar. Das flexible Staging-Modell erlaubt es, real vorkommende Felder zuerst zu sichten und dann das produktive Schema daraus abzuleiten.

## Extraktion

PDF:

- `UglyToad.PdfPig` fuer digitale PDFs
- Woerter werden nach Y-Position zu Zeilen gruppiert (raeumliche Rekonstruktion)
- bei fehlendem Textlayer: Issue "kein Textlayer" + OCR-Hinweis

Excel:

- `ClosedXML` fuer `.xlsx` / `.xls`
- Zellinhalte werden pro Worksheet als Label-Wert-Paare gelesen (Nachbarzellen-Heuristik)
- Zellkoordinate (`Sheet:Name,Zelle:A1`) als SourceRef

Feldmatcher:

- `FieldPatternMatcher` mit 17 Regex-Regeln fuer amixon-Versuchsprotokoll-Struktur
- Felder: Kunde, TeilnehmerKunde, TeilnehmerAmixon, Aufgabenstellung, Testapparat, Versuchsnummer, Produkt, Versuchsziel, Versuchsaggregat, Sonderausstattung, BefuellenMit, Gesamtmenge, Mischzeit, Drehzahl, Temperatur, Fuellgrad, Chargengewicht, Datum
- Dateiname-Parser fuer `{Versuchsnummer} {Kunde} {Mischertyp} {Baugroesse}-{Fabrikatnummer}`
- Confidence 0.75–0.90 je nach Regelspezifitaet

## Datenbank-Schema

```text
app_core.users               -- Benutzer
app_core.auth_sessions       -- aktive Sessions
app_core.documents           -- Dokument-Metadaten
app_core.import_runs         -- Importlaeufe pro Dokument
app_core.staged_fields       -- extrahierte Rohfelder (Key-Value)
app_core.validation_issues   -- Warnungen/Fehler pro Importlauf
```

Migrationen:

- alle Migrationen nutzen `IF NOT EXISTS` in Raw-SQL (Npgsql-10-Kompatibilitaet)
- EF-Konfigurationen unter `Infrastructure/Data/Configurations/`

## Nicht umgesetzt (bewusst)

- Rollenverwaltung im MVP
- Gruppen- oder Abteilungskontext
- komplexes DMS-QM-/ISO-Review
- statisches Frontend oder Vanilla-JS
- mehrere .NET-Projekte im Monorepo (Single-API genuegt fuer den MVP)
- direkte DB-Verbindung fuer KI-Komponenten
