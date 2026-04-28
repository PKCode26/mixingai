# Massnahmenplan und Readiness

Dieses Dokument beschreibt die empfohlene Umsetzungsreihenfolge fuer MixingAI.

Es ist bewusst als Massnahmenplan mit Readiness-Kriterien aufgebaut, nicht nur als lose Aufgabenliste. Jede Phase hat ein klares Ergebnis und einfache Kriterien, wann sie als bereit fuer die naechste Phase gilt.

## Leitprinzip

```text
Technische Basis
  -> Dokumente kontrolliert speichern
  -> Import und Review
  -> Datenmodell stabilisieren
  -> Suche und Export
  -> KI anbinden
  -> Pilotbetrieb
```

Die Review-Maske ist der zentrale Produktbaustein. KI kommt erst, wenn die Datenbasis und die Such-/Filterfunktionen funktionieren.

## Aktueller Status

Stand im Repo:

- Phase 1 ist teilweise umgesetzt: Backend, Frontend, Docker Compose, Nginx Skeleton und Env-Vorlage existieren.
- Phase 2 ist teilweise umgesetzt: Auth-Grundlage, Login, Protected Route und Shared Shell existieren.
- Phase 3 ist der naechste groessere Schritt: Document Vault mit Upload, Storage, Hash und Dokumentliste.

Offene untracked Dateien im Arbeitsbaum muessen separat entschieden werden:

```text
.claude/
backend/MixingAI.Api/.dockerignore
```

Diese Doku nimmt sie nicht automatisch in den Commit auf.

## Phase 1: Projekt-Scaffold

Ziel:

Das Projekt ist technisch lauffaehig und hat eine belastbare Grundstruktur.

Massnahmen:

- .NET Backend anlegen
- React/Vite Frontend anlegen
- Docker Compose anlegen
- PostgreSQL-Service konfigurieren
- Ollama-Service vorbereiten
- Nginx-Konfiguration als Skeleton anlegen
- Storage-Pfade konfigurieren
- Basis-Konfiguration und `.env.example` anbinden
- Build- und Startbefehle dokumentieren

Readiness:

- Backend startet lokal
- Frontend startet lokal
- PostgreSQL ist erreichbar
- Docker Compose startet die Basisdienste
- Repo hat reproduzierbare Startbefehle
- keine Secrets im Git

Abhaengigkeiten:

- keine fachlichen Testdaten erforderlich

## Phase 2: Basis-App

Ziel:

Die interne Anwendung ist erreichbar, Benutzer koennen sich anmelden und die Shared Shell steht.

Massnahmen:

- einfache User-/Session-Logik implementieren
- Login/Logout/Me-Endpunkte bauen
- Seed-User fuer Entwicklung anlegen
- Shared Shell implementieren
- Startseite mit Kacheln bauen
- zweite Navigationsebene als Tabs/kompakte Navigation anlegen
- minimale Shared Design Tokens und Komponenten anlegen

Readiness:

- Benutzer kann sich anmelden und abmelden
- geschuetzte Seiten sind ohne Login nicht erreichbar
- Startseite zeigt Kacheln
- keine Sidebar
- Seiten nutzen dieselbe Shared Shell

Abhaengigkeiten:

- Phase 1 abgeschlossen

## Phase 3: Document Vault

Ziel:

PDF- und Excel-Dateien koennen kontrolliert in der Anwendung gespeichert werden.

Massnahmen:

- Dokument-Entity anlegen
- Dokumentversion anlegen
- Upload-Endpunkt fuer PDF/Excel
- lokaler App-Storage
- SHA256-Hash berechnen
- Dubletten erkennen
- Dokumentliste bauen
- Download/Vorschau vorbereiten
- leichte Versionierung abbilden

Readiness:

- Datei kann hochgeladen werden
- Datei wird im App-Storage abgelegt
- Metadaten stehen in PostgreSQL
- Hash ist gespeichert
- Dublette wird erkannt
- Dokument ist in der UI sichtbar

Abhaengigkeiten:

- Phase 2 abgeschlossen

## Phase 4: Import-Gate

Ziel:

Dateien werden nicht direkt produktiv, sondern erzeugen Importlaeufe und Staging-Daten.

