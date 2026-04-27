# Deployment-Zielsetup

## Zielumgebung

MixingAI wird on-prem auf einer Linux-VM bei amixon betrieben.

Festgelegtes Betriebssystem:

```text
Distributor ID: Ubuntu
Description:    Ubuntu 24.04.4 LTS
Release:        24.04
Codename:       noble
```

Geplante Hardware:

```text
CPU:      AMD Ryzen Threadripper 7960X
RAM:      128 GB DDR5-5600
Storage:  2 x 2 TB M.2 NVMe RAID1
GPU:      NVIDIA RTX PRO 6000
```

## Bewertung

Diese Maschine ist fuer den MVP sehr gut dimensioniert.

Sie kann voraussichtlich auf derselben VM betreiben:

- MixingAI Backend
- Frontend-Auslieferung
- PostgreSQL
- App-Storage
- Import-/OCR-Jobs
- Ollama fuer lokale KI
- lokale Embedding-Erzeugung

## Betriebsmodell

Festlegung:

- Docker Compose fuer den MVP
- PostgreSQL lokal auf derselben VM
- Ollama lokal auf derselben VM
- Dokument- und Import-Storage auf lokalem NVMe-Volume
- Nginx als Reverse Proxy
- TLS/HTTPS fuer den Browserzugriff
- keine externen Cloud-Dienste fuer produktive Daten

## Vorgeschlagene Container

```text
mixingai-api
  ASP.NET Core API
  statische Frontend-Auslieferung oder Reverse Proxy auf Frontend-Build

mixingai-postgres
  PostgreSQL
  persistentes Volume

mixingai-ollama
  lokaler Ollama-Service
  GPU-Zugriff ueber NVIDIA Container Runtime
  persistentes Modell-Volume

mixingai-worker
  optional spaeter separater Import-/OCR-Worker
  im MVP kann der Worker auch im API-Prozess als Hosted Service laufen

mixingai-nginx
  Reverse Proxy / TLS Terminierung
```

MVP-Empfehlung:

```text
API + Frontend in einem Deployment
PostgreSQL als eigener Container
Ollama als eigener Container
Worker zuerst im API-Prozess
Nginx als fester Einstiegspunkt fuer HTTPS
```

## Nginx und TLS

Festlegung:

- Browserzugriff laeuft ueber HTTPS.
- Nginx terminiert TLS.
- Nginx leitet intern an die ASP.NET-App weiter.
- Die ASP.NET-App ist intern nicht direkt aus dem Netzwerk erreichbar.

Zielbild:

```text
Browser
  -> https://mixingai.<intern>
  -> Nginx
  -> mixingai-api
```

TLS-Zertifikat:

- bevorzugt ueber interne amixon-IT / interne CA
- alternativ ein intern verwaltetes Zertifikat fuer den Hostnamen
- Zertifikatsablage nicht im Git
- Zertifikatserneuerung im Betriebskonzept festhalten
- Einrichtung kann am Ende des MVP-Scaffolds erfolgen, die Architektur bleibt trotzdem HTTPS-first

Nginx sollte mindestens setzen:

- HTTPS Redirect von HTTP auf HTTPS
- sinnvolles Upload-Limit fuer PDF/Excel-Dateien
- Proxy-Header fuer ASP.NET
- Security Header fuer interne Webanwendung
- Timeouts passend fuer grosse Uploads und Exporte

## Storage

Persistente Pfade:

```text
/srv/mixingai/postgres
/srv/mixingai/storage/documents
/srv/mixingai/storage/imports
/srv/mixingai/storage/exports
/srv/mixingai/ollama
/srv/mixingai/backups
```

Grundsatz:

- DB-Daten und Datei-Storage muessen gemeinsam gesichert werden.
- Originaldokumente werden nicht ins Git geschrieben.
- Echte Testdaten bleiben auf der amixon-VM.

## Lokale KI

Ollama ist als lokaler KI-Provider festgelegt.

Offene konkrete Auswahl:

- Chatmodell
- Embeddingmodell
- Kontextgroesse
- Parallelitaet
- Antwortzeit-Ziel

Die RTX PRO 6000 macht lokale KI realistisch. Trotzdem muessen konkrete Modelle mit echten Such- und Antwortaufgaben getestet werden.

## OCR und Import

Startreihenfolge:

1. Excel strukturiert parsen.
2. Digitale PDFs ohne OCR extrahieren.
3. Lokale OCR fuer Scans ergaenzen.
4. Layout-/Tabellenerkennung mit echten Dokumenten vergleichen.

Kandidaten:

- Tesseract fuer einfache OCR
- PaddleOCR/PP-Structure fuer Layout und Tabellen
- Docling fuer strukturierte Dokumentkonvertierung

## Backup

Backup muss mindestens enthalten:

- PostgreSQL Dump oder physisches DB-Backup
- App-Storage
- Importartefakte, soweit fuer Nachvollziehbarkeit relevant
- Ollama-Modelle optional, da wiederherstellbar
- Konfiguration ohne Secrets

Wichtig:

Ein DB-Backup ohne Dokument-Storage ist wertlos. Ein Dokument-Storage ohne DB ist ebenfalls nicht ausreichend.

Festlegung:

- Backup taeglich
- DB und Datei-Storage gemeinsam sichern
- Retention nach ueblichem Schema:
  - taegliche Backups fuer kurzfristige Wiederherstellung
  - wöchentliche Backups fuer mittlere Frist
  - monatliche Backups fuer laengere Aufbewahrung
- genaue Aufbewahrungsdauer mit amixon final abstimmen

## Noch vor Installation klaeren

- genaue Linux-Distribution und Version
- Zugriff auf NVIDIA-Treiber und Container Runtime
- interner Hostname: `mixingai.amixon.local`
- TLS-Zertifikat aus interner CA oder anderer amixon-Quelle
- Backup-Ziel
- Wartungsfenster fuer Updates
