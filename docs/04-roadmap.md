# Roadmap

## Phase 0: Projektbasis

Ziele:

- Repo-Struktur anlegen
- Architektur dokumentieren
- Tech-Stack festlegen
- erpforai-Uebernahme schneiden

Ergebnis:

- dokumentierter Projektstart
- klare Modulgrenzen

## Phase 1: Administrative Basis

Ziele:

- .NET Backend anlegen
- PostgreSQL anbinden
- User und Sessions
- Login/Logout/Me
- einfache Admin-Seeds

Ergebnis:

- geschuetzte Anwendung
- einfache Anmeldung ohne komplexe Rechtekontrolle

## Phase 2: DMS / Document Vault

Ziele:

- Dokumente hochladen
- Dateien kontrolliert speichern
- Versionen abbilden
- SHA256-Hash berechnen
- Dubletten erkennen
- Dokumente suchen und filtern
- Archivieren statt loeschen

Ergebnis:

- Dateien liegen sauber im App-Storage
- DB kennt Dokumente, Versionen und Metadaten

## Phase 3: Import-Gate

Ziele:

- Importlaeufe modellieren
- Statusmodell umsetzen
- Excel-Dateien strukturiert lesen
- PDF-Text extrahieren
- OCR-Provider-Schnittstelle definieren
- Staging-Tabellen fuer erkannte Werte
- Validierungsfehler speichern

Ergebnis:

- Dateien werden nicht direkt produktiv
- Extraktionsergebnisse landen zuerst im Review

## Phase 4: Review-Maske

Ziele:

- erkannte Tabellen anzeigen
- erkannte Rezeptwerte korrigierbar machen
- Rohstoffe mappen
- Einheiten pruefen
- Quellen anzeigen
- Freigabeprozess fuer Rezeptdaten

Ergebnis:

- fachlich gepruefte Daten
- keine ungeprueften OCR-Ergebnisse in der Rezeptdatenbank

## Phase 5: Versuchs- und Rezeptdatenbank

Ziele:

- Versuche
- Versuchsdokumente
- Rezepte
- Rezeptversionen
- Rezeptpositionen
- Rohstoffe
- Rohstoffalias
- Einheiten
- Prozessparameter
- Pruefwerte
- Quellenverweise

Ergebnis:

- normale Suche und Filter
- Rezeptvergleich
- Versionsvergleich
- Export

## Phase 6: Ops / Datenqualitaet

Ziele:

- Import-Fehlerdashboard
- Dubletten-/Hash-Konflikte
- Validierungsregeln
- Job-Neustart
- Datenqualitaetsmetriken
- Audit-Ansicht

Ergebnis:

- Betrieb ist kontrollierbar
- schlechte Daten werden sichtbar, bevor sie Schaden machen

## Phase 7: KI-Assistenz

Ziele:

- KI-Service im Backend
- Tool-Schnittstellen fuer Suche und Vergleich
- lokales/on-prem LLM
- Chat nur auf freigegebenen Daten
- Quellenpflicht
- Rohstoff-Mapping-Vorschlaege
- natuerliche Sprache zu Datenbankfiltern

Ergebnis:

- Chat erleichtert Bedienung
- Kernsystem bleibt auch ohne KI nutzbar

## MVP-Schnitt

Minimaler MVP:

- Login
- Dokumentupload
- kontrollierter Storage
- Excel-Import fuer 1-2 reale Templates
- Review-Maske
- Versuchsdatenbank
- Rezeptdatenbank
- Suche nach Rohstoff, Rezept, Produktgruppe
- Suche nach Kunde, Versuchsnummer, Mischertyp und Maschine
- Quellenverweis auf Excel-Zelle oder PDF-Seite

Noch nicht im MVP:

- vollstaendiger Chatbot
- komplexe Korrelationen
- automatisches Training
- grosser DMS-Workflow
- allgemeine Fileserver-Synchronisation
