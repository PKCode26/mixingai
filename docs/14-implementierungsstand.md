# Implementierungsstand

Stand: nach Phase 1–4 (Document Vault + Import Gate).

## Phase 1 — Projekt-Scaffold `erledigt`

- ASP.NET Core 10 WebAPI
- React + TypeScript + Vite Frontend
- Docker Compose fuer lokale Entwicklung (`docker-compose.dev.yml`)
- Docker Compose fuer Produktion/Pilot (`docker-compose.yml`)
- Nginx-Konfiguration mit HTTP->HTTPS-Redirect und TLS-Skeleton
- `.env.example` als Vorlage
- PostgreSQL auf Port 5433 (dev) um Konflikt mit lokalem Windows-Postgres zu vermeiden
- Startscript `start-dev.bat` + `scripts/start-dev.ps1`

## Phase 2 — Basis-App `erledigt`

Backend:

- `User`, `AuthSession`, `AuditableEntity`
- `PasswordHashingService` (ASP.NET PasswordHasher)
- `SessionTokenService` (SHA256-Hash des Tokens)
- `CurrentUserService` (Token aus Cookie oder Bearer-Header)
- Login / Logout / Me unter `/api/auth`
- Rate Limiting fuer Auth-Endpunkte
- HttpOnly-Cookie `mixingai_auth`
- Development-Seed fuer Admin- und Test-Benutzer
- Health-Endpunkt `/health`
- EF Core + Npgsql + Migrationen mit `IF NOT EXISTS` SQL

Frontend:

- Login-Seite mit Fehlerbehandlung
- Protected Route (Weiterleitung auf Login ohne Session)
- Shared Shell (AppShell + Navigation)
- Startseite mit Kacheln
- Platzhalterseiten fuer Versuche und Admin

## Phase 3 — Document Vault `erledigt`

Backend:

- `Document`-Entity (OriginalFileName, DisplayName, MimeContentType, FileSizeBytes, ContentHash, StoragePath, DocumentType, IsArchived)
- `StorageService`: Upload, SHA256-Hash, relative Pfade, Download-Stream, Loeschen, GetFullPath
- Duplikatserkennung per SHA256-Hash (409 Conflict mit Verweis auf vorhandene Datei)
- Archivieren / Wiederherstellen (kein hartes Loeschen)
- 6 Endpunkte unter `/api/documents`: upload, list (Suche + Archivfilter), get, download, archive, unarchive
- Migration `AddDocuments` (IF NOT EXISTS)

Frontend:

- `DocumentsPage` mit Drag-and-Drop-Uploadzone
- Dateiliste mit Typbadge, Hash-Vorschau, Datum, Aktionsbuttons
- Duplikatsmeldung mit Verweis auf vorhandene Datei
- Suche nach Dateiname, Archivierungsfilter

## Phase 4 — Import Gate `erledigt (Infrastruktur)`

Backend:

- Entities: `ImportRun`, `StagedField` (Key-Value, flexibel), `ValidationIssue`
- Status: Queued, Extracting, NeedsReview, Approved, Published, Archived, Failed, Rejected, NeedsRework
- `ImportProcessor` als BackgroundService (alle 4s, max. 5 Runs gleichzeitig)
- `IDocumentExtractor` als Schnittstelle fuer Extraktoren
- `PdfExtractor` (PdfPig): Textlayer zeilenweise, Quellverweis `Seite:N`
- `ExcelExtractor` (ClosedXML): Label-Wert-Paare aus Nachbarzellen, Quellverweis `Sheet:Name,Zelle:A1`
- `FieldPatternMatcher`: 17 Regex-Regeln fuer amixon-Versuchsprotokoll-Struktur + Dateiname-Parser
- 9 Endpunkte unter `/api/imports`: create, list, get, staged, issues, approve, reject, rework, confirm field
- Migration `AddImportRuns` (IF NOT EXISTS)

Frontend:

- `DocumentsPage` jetzt mit zwei Tabs: **Dateien** | **Importläufe**
- Button „Import starten" pro Dokument-Zeile
- `ImportRunsTab`:
  - Tabelle aller Importlaeufe mit Status-Badges (Queued, Extracting, NeedsReview, Approved, Failed…)
  - aufklappbare Staged-Fields-Panels pro Run (Feldname, Wert, Konfidenz, Quellverweis)
  - Validierungsprobleme als Warnleiste
  - „Freigeben" / „Ablehnen"-Buttons fuer Runs im Status NeedsReview oder NeedsRework
  - Volltext-Akkordeon (RawText versteckt, aufklappbar)
  - Auto-Refresh alle 5s (Extracting -> NeedsReview live sichtbar)
- `types/imports.ts` + `api.imports.*` in `api.ts`

## Readiness-Abgleich

| Phase | Status | Kommentar |
| --- | --- | --- |
| 1 Scaffold | erledigt | Backend, Frontend, Docker, Nginx, Env, Startscript |
| 2 Basis-App | erledigt | Login, Auth, Protected Route, Shared Shell |
| 3 Document Vault | erledigt | Upload, Hash, Dubletten, Archiv, Download |
| 4 Import Gate | erledigt (Infrastruktur) | Extraktor-Pipeline, BackgroundService, Staging, 9 Endpunkte, Frontend mit Tabs und Review-Aktionen |
| 5 Review-Maske | offen | Vollstaendige Review-Detailseite mit Dokumentvorschau links + Feldern rechts fehlt noch |
| 6 Datenmodell stabilisieren | offen | Nach ersten echten Dokumenten |
| 7 Suche und Filter | offen | Nach Datenmodell |
| 8 Excel-Export | offen | Nach Suche/Filter |
| 9 On-Prem KI mit Ollama | offen | Nach Suche/Filter |
| 10 OCR-Provider | offen | Nach echten gescannten PDFs |
| 11 Deployment | offen | Compose/Nginx Skeleton vorhanden, Zielbetrieb noch nicht validiert |
| 12 Pilotdaten | offen | Nur on-prem mit echten Daten |

## Naechste sinnvolle Schritte

1. Echte Versuchsdokumente importieren und `FieldPatternMatcher` nachschaerfen.
2. Review-Detailseite (Phase 5): Dokumentvorschau links, Felder rechts, Pflichtfelder markieren.
3. Produktives Datenmodell aus Staging-Ergebnissen ableiten (Trial/Recipe-Schema).
4. OCR-Pfad evaluieren sobald gescannte PDFs vorhanden.

## Bekannte Einschraenkungen

- `FieldPatternMatcher` ist auf amixon-Versuchsprotokoll-Struktur zugeschnitten; andere Dokumenttypen brauchen eigene Regeln.
- Gescannte PDFs (kein Textlayer) erzeugen einen Hinweis im Staging, werden aber nicht extrahiert.
- Review-Tab zeigt Felder als Tabelle; noch kein Side-by-Side mit Dokumentvorschau.
- Migrationen nutzen Raw-SQL mit `IF NOT EXISTS` (Workaround fuer Npgsql 10 Transaktions-Bug bei `ExecuteNonQuery`).
