# Tech-Stack

## Empfehlung

Der Stack soll nah an `erpforai` bleiben, aber fachlich deutlich kleiner sein.

```text
Frontend:   React + TypeScript
Backend:    ASP.NET Core / .NET 10
API:        REST, spaeter optional toolbasierte KI-Endpunkte
DB:         PostgreSQL
ORM:        EF Core + Npgsql
Storage:    lokaler App-Storage, spaeter S3/MinIO/Azure Blob austauschbar
Jobs:       Background Worker im Backend, spaeter Hangfire/Quartz bei Bedarf
OCR:        austauschbarer Provider
KI:         separater Backend-Service, kein Frontend-Key
```

## Backend

Empfohlen:

- ASP.NET Core
- .NET 10 LTS
- EF Core
- Npgsql fuer PostgreSQL
- Minimal APIs oder Controller
- modulare Ordnerstruktur nach Fachbereichen

Warum:

- `erpforai` nutzt bereits .NET 10, EF Core und PostgreSQL.
- Auth-/Rechte-Pattern kann uebernommen und vereinfacht werden.
- PostgreSQL passt gut fuer relationale Rezeptdaten, Volltextsuche und optional Vektorsuche.

## Frontend

Empfohlen:

- React
- TypeScript
- Vite
- einfache, dichte Business-UI

Kernmasken:

- Login
- Dokumentenarchiv
- Upload / Import
- Review und Korrektur
- Rezeptsuche
- Rezeptvergleich
- Rohstoffverwaltung
- Ops Dashboard
- spaeter KI-Chat

Warum nicht zuerst Chat:

Die Anwendung muss auch ohne KI nutzbar sein. Suche, Filter, Vergleich und Review sind normale Produktfunktionen.

## Datenbank

Empfohlen:

- PostgreSQL als zentrale Datenbank
- EF Core Migrations
- getrennte Tabellen fuer DMS, Import-Staging und Rezeptdaten
- Full Text Search fuer Dokumenttexte und Metadaten
- optional `pgvector` fuer semantische Aehnlichkeit

Startschemas:

```text
app_core      -- User, Rollen, Rechte, Audit
app_dms       -- Dokumente, Versionen, Dateien, OCR-Text
app_import    -- Importlaeufe, Staging, Validierungen
app_recipe    -- Rezepte, Rohstoffe, Rezeptpositionen, Eigenschaften
app_ai        -- KI-Konfiguration, Chatverlauf optional, Tool-Logs
```

## Dateiablage

Start:

- lokaler Storage unterhalb des App-Installationsverzeichnisses oder konfigurierbarer Root-Pfad
- DB speichert `storage_path`, nicht den Dateiinhalt
- SHA256-Hash pro Datei
- Originaldateiname und MIME-Type speichern

Spaeter austauschbar:

- S3
- MinIO
- Azure Blob Storage
- SMB nur als Importquelle, nicht als fuehrende App-Ablage

## OCR und Dokumentextraktion

Provider austauschbar halten.

Startpfade:

- Excel direkt strukturiert lesen
- digitale PDFs mit Text-/Tabellenextraktion lesen
- gescannte PDFs per OCR
- OCR-Ergebnisse immer mit Confidence und Quelle speichern

Moegliche Provider:

- Azure AI Document Intelligence fuer Layout, Tabellen und OCR
- lokale OCR-Komponenten bei Offline-Anforderung
- spezialisierte Parser fuer wiederkehrende Excel-Templates

Wichtig:

Excel ist nicht "OCR". Excel-Dateien sollen strukturiert ueber Workbook, Sheets, Zellen, Formeln und Tabellenbereiche gelesen werden.

## KI-Integration

KI wird als Backend-Komponente angebunden.

Regeln:

- keine API-Keys im Frontend
- keine direkte DB-Verbindung fuer das Modell
- nur definierte Backend-Tools
- Quellenpflicht in Antworten
- produktive Suche nur auf freigegebenen Daten

MVP-KI spaeter:

- natuerliche Sprache in Suchfilter uebersetzen
- Rohstoff-Mapping vorschlagen
- aehnliche Rezepte erklaeren
- Ergebnisse zusammenfassen
- Quellen anzeigen

## Deployment

Naheliegend:

- Windows lokal fuer Entwicklung
- Linux VM oder Windows Server fuer Pilotbetrieb
- PostgreSQL separat oder auf derselben Pilotmaschine
- App-Storage auf lokaler Platte/Volume
- Backup fuer DB und Storage gemeinsam planen

## Tests

Wichtige Testarten:

- Unit Tests fuer Parser und Validierungsregeln
- Integration Tests fuer API + DB
- Import-Fixtures mit Beispiel-PDFs/-Excels
- Regressionstests fuer Rohstoff-Mapping
- End-to-End Tests fuer Upload -> Review -> Freigabe -> Suche

## Sicherheitsgrundsaetze

- HttpOnly Session Cookie
- Rollen-/Rechtepruefung im Backend
- Audit-Log fuer Datenfreigaben und Aenderungen
- Upload-Dateitypen whitelisten
- Dateipfade nie direkt vom Client verwenden
- Storage-Pfade gegen Path Traversal schuetzen
- Rate Limiting fuer Login und teure KI/OCR-Endpunkte
