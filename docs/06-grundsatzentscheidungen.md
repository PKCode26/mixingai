# Grundsatzentscheidungen

Dieses Dokument sammelt fruehe Produkt- und Architekturentscheidungen, die vor der Implementierung bewusst getroffen werden sollten.

## Bereits entschieden

### Kein Dokumenten-Chat als Kern

MixingAI wird nicht als RAG-Chatbot ueber einen ungeordneten Dokumentenhaufen gebaut.

Der Kern ist:

```text
Dokumente
  -> kontrollierter Import
  -> Review
  -> strukturierte Datenbank
  -> Suche / Filter / Vergleich
  -> optional KI-Assistenz
```

### Fileserver nur als Altquelle

Der Windows-Fileserver wird fuer den Altbestand genutzt, aber nicht als dauerhaft fuehrende Datenquelle.

Importierte Dateien werden in den App-Storage kopiert und dort versioniert, gehasht und nachvollziehbar abgelegt.

### KI ist optional

MixingAI muss ohne KI produktiv nutzbar sein.

KI darf spaeter helfen bei:

- Suche in natuerlicher Sprache
- Rohstoff-/Bezeichnungs-Mapping
- Zusammenfassungen
- Importvorschlaegen
- Bedienhilfe

KI darf nicht:

- die Datenbank ersetzen
- ungepruefte Daten produktiv machen
- direkt in die produktive DB schreiben
- ohne Freigabe Dokumentinhalte an externe Anbieter senden

### On-Prem als Grundannahme

Da Kundendaten nicht in externe KI-/Cloud-Systeme gelangen sollen, wird On-Prem oder private Infrastruktur als Zielannahme behandelt.

Folgen:

- Cloud-KI ist standardmaessig aus.
- Cloud-OCR ist standardmaessig aus.
- KI-/OCR-Provider werden austauschbar gehalten.
- Die Anwendung funktioniert auch ohne KI.
- Externe Verarbeitung darf nur explizit aktiviert werden.

## Versuchsprotokolle als Referenz

Die vorliegende Beispiel-Dokumentation beschreibt Versuchsprotokolle, aber diese Struktur ist nicht garantiert immer identisch.

Sie wird als Startpunkt fuer die Ordnung genutzt, nicht als starres Schema.

### Fachliches Kernobjekt

Neben Rezepten braucht das System ein Objekt fuer Versuche bzw. Versuchsprotokolle.

Moegliche Struktur:

```text
Trial / Versuch
  -> Versuchsnummer
  -> Kunde
  -> Produkt
  -> Ziel / Aufgabenstellung
  -> Teilnehmer kundenseitig
  -> Teilnehmer intern
  -> Testapparat / Geraet
  -> Mischertyp
  -> Baugroesse
  -> Fabrikatnummer / Maschinennummer
  -> Sonderausstattung
  -> Befuellung
  -> Gesamtmenge
  -> Dokumentquelle
  -> optionale Rezeptdaten
  -> optionale Prozessparameter
  -> optionale Ergebnis-/Pruefwerte
```

### Dokumentnamen als Signal

Die Dateibezeichnung kann wichtige Metadaten enthalten, z.B.:

```text
Versuchsnummer Kunde Mischertyp Baugroesse-Fabrikatnummer
```

Weil Kundenbezeichnungen Leerzeichen enthalten koennen und Altdaten uneinheitlich sein koennen, darf der Dateiname nur als Importvorschlag dienen. Die Review-Maske muss die Werte bestaetigen oder korrigieren lassen.

### Excel-Uebersicht als zweite Quelle

Falls Ziele, Aufgabenstellungen oder Zusatzdaten in einer Excel-Uebersicht gepflegt wurden, muss das System PDF-Protokolle und Excel-Zeilen verknuepfen koennen.

Beispiel:

```text
trial
  -> pdf document version
  -> excel overview document version
  -> excel sheet
  -> excel row
```

### Flexible Extraktion

Da Protokolle nicht immer gleich aufgebaut sind, braucht der Import:

- Pflichtfelder, die fuer Freigabe notwendig sind
- optionale Felder
- Confidence je erkanntem Wert
- Quellenverweis je erkanntem Wert
- Review statt automatischer Wahrheit
- flexible Feldgruppen fuer seltene Zusatzinformationen

## Noch zu klaeren

### 1. MVP-Schnitt

