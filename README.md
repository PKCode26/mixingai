# MixingAI

MixingAI ist ein kontrolliertes Rezept- und Dokumentensystem fuer industrielle Mischungsdaten, z.B. Zement- und Baustoffrezepturen aus PDF- und Excel-Dateien.

Das System ist bewusst kein "Chatbot ueber Dokumente". Der Kern ist eine gepruefte Datenbank mit sauberem Import-Gate, Dokumentenarchiv, Versuchs-/Rezeptdatenmodell und normalen Such-/Filterfunktionen. KI wird spaeter als Assistenzschicht angebunden.

## Zielbild

```text
Altbestand Windows-Fileserver
  -> kontrollierte Migration / Upload
  -> App-Storage mit Hash und Version
  -> Import- und Extraktionslauf
  -> Review- und Korrekturmaske
  -> freigegebene Versuchs- und Rezeptdatenbank
  -> Suche, Filter, Vergleich, Export
  -> optional KI-Chat als Bedienhilfe
```

Der Windows-Fileserver ist nur Quelle fuer die initiale Migration. Nach der Uebernahme ist die Anwendung das fuehrende System fuer diese Versuchs-, Rezept- und Dokumentdaten.

## Kernbereiche

1. User, Rollen und Rechte
2. Ops / Datenqualitaet
3. DMS / Dokumentenarchiv
4. Versuchs- und Rezeptdatenbank
5. Import- und Review-Maske
6. KI-Chatbot als spaeterer Zusatz

## Dokumentation

- [Architektur](docs/01-architektur.md)
- [Tech-Stack](docs/02-tech-stack.md)
- [Uebernahme aus erpforai](docs/03-erpforai-uebernahme.md)
- [Roadmap](docs/04-roadmap.md)
- [Umgebungsvariablen](docs/05-umgebungsvariablen.md)
- [Grundsatzentscheidungen](docs/06-grundsatzentscheidungen.md)
- [On-Prem-KI und Testdaten](docs/07-on-prem-ki-und-testdaten.md)
- [Deployment-Zielsetup](docs/08-deployment-zielsetup.md)
- [UI- und Produktleitplanken](docs/09-ui-und-produktleitplanken.md)
- [Datenmodell-Inputfaktoren](docs/10-datenmodell-inputfaktoren.md)

## Grundsatz

Die KI ist nicht die Datenbank und nicht das fuehrende System.

Backend, Datenbank, Validierungsregeln, Berechtigungen und Audit-Logs entscheiden, welche Daten gelten. KI hilft beim Import, bei Rohstoff-Mapping, bei Suche und bei der Bedienung.
