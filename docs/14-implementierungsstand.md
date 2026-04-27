# Implementierungsstand

Dieses Dokument beschreibt den aktuellen technischen Stand im Repo und gleicht ihn mit dem Massnahmenplan ab.

Stand: nach erstem Scaffold und Auth-/Shell-Start.

## Vorhanden

### Backend

Pfad:

```text
backend/MixingAI.Api
```

Vorhanden:

- ASP.NET Core 10 WebAPI
- EF Core + Npgsql
- `AppDbContext`
- Initial-Migration fuer User/Auth
- `User`
- `AuthSession`
- `PasswordHashingService`
- `SessionTokenService`
- `CurrentUserService`
- Login/Logout/Me-Endpunkte unter `/api/auth`
- HttpOnly-Session-Cookie
- Rate Limiting fuer Auth
- Development Seed fuer Admin/User
- Health-Endpunkt `/health`

Noch nicht vorhanden:

- Document Vault
- Datei-Upload
- Storage-Service
- Import-Gate
- Review-Funktion
- Trial-/Recipe-Datenmodell
- Ollama-Backend-Service
- OCR-Provider

### Frontend

Pfad:

```text
frontend
```

Vorhanden:

- React + TypeScript + Vite
- React Router
- TanStack Query
- Login-Seite
- Protected Route
- Shared Shell
- Startseite
- Platzhalterseiten fuer Dokumente, Versuche und Admin

Noch zu pruefen/anzupassen:

- Keine Sidebar beibehalten
- Kachelstartseite gegen UI-Leitplanken pruefen
- zweite Navigationsebene fuer Dokumente/Review/Versuche bauen
- Design minimal und amixon-orientiert halten

### Deployment / Infrastruktur

Vorhanden:

- `docker-compose.dev.yml` fuer lokale PostgreSQL- und Ollama-Container
- `docker-compose.yml` fuer Pilot/Produktion
- Nginx-Konfiguration mit HTTP->HTTPS Redirect
- Nginx TLS Skeleton fuer `mixingai.amixon.local`
- persistente Volume-Ziele unter `/srv/mixingai`
- `.env.example` mit Development- und Compose-Variablen

Noch offen:

- TLS-Zertifikat aus amixon-Umgebung
- NVIDIA Container Runtime auf Ziel-VM pruefen
- konkrete Ollama-Modelle
- Backup-Script
- Restore-Test

## Readiness-Abgleich

| Phase | Status | Kommentar |
| --- | --- | --- |
| 1 Projekt-Scaffold | teilweise erledigt | Backend, Frontend, Docker, Nginx und Env sind angelegt. Build/Start muss noch verifiziert und dokumentiert werden. |
| 2 Basis-App | teilweise erledigt | Login, Auth-Grundlage und Shared Shell existieren. UI-Leitplanken noch gegen Umsetzung pruefen. |
| 3 Document Vault | offen | Naechster groesserer Implementierungsschritt. |
| 4 Import-Gate | offen | Abhaengig von Document Vault. |
| 5 Review-Maske | offen | Zentraler MVP-Screen. |
| 6 Datenmodell stabilisieren | offen | Nach ersten Dokumentmustern. |
| 7 Suche und Filter | offen | Nach Trial-Datenmodell. |
| 8 Excel-Export | offen | Nach Suche/Filter. |
| 9 On-Prem KI mit Ollama | offen | Nach Suche/Filter; Service-Schnittstelle vorbereiten. |
| 10 OCR-Provider | offen | Nach echten PDF-Beispielen. |
| 11 Deployment | offen | Compose/Nginx Skeleton existiert, Zielbetrieb noch nicht validiert. |
| 12 Pilotdaten | offen | Nur on-prem mit echten Daten. |

## Naechste sinnvolle technische Schritte

1. Build und Start lokal verifizieren.
2. README-Startbefehle gegen Ist-Zustand pruefen.
3. Untracked Dateien bewusst behandeln:
   - `.claude/`
   - `backend/MixingAI.Api/.dockerignore`
4. Document Vault implementieren:
   - `Document`
   - `DocumentVersion`
   - Storage-Service
   - Upload
   - Hash/Dublettencheck
   - Dokumentliste
5. Danach Import-Gate und Review-Maske beginnen.

## Hinweis zu erpforai-Uebernahme

Die Auth-Grundlage ist bereits sichtbar aus dem `erpforai`-Pattern abgeleitet und fuer MixingAI vereinfacht.

Die DMS-Uebernahme steht noch aus. Sie soll nicht als ERP-DMS kopiert werden, sondern als schlanker Document Vault umgesetzt werden.