Welche Funktionen muessen in der ersten lauffaehigen Version enthalten sein?

Empfohlener MVP:

- Login
- Dokumentupload
- kontrollierter Storage
- PDF-Dokument als Versuchsprotokoll erfassen
- Excel-Uebersicht importieren oder referenzieren
- Review-Maske fuer Versuchsdaten
- einfache Versuchsdatenbank
- Rezept-/Mischdaten falls vorhanden
- Suche/Filter nach Kunde, Produkt, Versuch, Maschine, Mischertyp

Nicht im MVP:

- KI-Chat
- komplexe Korrelationen
- vollautomatische Klassifikation
- grosser DMS-Workflow

### 2. Mandantenmodell

Ist das System nur fuer einen Kunden gedacht oder spaeter fuer mehrere Kunden/Mandanten?

Empfehlung:

Single-Tenant im MVP, aber Datenmodell so schneiden, dass `tenant_id` spaeter moeglich bleibt.

### 3. Betriebsmodell

Wo laeuft die Anwendung?

Optionen:

- Kunde on-prem
- private VM
- eigener Server
- Docker/Compose
- spaeter Kubernetes nur bei echtem Bedarf

Diese Entscheidung beeinflusst OCR, KI, Backup, Updates und Storage.

### 4. Cloud-Verbot genau definieren

Muss wirklich alles offline/on-prem bleiben, oder sind einzelne externe Dienste nach Freigabe erlaubt?

Zu klaeren:

- duerfen Dokumente an Cloud-OCR?
- duerfen Metadaten an KI?
- duerfen anonymisierte Auszuege an KI?
- muss ein lokales LLM verwendet werden?
- reicht "KI standardmaessig aus"?

### 5. Dokument- und Versionierungsregeln

Was ist:

- ein neues Dokument?
- eine neue Version?
- eine Dublette?
- ein Anhang?
- ein ersetztes Protokoll?
- ein PDF, das zu einer Excel-Zeile gehoert?

### 6. Freigaberegeln

Welche Felder muessen geprueft sein, bevor ein Versuch oder Rezept produktiv wird?

Beispiele:

- Versuchsnummer vorhanden
- Kunde gesetzt
- Produkt gesetzt
- Quelle verknuepft
- Mengen plausibel
- Einheiten gueltig
- Rohstoffe gemappt
- Reviewer gesetzt

### 7. Such- und Filteranforderungen

Welche Abfragen sind fuer den Kunden wirklich wichtig?

Beispiele:

- nach Kunde
- nach Produkt
- nach Versuchsnummer
- nach Mischertyp
- nach Baugroesse
- nach Maschinennummer
- nach Rohstoff
- nach Menge/Anteil
- nach Ergebnis-/Pruefwert
- nach Zeitraum

### 8. Rohstoff- und Bezeichnungsnormalisierung

Welche Stammdaten sind fuehrend?

Zu klaeren:

- gibt es Materialnummern?
- gibt es Lieferantennamen?
- gibt es Synonymlisten?
- muessen Rohstoffe kundenspezifisch gemappt werden?
- duerfen mehrere Aliase auf einen Rohstoff zeigen?

### 9. Echte Testdaten

Fuer den Pilot werden echte Dateien benoetigt.

Empfehlung:

- 20 bis 50 Dokumente
- gute PDFs
- schlechte/scanned PDFs
- typische Excel-Uebersichten
- alte Benennungsvarianten
- Sonderfaelle mit mehreren Geraeten

### 10. Backup und Restore

DB und Storage muessen gemeinsam gesichert werden.

Zu klaeren:

- Backup-Intervall
- Restore-Test
- Aufbewahrungszeit
- wer betreibt Backups?
- wie werden Datei-Storage und DB konsistent gesichert?

### 11. Audit-Tiefe

Welche Aktionen muessen nachvollziehbar sein?

Empfehlung:

- Upload
- Datei-Version
- Importlauf
- Extraktionsvorschlag
- manuelle Korrektur
- Freigabe
- Archivierung
- Rohstoff-Mapping
- Rezept-/Versuchsaenderung
- KI-Abfrage optional, wenn aktiviert

### 12. Export

Welche Daten muessen wieder raus?

Moeglich:

- Excel-Export fuer Suchergebnisse
- PDF-Report fuer Versuch/Rezept
- CSV fuer Analysen
- Export inklusive Quellenverweisen
