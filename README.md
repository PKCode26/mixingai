# MixingAI

MixingAI ist ein kontrolliertes Rezept- und Dokumentensystem fuer industrielle Mischungsdaten, z.B. Zement- und Baustoffrezepturen aus PDF- und Excel-Dateien.

Das System ist bewusst kein "Chatbot ueber Dokumente". Der Kern ist eine gepruefte Datenbank mit sauberem Import-Gate, Dokumentenarchiv, Versuchs-/Rezeptdatenmodell und normalen Such-/Filterfunktionen. KI wird spaeter als Assistenzschicht angebunden.

## Zielbild

```text
Altbestand Windows-Fileserver
  -> kontrollierte Migration / Upload
  -> App-Storage mit Hash und Version
  -> Import- und Extraktionslauf
  -> Review- und Korrekturmaske
  -> freigegebene Versuchs- und Rezeptdatenbank
  -> Suche, Filter, Vergleich, Export
  -> optional KI-Chat als Bedienhilfe
```

Der Windows-Fileserver ist nur Quelle fuer die initiale Migration. Nach der Uebernahme ist die Anwendung das fuehrende System fuer diese Versuchs-, Rezept- und Dokumentdaten.

## Kernbereiche

1. User und einfache Anmeldung
2. Ops / Datenqualitaet
3. DMS / Dokumentenarchiv
4. Versuchs- und Rezeptdatenbank
5. Import- und Review-Maske
6. KI-Chatbot als spaeterer Zusatz

## Dokumentation

- [Architektur](docs/01-architektur.md)
- [Tech-Stack](docs/02-tech-stack.md)
- [Architekturentscheidungen](docs/03-architekturentscheidungen.md)
- [Roadmap](docs/04-roadmap.md)
- [Umgebungsvariablen](docs/05-umgebungsvariablen.md)
- [Grundsatzentscheidungen](docs/06-grundsatzentscheidungen.md)
- [On-Prem-KI und Testdaten](docs/07-on-prem-ki-und-testdaten.md)
- [Deployment-Zielsetup](docs/08-deployment-zielsetup.md)
- [UI- und Produktleitplanken](docs/09-ui-und-produktleitplanken.md)
- [Datenmodell-Inputfaktoren](docs/10-datenmodell-inputfaktoren.md)
- [Massnahmenplan und Readiness](docs/11-massnahmenplan-readiness.md)
- [Known Bugs](docs/12-known-bugs.md)
- [Ideen und Optimierungen](docs/13-ideen-und-optimierungen.md)
- [Implementierungsstand](docs/14-implementierungsstand.md)

## Struktur

```
mixingai/
|-- backend/MixingAI.Api/   ASP.NET Core 10 WebAPI + EF Core + Npgsql
|-- frontend/               React + TypeScript + Vite
|-- nginx/                  Nginx-Konfiguration (Reverse Proxy + TLS)
|-- docker-compose.yml      Produktion / Pilotbetrieb
|-- docker-compose.dev.yml  Lokale Entwicklung (nur DB + Ollama als Container)
`-- .env.example            Vorlage fuer lokale .env
```

## Lokale Entwicklung starten

**Voraussetzung:** .NET 10 SDK, Node.js 20+, Docker Desktop

```bash
# 1. .env anlegen
cp .env.example .env
# POSTGRES_PASSWORD in .env setzen

# 2. Datenbank und Ollama als Container starten
docker compose -f docker-compose.dev.yml up -d

# 3. Backend starten
cd backend/MixingAI.Api
dotnet run

# 4. Frontend starten (neues Terminal)
cd frontend
npm install
npm run dev
```

Backend laeuft auf http://localhost:5085 (Health: `/health`)
Frontend laeuft auf http://localhost:5173

## Produktion (Docker Compose)

```bash
# .env mit produktiven Werten befuellen
# Nginx-Zertifikat unter nginx/certs/ ablegen (nicht in Git)

docker compose up -d
```

## EF Core Migrationen

```bash
cd backend/MixingAI.Api
dotnet ef migrations add <Name>
dotnet ef database update
```

## Grundsatz

Die KI ist nicht die Datenbank und nicht das fuehrende System.

Backend, Datenbank, Validierungsregeln, Berechtigungen und Audit-Logs entscheiden, welche Daten gelten. KI hilft beim Import, bei Rohstoff-Mapping, bei Suche und bei der Bedienung.