Massnahmen:

- `import_runs` modellieren
- Statusmodell bauen
- Fehler-/Validierungsstatus bauen
- Excel-Parser fuer einfache Uebersicht vorbereiten
- digitale PDF-Text-Extraktion vorbereiten
- Staging-Struktur fuer erkannte Felder anlegen
- Importstatus in UI anzeigen

Readiness:

- Upload kann Importlauf erzeugen
- Importstatus ist sichtbar
- Fehler werden gespeichert
- erkannte Werte landen im Staging
- produktive Versuchsdaten bleiben unberuehrt

Abhaengigkeiten:

- Phase 3 abgeschlossen
- erste synthetische Beispiel-Dateien sinnvoll

## Phase 5: Review-Maske

Ziel:

Benutzer koennen erkennen, was eingelesen wurde und was tatsaechlich im Dokument steht.

Massnahmen:

- Review-Liste bauen
- Review-Detailseite bauen
- Dokumentvorschau links
- erkannte Felder rechts
- Quellenverweise anzeigen
- Werte editierbar machen
- Pflichtfelder markieren
- Confidence anzeigen
- Freigabeaktion bauen

Readiness:

- Reviewer sieht Originaldokument und extrahierte Werte nebeneinander
- Werte koennen korrigiert werden
- fehlende Pflichtfelder sind sichtbar
- Quellenverweis ist nachvollziehbar
- Freigabe erzeugt produktive Daten

Abhaengigkeiten:

- Phase 4 abgeschlossen
- mindestens synthetische Testdateien vorhanden

## Phase 6: Erstes Datenmodell stabilisieren

Ziel:

Das produktive Datenmodell fuer Versuche, Dokumente, Rezept-/Mischdaten und Quellen ist fuer den MVP ausreichend stabil.

Massnahmen:

- echte Dokumentvarianten on-prem sichten
- Trial-Modell schneiden
- Document/Source-Modell pruefen
- Recipe/Material-Modell nur soweit noetig bauen
- Pflichtfelder definieren
- Indexe fuer Startfilter anlegen
- Migrationen stabilisieren

Readiness:

- Versuche koennen produktiv gespeichert werden
- Dokumentquellen bleiben nachvollziehbar
- Startfilter sind technisch abbildbar
- neue Felder koennen spaeter ergaenzt werden
- keine groben Schema-Annahmen blockieren echte Daten

Abhaengigkeiten:

- Phase 5 abgeschlossen
- erste echte oder realistische Testdokumente verfuegbar

## Phase 7: Suche und Filter

Ziel:

Benutzer koennen ohne KI nach Versuchen und relevanten Feldern suchen.

Massnahmen:

- Versuchsliste bauen
- Filter nach Kunde, Produkt, Versuchsnummer, Mischertyp, Baugroesse, Maschine, Zeitraum, Status
- Volltextsuche fuer relevante Textfelder
- Detailseite fuer Versuch
- einfache Quellenanzeige

Readiness:

- Suchergebnisse sind stabil
- Filter sind kombinierbar
- Detailseite zeigt Versuch + Dokumentquelle
- Suche funktioniert ohne KI

Abhaengigkeiten:

- Phase 6 abgeschlossen

## Phase 8: Excel-Export

Ziel:

Suchergebnisse koennen als Excel mit Quellenverweisen exportiert werden.

Massnahmen:

- Exportmodell definieren
- Excel-Export fuer Versuchsliste bauen
- Quellenverweise als Spalten ausgeben
- Filterkontext optional im Export ablegen

Readiness:

- gefilterte Ergebnisse koennen exportiert werden
- Export enthaelt Quellenverweise
- Export ist fuer Fachanwender lesbar

Abhaengigkeiten:

- Phase 7 abgeschlossen

## Phase 9: On-Prem KI mit Ollama

Ziel:

KI-Suche hilft bei der Bedienung, ohne Daten an externe Dienste zu senden.

Massnahmen:

- Ollama-Service anbinden
- Chatmodell konfigurieren
- Embeddingmodell konfigurieren
- Backend-Tools fuer Suche definieren
- KI darf nur freigegebene Daten abfragen
- Antwort mit Quellen erzeugen
- einfache KI-Suchseite bauen

