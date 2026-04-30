# Grundsatzentscheidungen

Dieses Dokument sammelt fruehe Produkt- und Architekturentscheidungen, die vor der Implementierung bewusst getroffen werden sollten.

## Bereits entschieden

### MVP nur mit einfacher Anmeldung

Im MVP gibt es nur einfache Benutzeranmeldung.

Nicht vorgesehen:

- keine Gruppenverwaltung
- keine komplexe Rechte-Matrix
- keine fachlichen Zugriffsbeschraenkungen pro Modul
- keine Mandanten-/Department-Scopes

Grundsatz:

Alle angemeldeten Benutzer duerfen im MVP grundsaetzlich alles. Die Installation laeuft auf einer geschlossenen VM und wird nur von wenigen Personen genutzt.

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

Festgelegt:

- Betrieb auf einer Linux-VM bei amixon.
- Keine produktiven Kundendaten in externe KI-Systeme.
- Keine produktiven Kundendaten in externe Cloud-OCR.
- KI-Suche bleibt Ziel des Produkts, muss aber lokal/on-prem laufen.
- Die Offline-KI soll mit Ollama betrieben werden.

### Leichte Dokumentversionierung

Die Dokumente sind keine gelenkten Dokumente.

Grundsatz:

- Jeder Versuch hat im Normalfall eine eigene Datei.
- Versionierung ist nicht der zentrale Fachprozess.
- Eine neue Version ist trotzdem hilfreich, wenn eine Datei korrigiert oder erneut importiert wird.
- Es braucht keinen grossen DMS-Freigabeworkflow.

## Versuchsprotokolle als Referenz

Die vorliegende Beispiel-Dokumentation beschreibt Versuchsprotokolle, aber diese Struktur ist nicht garantiert immer identisch.

Sie wird als Startpunkt fuer die Ordnung genutzt, nicht als starres Schema.

### Fachlicher Kern noch offen

Das System braucht eine fachliche Klammer fuer Versuchsprotokolle, Rezepturen, Prozessparameter, Messwerte und Dokumentquellen. Ob diese Klammer im produktiven Datenmodell hauptsaechlich `Trial`/Versuch, `Recipe`/Rezeptur oder eine Mischform ist, wird erst nach Sichtung echter Dokumente entschieden.

Moegliche Trial-Struktur, falls der Versuch die fuehrende Klammer bildet:

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

Moegliche Recipe-Struktur, falls Rezepturen der fuehrende Such- und Vergleichsanker sind:

