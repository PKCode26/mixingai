# UI- und Produktleitplanken

## Grundsatz

MixingAI ist eine interne Fachanwendung fuer wenige Benutzer. Das UI soll ruhig, direkt und arbeitsorientiert sein.

Nicht Ziel:

- keine Marketing-Oberflaeche
- keine grosse Portalnavigation
- keine Sidebar
- keine aufwendigen Animationen
- keine ueberladene Design-Sprache

Ziel:

- schnell erfassbare Kacheln
- klare zweite Navigationsebene
- einfache Tabellen
- gute Review-Maske
- konsistente Shared Shell
- minimaler, aber fester Shared Design Standard

## Shared Shell

Von Anfang an wird eine Shared Shell genutzt.

Die Shell enthaelt:

- Kopfbereich mit Produktname
- aktueller Benutzer / Logout
- Hauptnavigation als Kacheln oder kompakte Modulnavigation
- konsistente Inhaltsbreite
- einheitliche Page-Titel
- einheitliche Feedback-/Fehleranzeigen

Keine Sidebar.

## Startseite

Die Startseite besteht aus klickbaren Kacheln.

Startkacheln:

- Import
- Review
- Versuche
- Dokumente
- Suche
- Export
- KI-Suche
- Betrieb

Die KI-Suche kann im MVP sichtbar, aber deaktiviert oder als spaeterer Bereich markiert sein.

## Zweite Navigationsebene

Innerhalb eines Bereichs gibt es eine einfache zweite Navigationsebene, z.B. Tabs oder kompakte Textbuttons.

Beispiel Import:

- Upload
- Importlaeufe
- Fehler

Beispiel Review:

- Offen
- In Bearbeitung
- Freigegeben

Beispiel Versuche:

- Liste
- Detail
- Vergleich

## Design

Das Design darf sich leicht an amixon orientieren, aber ohne grossen Aufwand.

Leitplanken:

- helle, sachliche Business-Oberflaeche
- dezente Akzentfarbe
- Tabellen und Formulare vor Kartenoptik
- Kacheln nur fuer Start-/Modulnavigation
- klare Statusfarben fuer Import/Review
- keine dekorativen Grafiken
- gute Lesbarkeit auf Desktop

## Review-Maske

Die Review-Maske ist der wichtigste Screen.

Zielaufbau:

```text
links:  Dokumentvorschau
rechts: erkannte Felder und Tabellen
oben:   Versuchskopf / Status / Aktionen
unten:  Validierungsfehler oder Quellenliste
```

Wichtig:

- Originalstelle im Dokument sichtbar machen
- extrahierte Werte daneben editierbar machen
- Confidence anzeigen
- fehlende Pflichtfelder markieren
- Freigabe erst nach Mindestpruefung erlauben

## Umsetzung

Beim Scaffolding direkt anlegen:

- Shared Layout-Komponente
- Shared CSS/Tokens
- Page-Header-Komponente
- Kachel-Komponente
- Tab-/Second-Level-Navigation
- Feedback-/Alert-Komponente
- einfache Table-Styles
- Form-Styles

Der Standard bleibt bewusst klein, damit die Anwendung nicht im UI-Framework stecken bleibt.
