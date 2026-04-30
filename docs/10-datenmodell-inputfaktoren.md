# Datenmodell-Inputfaktoren

Das endgueltige Datenmodell wird nicht aus Annahmen gebaut, sondern aus den realen Dokumenten, Suchzielen und Review-Prozessen abgeleitet.

Diese Liste beschreibt, welche Informationen wir frueh sammeln muessen, um das Schema sauber zu schneiden.

## Leitentscheidung offen

Der fachliche Kern wird nicht vorab hart festgelegt. Ob `Trial`/Versuch, `Recipe`/Rezeptur oder eine Kombination daraus das fuehrende Objekt wird, ergibt sich aus:

- was in den realen Dokumenten stabil wiederkehrt
- wonach die Nutzer zuerst suchen und filtern
- welche Daten im Review sicher bestaetigt werden koennen
- welche Quellenbeziehungen spaeter im Export und in der KI-Antwort sichtbar sein muessen

Das Import-Staging bleibt deshalb bewusst flexibler als das spaetere produktive Fachmodell.

## 1. Dokumentarten

Zu klaeren:

- Welche Dateitypen gibt es?
- PDF digital oder Scan?
- Excel-Uebersicht, Excel-Rezeptur oder beides?
- Gibt es Anhaenge, Fotos, Messdaten, Rohstoffdatenblaetter?
- Gibt es pro Versuch immer genau ein PDF?
- Gibt es Versuche ohne Excel-Zeile?
- Gibt es Excel-Zeilen ohne PDF?

Einfluss aufs Datenmodell:

- `documents`
- `document_versions`
- `document_type`
- Verknuepfung `document -> fachliches Objekt` (Trial, Recipe oder Mischform; wird anhand echter Daten festgelegt)
- optionale Anhaenge

## 2. Dokumentstruktur

Zu klaeren:

- Sind Versuchsprotokolle ein- oder mehrseitig?
- Sind Felder immer gleich benannt?
- Gibt es Tabellen?
- Gibt es Freitextbereiche?
- Gibt es handschriftliche oder gescannte Inhalte?
- Gibt es mehrere Geraete/Mischer in einem Versuch?
- Welche Informationen stehen nur im Dateinamen?

Einfluss aufs Datenmodell:

- flexible Staging-Felder
- Quellenverweise mit Seite, Bounding Box, Tabelle, Zeile, Zelle
- Pflichtfelder vs. optionale Felder
- mehrere Equipment-Zuordnungen pro fachlichem Datensatz, falls mehrere Geraete getestet werden

## 3. Identifikation eines Versuchs

Zu klaeren:

- Was ist die eindeutige Versuchsnummer?
- Ist die Versuchsnummer immer vorhanden?
- Kann eine Versuchsnummer mehrfach vorkommen?
- Ist Kunde + Versuchsnummer eindeutig?
- Gibt es Serien, Varianten oder Wiederholungen?
- Wie wird eine Korrekturdatei erkannt?

Einfluss aufs Datenmodell:

- `trials`
- eindeutige Constraints
- Dublettenlogik
- leichte Versionierung
- Statusmodell

## 4. Kunde und Beteiligte

Zu klaeren:

- Wie wird der Kunde geschrieben?
- Gibt es Kundennummern?
- Gibt es mehrere Teilnehmer kundenseitig?
- Gibt es mehrere interne Teilnehmer?
- Sind Teilnehmer fuer Suche relevant oder nur Metadaten?

Einfluss aufs Datenmodell:

- `customers`
- `customer_aliases`
- `trial_participants`
- Freitext vs. normalisierte Stammdaten

## 5. Produkt und Aufgabenstellung

Zu klaeren:

- Was ist "Produkt" im fachlichen Sinn?
- Ist Produkt ein Freitext oder Stammdatensatz?
- Kommt Ziel/Aufgabenstellung aus PDF oder Excel?
- Gibt es Produktgruppen?
- Muss nach Ziel/Aufgabe gesucht werden?

Einfluss aufs Datenmodell:

- `products`
- `product_aliases`
- `trial.objective`
- Volltextfelder
- Excel-Zeilen-Verknuepfung

## 6. Maschine, Mischertyp und Ausstattung

Zu klaeren:

- Welche Maschinen-/Geraetebezeichnungen gibt es?
- Ist Mischertyp getrennt von Baugroesse?
- Ist Fabrikatnummer gleich Maschinennummer?
- Kann ein Versuch mehrere Testapparate haben?
- Sind Sonderausstattungen strukturiert oder Freitext?
- Gibt es amixon-interne Maschinenstammdaten?