Readiness:

- Ollama laeuft on-prem
- KI greift nur ueber Backend-Tools zu
- keine direkte DB-Verbindung fuer KI
- keine externen KI-APIs
- Antworten enthalten Quellen

Abhaengigkeiten:

- Phase 7 abgeschlossen
- Phase 8 optional, aber hilfreich
- Modellwahl auf Zielhardware getestet

## Phase 10: OCR-Provider festlegen

Ziel:

Fuer gescannte oder schlecht strukturierte PDFs wird ein lokaler OCR-/Dokumentanalysepfad festgelegt.

Massnahmen:

- echte Beispiel-PDFs on-prem testen
- Tesseract testen
- PaddleOCR/PP-Structure testen
- Docling testen
- Qualitaet, Geschwindigkeit und Quellenverweise vergleichen
- Provider fuer MVP festlegen

Readiness:

- OCR-Provider laeuft on-prem
- Quellenverweise sind ausreichend
- Qualitaet reicht fuer Review-Vorschlaege
- schlechte Faelle landen sauber in Review/Fehlerstatus

Abhaengigkeiten:

- echte Testdateien auf amixon-VM
- Phase 4/5 nutzbar

## Phase 11: Deployment vorbereiten

Ziel:

Das System kann auf der amixon-VM betrieben werden.

Massnahmen:

- Docker Compose fuer Produktion/Pilot finalisieren
- Nginx + TLS einrichten
- Hostname `mixingai.amixon.local` konfigurieren
- persistente Volumes unter `/srv/mixingai`
- Backup-Script fuer PostgreSQL + Storage
- Restore-Prozess dokumentieren
- Logging und Basis-Monitoring einrichten

Readiness:

- App ist ueber HTTPS erreichbar
- Container starten reproduzierbar
- Daten bleiben persistent
- Backup laeuft taeglich
- Restore wurde mindestens einmal getestet

Abhaengigkeiten:

- Infrastrukturzugriff auf amixon-VM
- TLS-Zertifikat aus amixon-Umgebung

## Phase 12: Pilotdaten und Abnahme

Ziel:

Der MVP wird mit echten Dokumenten in der amixon-Umgebung validiert.

Massnahmen:

- 20 bis 50 echte Dateien on-prem sammeln
- Import durchfuehren
- Review durchlaufen
- Such-/Exportfaelle pruefen
- KI-Suche testen
- Datenmodell nachjustieren
- UI-Reibungspunkte beheben

Readiness:

- echte Dokumente koennen verarbeitet werden
- Review ist fachlich brauchbar
- Suche beantwortet Startfragen
- Export ist verwendbar
- offene Datenmodell-Anpassungen sind bekannt

Abhaengigkeiten:

- Phasen 1 bis 11 ausreichend abgeschlossen

## Steuerungsansicht

| Phase | Ergebnis | Blockiert durch |
| --- | --- | --- |
| 1 Scaffold | Projekt laeuft technisch | keine |
| 2 Basis-App | Login + Shell | Phase 1 |
| 3 Document Vault | Dateien sauber gespeichert | Phase 2 |
| 4 Import-Gate | Staging statt Direktimport | Phase 3 |
| 5 Review | Daten gegen Quelle pruefbar | Phase 4 |
| 6 Datenmodell | MVP-Schema stabil | Phase 5 + Testdaten |
| 7 Suche | Filter ohne KI | Phase 6 |
| 8 Export | Excel mit Quellen | Phase 7 |
| 9 KI | Ollama-Suche mit Quellen | Phase 7 |
| 10 OCR | lokaler OCR-Pfad | echte PDFs |
| 11 Deployment | HTTPS-Betrieb | Infrastruktur |
| 12 Pilot | echte Abnahme | alles davor |

## Wichtigste Reihenfolge

```text
Scaffold
  -> Login + Shared Shell
  -> Upload + Document Vault
  -> Import-Gate
  -> Review-Maske
  -> Datenmodell stabilisieren
  -> Suche/Export
  -> Ollama-KI
  -> OCR nach echten Daten
  -> Deployment/Pilot
```
