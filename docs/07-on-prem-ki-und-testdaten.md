# On-Prem-KI und Testdaten

## Grundsatz

MixingAI soll KI-Suche anbieten, aber produktive Kundendaten duerfen nicht in externe KI- oder Cloud-Systeme gelangen.

Deshalb gilt:

- KI lokal/on-prem betreiben
- OCR lokal/on-prem betreiben
- keine API-Keys fuer externe KI im Standardbetrieb
- keine Dokumentinhalte an externe Dienste senden
- Test mit echten Daten nur in der amixon-Umgebung

## On-Prem-KI-Architektur

```text
Frontend
  -> MixingAI Backend
  -> kontrollierte Backend-Tools
  -> PostgreSQL / Volltext / Vektorindex
  -> lokaler LLM-Service
```

Der LLM-Service spricht nicht direkt mit der Datenbank. Das Backend ruft definierte Funktionen auf, z.B.:

- `searchTrials(filters)`
- `searchRecipes(filters)`
- `getTrialSources(trialId)`
- `compareTrials(trialA, trialB)`
- `suggestMaterialAliases(rawName)`

Die KI formuliert Antworten und hilft beim Suchen, aber die Daten kommen aus dem Backend.

## Lokale LLM-Optionen

### Pilot: Ollama

Ollama ist fuer einen Pilot attraktiv, weil es lokale Modelle einfach bereitstellt und eine lokale HTTP-API anbietet.

Einsatz:

- schneller Start
- lokale Entwicklung
- erste Chat-/Suchassistenz
- kleine bis mittlere Modelle

Risiko:

- Performance haengt stark von VM/CPU/GPU ab
- fuer produktiven Mehrbenutzerbetrieb eventuell spaeter zu einfach

### Produktiver: vLLM

vLLM ist interessant, wenn eine GPU verfuegbar ist und ein OpenAI-kompatibler lokaler Server gewuenscht ist.

Einsatz:

- interne LLM-API
- bessere Serving-Performance
- OpenAI-kompatible Client-Schnittstelle
- geeignet fuer separaten GPU-Host

Risiko:

- mehr Infrastrukturaufwand
- GPU/VRAM muss geplant werden

## Lokale OCR- und Dokumentanalyse

### Digitale PDFs

Digitale PDFs sollten zuerst ohne OCR analysiert werden:

- Text extrahieren
- Tabellenbereiche erkennen
- Seiten- und Positionsinformationen speichern

### Scans

Bei gescannten PDFs braucht es OCR.

Optionen:

- Tesseract fuer einfache lokale OCR
- PaddleOCR/PP-Structure fuer Layout- und Tabellenerkennung
- Docling fuer strukturierte Dokumentkonvertierung und OCR-Pipeline

Die OCR-Komponente muss austauschbar bleiben. Der Import speichert immer Quelle, Confidence und erkannte Struktur.

## Embeddings und Suche

Auch semantische Suche muss lokal laufen.

Empfehlung:

- normale Filter und PostgreSQL-Volltext zuerst
- lokale Embedding-Modelle spaeter fuer semantische Aehnlichkeit
- Vektoren in PostgreSQL/pgvector oder einer lokalen Vektorloesung speichern

Keine Embeddings an Cloud-Provider senden.

## Testdatenstrategie

### Problem

Fuer gute Parser, Review-UI und KI-Suche braucht man echte Beispiele. Gleichzeitig duerfen Kundendaten nicht extern verarbeitet werden.

### Vorgehen

1. Synthetische Testdaten im Repo
   - kuenstliche PDFs
   - kuenstliche Excel-Uebersichten
   - absichtlich kaputte Varianten
   - keine echten Kundendaten

2. Anonymisierte Kundendaten nur nach Freigabe
   - Kundennamen ersetzen
   - Projektnummern ersetzen
   - Mengen optional veraendern
   - Originalstruktur erhalten

3. Echte Testdaten nur on-prem
   - Ablage auf der amixon-VM
   - nicht ins Git
   - nicht in Cloud-Tools
   - nicht in externe KI

4. Abnahme-Corpus
   - 20 bis 50 echte Dateien
   - gute PDFs
   - Scans
   - Excel-Uebersichten
   - Sonderfaelle
   - erwartete Extraktionsergebnisse als lokale Testreferenz

## Review-UI als Sicherheitsnetz

Da Dokumente uneinheitlich sein koennen, ist die Review-UI wichtiger als die Extraktions-KI.

Pflichtfunktionen:

- Originaldokument neben extrahierten Daten anzeigen
- erkannte Stelle markieren
- Wert editierbar machen
- Confidence anzeigen
- fehlende Pflichtfelder anzeigen
- Rohstoff-/Bezeichnungsalias bestaetigen
- Freigabe erst nach Review erlauben

## Offene technische Entscheidung

Vor Implementierung klaeren:

- Hat die amixon-Linux-VM eine GPU?
- Falls nein: reicht CPU-LLM fuer die erwartete Nutzung?
- Soll KI auf derselben VM oder separatem internen GPU-Host laufen?
- Welche OCR-Qualitaet liefern Tesseract, PaddleOCR und Docling auf echten Beispiel-PDFs?
- Welche Antwortzeit ist fuer die Suche akzeptabel?