Einfluss aufs Datenmodell:

- `equipment`
- `equipment_aliases`
- `trial_equipment`
- Felder fuer Mischertyp, Baugroesse, Serien-/Fabrikatnummer
- optionale Ausstattungstabellen

## 7. Misch-/Rezepturdaten

Zu klaeren:

- Gibt es in jedem Versuch eine Rezeptur?
- Sind Rezepturdaten im PDF, Excel oder beidem?
- Sind Mengen absolut, prozentual oder beides?
- Welche Einheiten kommen vor?
- Muss Summe 100 Prozent ergeben?
- Gibt es Reihenfolgen der Zugabe?
- Gibt es Varianten innerhalb eines Versuchs?

Einfluss aufs Datenmodell:

- `recipes`
- `recipe_versions`
- `recipe_lines`
- `units`
- `unit_conversions`
- `line_order`
- Plausibilitaetsregeln

## 8. Rohstoffe und Synonyme

Zu klaeren:

- Gibt es Materialnummern?
- Gibt es Lieferantennamen?
- Gibt es interne Rohstoffstammdaten?
- Sind Rohstoffe kundenspezifisch benannt?
- Gibt es Schreibvarianten?
- Muss ein Rohstoffalias durch Review bestaetigt werden?

Einfluss aufs Datenmodell:

- `materials`
- `material_aliases`
- `material_sources`
- Mapping-Status
- Review-Pflicht fuer neue Aliase

## 9. Prozessparameter

Zu klaeren:

- Welche Prozessparameter kommen vor?
- Mischzeit?
- Drehzahl?
- Temperatur?
- Feuchte?
- Fuellmenge?
- Reihenfolge?
- Chargenparameter?
- Sind Parameter tabellarisch oder Freitext?

Einfluss aufs Datenmodell:

- `trial_process_parameters`
- flexible Parametertypen
- Einheit je Parameter
- Quellenverweise

## 10. Ergebnis- und Pruefwerte

Zu klaeren:

- Gibt es Ergebnisdaten im Protokoll?
- Welche Messwerte sind relevant?
- Sind sie numerisch oder Freitext?
- Gibt es Zielwerte / Bewertung / bestanden?
- Sind Ergebnisdaten Voraussetzung fuer Korrelationen?

Einfluss aufs Datenmodell:

- `trial_measurements`
- `measurement_types`
- numerische Werte + Einheit
- Freitextbewertungen
- spaetere Analyse-/Korrelationsfunktionen

## 11. Suche und Filter

Zu klaeren:

- Welche Filter muessen im MVP funktionieren?
- Welche Filter entstehen spaeter aus echten Daten?
- Welche Felder muessen normalisiert sein?
- Welche Felder reichen als Volltext?
- Welche Suchergebnisse muessen exportierbar sein?

Startfilter:

- Kunde
- Produkt
- Versuchsnummer
- Mischertyp
- Baugroesse
- Maschine/Fabrikatnummer
- Rohstoff
- Zeitraum
- Status

Einfluss aufs Datenmodell:

- Indexe
- Stammdatentabellen
- Volltextspalten
- Exportprojektionen

## 12. Quellenverweise

Zu klaeren:

- Muss jeder Wert einen Quellenverweis haben?
- Reicht Datei + Seite?
- Brauchen wir Bounding Boxes?
- Brauchen wir Excel-Zelle/Sheet/Zeile?
- Muss im Review die Originalstelle markiert werden?

Einfluss aufs Datenmodell:

- `extraction_sources`
- `field_sources`
- `page_number`
- `sheet_name`
- `cell_address`
- `table_index`
- `row_index`
- `bounding_box`

## 12a. OCR-Text und Freitext

Zu klaeren:

- Muss OCR-Text seitenweise oder blockweise gespeichert werden?
- Sind Freitextbereiche fachlich Teil des Versuchs, der Rezeptur oder nur Dokumentnotizen?
- Welche Freitexttypen kommen vor, z.B. Verarbeitungshinweis, Warnhinweis, Pruefnotiz, Kundenkommentar?
- Muss der Nutzer spaeter ueber Freitext suchen koennen?
- Soll die KI spaeter Freitext semantisch durchsuchen?

Einfluss aufs Datenmodell:

- `document_text_segments`
- `text_type`
- `document_id`
- optionale Verknuepfung zu Trial/Recipe/Fachdatensatz
- `page_number`, `sheet_name`, `cell_address`, `bounding_box`
- Volltextindex und spaeter optional Vektorindex

