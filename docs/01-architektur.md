# Architektur

## Produktdefinition

MixingAI ist ein geprueftes Versuchs-, Rezept- und Dokumentensystem mit KI-Assistenz.

Die Anwendung soll viele PDF- und Excel-Dateien mit Versuchs-, Mischungs- und Rezepturdaten kontrolliert uebernehmen, strukturieren, pruefen und anschliessend durchsuchbar machen. Der Chatbot ist eine Komfortschicht, nicht das Basissystem.

## Systemgrenzen

### Fuehrendes System

Nach der Migration ist MixingAI das fuehrende System fuer:

- Rezeptdokumente
- Versuchsprotokolle
- Dokumentversionen
- Versuchsdaten
- extrahierte Rezeptdaten
- Rohstoffe und Synonyme
- Quellenverweise
- Freigabestatus
- normale Suche, Filter und Vergleiche

### Fileserver

Der bestehende Windows-Fileserver wird nur fuer den Altbestand genutzt:

- schrittweises Einlesen bestehender PDF-/Excel-Dateien
- optional Speicherung des originalen Pfads als Referenz
- keine dauerhafte Live-Abhaengigkeit

MixingAI kopiert importierte Dateien in einen kontrollierten App-Storage. Damit bleiben Hash, Version, Quelle und Audit nachvollziehbar, auch wenn am Fileserver spaeter Dateien umbenannt oder verschoben werden.

## Hauptmodule

### 1. User und Rechte

Aufgaben:

- Login
- Rollen und Rechte
- Sitzungen
- Zugriff auf Dokumente, Import, Review, Rezeptdaten und KI steuern
- Audit-Informationen fuer Aenderungen speichern

Empfohlene Startrollen:

- Admin
- Rezeptmanager
- Pruefer
- Leser

Empfohlene Startrechte:

- `admin.manage`
- `document.upload`
- `document.read`
- `document.manage`
- `import.review`
- `recipe.read`
- `recipe.manage`
- `material.manage`
- `analysis.run`
- `ai.chat`

### 2. Ops / Datenqualitaet

Ops ist kein technischer Luxus, sondern die Sperre gegen schlechte Daten.

Aufgaben:

- Importlaeufe ueberwachen
- Fehler anzeigen
- Dubletten erkennen
- Hash-Konflikte erkennen
- Validierungsfehler sammeln
- Jobs neu starten
- Inkonsistenzen sichtbar machen
- verhindern, dass ungepruefte Extraktionsergebnisse produktiv werden

Wichtig: Es gibt eine klare Trennung zwischen Staging-Daten und freigegebenen Rezeptdaten.

### 3. DMS / Dokumentenarchiv

Das DMS ist hier ein schlanker Document Vault, kein grosses Workflow-DMS.

Aufgaben:

- Originaldateien speichern
- Dokumentmetadaten verwalten
- Versionen abbilden
- Datei-Hash speichern
- MIME-Type und Dateigroesse speichern
- OCR-/Volltextdaten speichern
- Archivieren statt hart loeschen
- Quellen fuer extrahierte Werte bereitstellen

Beispiele fuer Quellenverweise:

- Excel: Datei, Version, Sheet, Zelle, Tabellenbereich
- PDF: Datei, Version, Seite, Tabellenzeile, Bounding Box
- OCR: Seite, Textblock, Wortposition, Confidence

### 4. Versuchs- und Rezeptdatenbank

Die Versuchs- und Rezeptdatenbank ist das fachliche Ziel des Imports.

Versuchsprotokolle sind als flexible fachliche Quelle zu behandeln. Eine Beispielstruktur kann Felder wie Versuchsnummer, Kunde, Produkt, Aufgabenstellung, Testapparat, Mischertyp, Baugroesse, Fabrikatnummer, Sonderausstattung, Befuellung und Gesamtmenge enthalten. Diese Struktur ist ein Referenzpunkt, aber kein starres Schema fuer alle Altdokumente.

Kernobjekte:

- Versuch / Trial
- Versuchsdokument
- Rezept
- Rezeptversion
- Rezeptposition
- Rohstoff
- Rohstoffalias
- Einheit
- Prozessparameter
- Pruefwert / Eigenschaftswert
- Quellenverweis

Beispiele fuer Abfragen:

- alle Rezepte mit Rohstoff X
- alle Versuche eines Kunden
- alle Versuche mit bestimmtem Mischertyp oder bestimmter Baugroesse
- alle Rezepturen mit Rohstoff X ueber 8 Prozent
- Rezept A und Rezept B vergleichen
- Rezeptversionen vergleichen
- Rohstoffvarianten finden
- Rezepte nach Produktgruppe, Kunde, Datum oder Eigenschaft filtern

### 5. Import- und Review-Maske

Die Review-Maske ist der wichtigste Fachprozess.

Aufgaben:

- Datei hochladen
- Dokumenttyp setzen
- erkannte Tabellen anzeigen
- Rohstoffe mappen
- Mengen und Einheiten pruefen
- Prozessparameter pruefen
- Pruefwerte pruefen
- Dubletten oder neue Versionen erkennen
- Daten fuer die Rezeptdatenbank freigeben

Statusmodell:

```text
uploaded
  -> queued
  -> extracting
  -> needs_review
  -> approved
  -> published
  -> archived
```

Fehlerstatus:

```text
failed
rejected
needs_rework
```

### 6. KI-Chatbot

Der Chatbot kommt zuletzt.

Regeln:

- KI hat keinen direkten Schreibzugriff auf die Datenbank.
- KI greift nur ueber Backend-Tools zu.
- KI nutzt nur freigegebene Daten fuer produktive Antworten.
- KI-Antworten zeigen Quellen.
- KI darf Vorschlaege machen, aber keine fachliche Wahrheit festlegen.

Beispiele fuer Backend-Tools:

- `searchRecipes(filters)`
- `compareRecipes(recipeVersionA, recipeVersionB)`
- `findSimilarRecipes(recipeVersionId)`
- `suggestMaterialMapping(rawMaterialName)`
- `getDocumentSource(sourceId)`
- `runCorrelation(metricA, metricB, filters)`

## Datenfluss

```text
Datei
  -> App-Storage
  -> document_versions
  -> import_runs
  -> extraction_staging
  -> Review
  -> trials / recipes / recipe_versions / recipe_lines
  -> Suche / Vergleich / KI
```

## Staging vs. produktive Daten

Extraktionsergebnisse sind Vorschlaege.

Produktive Rezeptdaten entstehen erst nach Review/Freigabe. Dadurch koennen OCR-Fehler, falsche Tabellenbereiche, unklare Einheiten und Rohstoff-Synonyme abgefangen werden.

## Audit und Nachvollziehbarkeit

Jede produktive Rezeptinformation sollte nachvollziehbar sein:

- wer hat importiert?
- aus welcher Datei?
- aus welcher Version?
- aus welcher Seite/Zelle/Tabelle?
- wer hat geprueft?
- wann wurde freigegeben?
- was wurde korrigiert?

## Nicht-Ziele fuer den MVP

- kein vollstaendiges ERP
- kein allgemeines Unternehmens-DMS
- kein direkter Chat ueber ungeordnete Fileserver-Ordner
- kein automatisches Vertrauen in OCR-Ergebnisse
- kein KI-System mit direktem DB-Schreibzugriff
