# Ideen und Optimierungen

Diese Liste sammelt Ideen, die nicht zwingend fuer den MVP sind, aber spaeter sinnvoll werden koennen.

## Prioritaets-Legende

```text
P1  bald pruefen, wahrscheinlich wertvoll
P2  nach MVP sinnvoll
P3  spaeter / optional
```

## Produktideen

| ID | Prioritaet | Idee | Nutzen |
| --- | --- | --- | --- |
| I-001 | P1 | Review-Maske mit Dokumentstelle markieren | Schnellere fachliche Pruefung. |
| I-002 | P1 | Alias-Vorschlaege fuer Rohstoffe und Maschinen | Weniger manuelle Normalisierung. |
| I-003 | P1 | Excel-Export mit Quellenverweisen | Direkter Nutzen fuer Fachanwender. |
| I-004 | P2 | Vergleich zweier Versuche | Abweichungen zwischen aehnlichen Versuchen erkennen. |
| I-005 | P2 | Rezept-/Mischdaten als eigene Vergleichsansicht | Bessere technische Analyse. |
| I-006 | P2 | Saved Filters | Wiederkehrende Suchmuster schneller abrufen. |
| I-007 | P2 | Import-Vorlagen fuer bekannte PDF-/Excel-Layouts | Schnellere und stabilere Extraktion. |
| I-008 | P2 | Qualitaetsdashboard fuer Importfehler | Ops sieht Problemklassen schneller. |
| I-009 | P3 | PDF-Report fuer Versuchsdaten | Weitergabe ohne Excel. |
| I-010 | P3 | Volltextsuche ueber OCR-Text mit Highlighting | Schnelleres Auffinden einzelner Begriffe. |

## KI-Ideen

| ID | Prioritaet | Idee | Nutzen |
| --- | --- | --- | --- |
| AI-001 | P1 | Natuerliche Sprache in Suchfilter uebersetzen | KI macht Bedienung leichter, Datenbank bleibt fuehrend. |
| AI-002 | P1 | Antwort immer mit Quellen anzeigen | Vertrauen und Pruefbarkeit. |
| AI-003 | P2 | Aehnliche Versuche finden | Hilft bei Recherche und Wiederverwendung. |
| AI-004 | P2 | Rohstoff-/Maschinenalias vorschlagen | Beschleunigt Stammdatenaufbau. |
| AI-005 | P2 | Importfehler erklaeren | Review und Ops werden schneller. |
| AI-006 | P3 | Zusammenfassung eines Versuchs | Schnellueberblick im Detailscreen. |
| AI-007 | P3 | Korrelationen vorschlagen | Nur sinnvoll, wenn genug Ergebnis-/Pruefdaten vorhanden sind. |

## Technische Optimierungen

| ID | Prioritaet | Idee | Nutzen |
| --- | --- | --- | --- |
| T-001 | P1 | Parser-Schnittstellen strikt provider-neutral halten | OCR/Parser austauschbar. |
| T-002 | P1 | Import-Staging flexibler als produktives Schema halten | Uneinheitliche Dokumente besser abfangen. |
| T-003 | P1 | DB + Storage Restore-Test automatisieren | Betriebssicherheit. |
| T-004 | P2 | Worker als separaten Container auslagern | Bessere Skalierung von Import/OCR. |
| T-005 | P2 | pgvector fuer lokale semantische Suche testen | Aehnlichkeitssuche ohne Cloud. |
| T-006 | P2 | Queue fuer Importjobs einfuehren | Stabilere Verarbeitung bei groesseren Mengen. |
| T-007 | P3 | Observability mit Metriken/Dashboard | Nuetzlich nach Pilotstart. |
| T-008 | P3 | Rollen/Rechte spaeter erweitern | Nur falls mehr Nutzergruppen entstehen. |

## UI-Optimierungen

| ID | Prioritaet | Idee | Nutzen |
| --- | --- | --- | --- |
| UI-001 | P1 | Startkacheln mit Statuszahlen | Nutzer sieht offene Reviews und Fehler sofort. |
| UI-002 | P1 | Review-Tastaturbedienung fuer schnelle Korrektur | Effizient bei vielen Dokumenten. |
| UI-003 | P2 | Spaltenkonfiguration fuer Versuchsliste | Fachanwender koennen ihre Sicht anpassen. |
| UI-004 | P2 | Export direkt aus gefilterter Liste | Weniger Klicks. |
| UI-005 | P3 | Dokumentvorschau mit Treffer-Highlighting | Bessere Nachvollziehbarkeit. |

## Nicht fuer den MVP

Diese Punkte bleiben bewusst ausserhalb des MVP:

- komplexe Rollen-/Rechteverwaltung
- vollwertiges allgemeines DMS
- Cloud-KI
- Cloud-OCR
- grosse Portalnavigation
- mobile Optimierung als Hauptziel
- automatische Freigabe ohne Review

## Pflege

Ideen werden in konkrete Aufgaben ueberfuehrt, wenn:

- sie den MVP direkt verbessern
- echte Nutzerprobleme im Pilot auftauchen
- technische Risiken reduziert werden
- ausreichend echte Daten fuer die Funktion vorhanden sind