Grundsatz:

Freitext wird nicht pauschal an eine Rezeptur gehaengt. Er bekommt einen eigenen Textsegment-/Notizdatensatz und wird nur dann mit Trial oder Recipe verknuepft, wenn diese Beziehung im Review klar ist.

## 12b. Rohwerte, Normalisierung und Einheiten

Zu klaeren:

- Welche Dezimaltrennzeichen kommen vor?
- Werden Prozent, Kilogramm, Gramm, Liter und Tonnen gemischt?
- Werden Mengen absolut, relativ oder beides angegeben?
- Gibt es Zielwert, Istwert, Toleranz und Grenzwert?
- Welche Werte duerfen automatisch normalisiert werden?
- Welche Werte brauchen manuelle Bestaetigung?

Einfluss aufs Datenmodell:

- `raw_value`
- `numeric_value`
- `unit`
- `normalized_value`
- `normalization_status`
- `validation_issue`

Grundsatz:

OCR- oder Parserwerte werden nicht blind ueberschrieben. Ein Wert wie `1,5 %` bleibt als Rohwert erhalten; der normalisierte Zahlenwert wird separat gespeichert und muss bei Unsicherheit validiert werden.

## 12c. Staging vs. Produktivschema

Zu klaeren:

- Welche Staging-Felder werden zu echten Spalten?
- Welche bleiben flexible Attribute?
- Welche Felder sind nur Importdiagnose?
- Welche Quellen muessen nach Freigabe dauerhaft erhalten bleiben?

Einfluss aufs Datenmodell:

- `staged_fields` bleibt Import-/Review-Zwischenschicht
- produktive Tabellen entstehen erst nach Review/Freigabe
- `extraction_sources` oder `field_sources` verbinden produktive Werte mit Dokumentstellen
- Startfilter bekommen echte Spalten oder indexierte Projektionen

## 13. Review und Freigabe

Zu klaeren:

- Welche Felder sind Pflicht?
- Welche Felder duerfen leer bleiben?
- Welche Werte brauchen manuelle Bestaetigung?
- Wann gilt ein Versuch als freigegeben?
- Kann ein freigegebener Versuch korrigiert werden?
- Was muss im Audit stehen?

Einfluss aufs Datenmodell:

- `review_status`
- `validation_issues`
- `review_decisions`
- `approved_at`
- `approved_by`
- minimale Audit-Felder

## 14. Export

Zu klaeren:

- Welche Excel-Spalten braucht der Kunde?
- Exportiert man Versuche, Rezeptpositionen oder beides?
- Sollen Quellenverweise als eigene Spalten ausgegeben werden?
- Sollen Filter im Export dokumentiert werden?

Einfluss aufs Datenmodell:

- Export-Views
- flache Query-Modelle
- Quellenformat
- ggf. gespeicherte Exportjobs

## 15. Testdaten und Abnahme

Zu klaeren:

- Welche 20 bis 50 Dateien repraesentieren den echten Bestand?
- Welche Sonderfaelle muessen enthalten sein?
- Welche Extraktionsergebnisse gelten als korrekt?
- Welche Fehler duerfen automatisch erkannt werden?

Einfluss aufs Datenmodell:

- Pflichtfelder
- Staging-Flexibilitaet
- Parser-Regeln
- Validierungsregeln
- Review-UI

## Praktisches Vorgehen

1. Echte Beispielmenge on-prem sammeln.
2. Dokumentarten und Varianten klassifizieren.
3. Pro Dokument 10 bis 20 erwartete Felder markieren.
4. Suchfragen des Kunden danebenlegen.
5. Lokales Ollama-Analysewerkzeug ueber Feldinventar und Textausschnitte laufen lassen.
6. KI-Vorschlaege fachlich pruefen, nicht automatisch uebernehmen.
7. Erstes kanonisches Fachschema ableiten; Trial/Recipe-Schwerpunkt erst anhand echter Daten entscheiden.
8. Quellen-/Provenienzmodell fuer produktive Werte festlegen.
9. Rohwert-, Zahlenwert- und Einheitenmodell fuer Mengen/Parameter festlegen.
10. Import-Staging bewusst flexibler bauen als produktive Tabellen.
11. Review-Maske gegen echte Dokumente testen.
12. Danach Migrationsschema stabilisieren.

## Merksatz

Das Datenmodell richtet sich nach drei Dingen:

```text
Was steht wirklich in den Dokumenten?
Was muss der Nutzer spaeter suchen/exportieren?
Was muss im Review nachvollziehbar geprueft werden?
```