```text
Recipe / Rezeptur
  -> Rezeptkennung oder Bezug zum Versuch
  -> Produkt
  -> Rezeptpositionen / Bestandteile
  -> Menge roh aus Dokument
  -> Menge normalisiert
  -> Einheit
  -> Toleranz / Zielwert falls vorhanden
  -> Prozessparameter
  -> Dokumentquelle
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
- klare Trennung zwischen Staging und freigegebenem Fachmodell
- Rohwerte und normalisierte Werte getrennt speichern

## Noch zu klaeren

### 1. MVP-Schnitt

Festgelegt fuer den MVP:

- Login
- Dokumentupload
- kontrollierter Storage
- PDF-Dokument als Versuchsprotokoll erfassen
- Excel-Uebersicht importieren oder referenzieren
- gute Review-Maske fuer eingelesene Daten gegen Originaldokument
- einfache fachliche Datenbank fuer Versuche/Rezepturen
- Rezept-/Mischdaten falls vorhanden
- Suche/Filter nach Kunde, Produkt, Versuch, Maschine, Mischertyp
- Excel-Export mit Quellenverweisen

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

Festgelegt:

- Linux-VM bei amixon
- PostgreSQL und App-Storage werden dort betrieben
- KI/OCR-Komponenten laufen ebenfalls dort oder in derselben on-prem Umgebung
- Docker Compose wird fuer den MVP verwendet
- Nginx und TLS/HTTPS werden fuer Datenschutz und sauberen Betrieb fest eingeplant
- Zielhardware: AMD Ryzen Threadripper 7960X, 128 GB RAM, 2 x 2 TB NVMe RAID1, NVIDIA RTX PRO 6000

### 4. Cloud-Verbot genau definieren

Festgelegt fuer den Start:

- keine Cloud-KI fuer produktive Daten
- keine Cloud-OCR fuer produktive Daten
- externe Dienste nur fuer synthetische oder explizit freigegebene Testdaten
- Standardkonfiguration bleibt offline/on-prem

Noch offen:

- NVIDIA-Treiber / Container Runtime
- interner Hostname und TLS-Zertifikat aus amixon-Umgebung
- TLS-Zertifikat wird am Ende eingerichtet
- konkretes Ollama-Chatmodell und Embeddingmodell: neuestes passendes stabiles Modell, final nach Test auf Zielhardware

### 5. Dokument- und Versionierungsregeln

Grundsatz:

- jeder Versuch ist normalerweise ein eigenes Dokument
- Versionierung bleibt leichtgewichtig
- Dubletten werden ueber Hash erkannt
- erneuter Upload kann als Korrekturversion abgebildet werden

Noch zu definieren:

- ein neues Dokument?
- eine neue Version?
- eine Dublette?
- ein Anhang?
- ein ersetztes Protokoll?
- ein PDF, das zu einer Excel-Zeile gehoert?

### 6. Freigaberegeln

Wichtigster MVP-Prozess:

Die Review-Maske muss klar zeigen:

- was eingelesen wurde
- wo es im Dokument steht
- wie sicher die Extraktion ist
- was korrigiert wurde
- ob Pflichtfelder fehlen

Vor Freigabe sollte mindestens geprueft sein:

- Versuchsnummer vorhanden
- Kunde gesetzt
- Produkt gesetzt
- Quelle verknuepft
- Mengen plausibel
- Einheiten gueltig
- Rohstoffe gemappt
- Reviewer gesetzt

### 7. Such- und Filteranforderungen

Startfilter:

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

Grundsatz:

Filter muessen spaeter erweiterbar sein. Welche Filter wirklich wichtig werden, ergibt sich nach den ersten importierten Versuchen.

### 8. Rohstoff- und Bezeichnungsnormalisierung

Es gibt noch keine sichere Stammdatenbasis.

Grundsatz:

- Rohstoffe, Maschinenbezeichnungen und Synonyme werden zuerst aus den Dokumenten herausgelesen.
- Die Review-Maske muss daraus Vorschlaege machen.
- Bestaetigte Zuordnungen werden als Aliase/Stammdaten gespeichert.

Zu klaeren:

- gibt es Materialnummern?
- gibt es Lieferantennamen?
- gibt es Synonymlisten?
- muessen Rohstoffe kundenspezifisch gemappt werden?
- duerfen mehrere Aliase auf einen Rohstoff zeigen?

### 9. Echte Testdaten

Fuer den Pilot werden echte Dateien benoetigt, aber sie duerfen nicht in externe KI-/Cloud-Systeme gelangen.

Empfehlung:

- 20 bis 50 Dokumente
- gute PDFs
- schlechte/scanned PDFs
- typische Excel-Uebersichten
- alte Benennungsvarianten
- Sonderfaelle mit mehreren Geraeten

Vorgehen:

- echte Testdaten bleiben bei amixon/on-prem
- fuer Entwicklung ausserhalb der Kundenumgebung werden synthetische oder anonymisierte Dateien genutzt
- Parser- und UI-Tests koennen mit kuenstlich erstellten Fixtures laufen
- fachliche Abnahme laeuft mit echten Dateien auf der amixon-VM

### 10. Backup und Restore

DB und Storage muessen gemeinsam gesichert werden.

Festgelegt:

- Backup taeglich
- DB und Storage gemeinsam sichern
- uebliches Retention-Schema mit taeglichen, woechentlichen und monatlichen Backups

Noch zu klaeren:

- genaues Backup-Ziel
- genaue Aufbewahrungsdauer
- Restore-Test
- wer betreibt Backups?

### 11. Audit-Tiefe

Audit ist im MVP nur minimal relevant, da es interne Daten und wenige Benutzer sind.

Minimal erfassen:

- Upload
- Datei-Version
- Importlauf
- manuelle Korrektur
- Freigabe
- Archivierung

### 12. Export

Festgelegt:

- Excel-Export ist wichtig
- Export soll Quellenverweise enthalten

Optional spaeter:

- CSV fuer Analysen
- PDF-Report fuer Versuch/Rezept
