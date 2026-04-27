# Uebernahme aus erpforai

Quelle:

```text
C:\devprojekte\erpforai
```

Ziel:

Nicht das ERP kopieren, sondern die passenden Bausteine extrahieren und fuer MixingAI vereinfachen.

## Aktueller Stand

Die Auth-Grundlage ist im MixingAI-Repo bereits angelegt:

```text
backend/MixingAI.Api/Core/Auth/AuthSession.cs
backend/MixingAI.Api/Core/Users/User.cs
backend/MixingAI.Api/Core/Security/CurrentUserService.cs
backend/MixingAI.Api/Core/Security/PasswordHashingService.cs
backend/MixingAI.Api/Core/Security/SessionTokenService.cs
backend/MixingAI.Api/Core/Endpoints/AuthEndpoints.cs
backend/MixingAI.Api/Infrastructure/Data/AppDbContext.cs
```

Sie ist bewusst kleiner als `erpforai`:

- keine Rollenverwaltung im MVP
- keine Gruppen
- keine Department-/Employee-Bezuege
- `IsAdmin` nur fuer einfache Admin-Unterscheidung
- alle normalen angemeldeten Nutzer duerfen fachlich erstmal alles

Die DMS-/Document-Vault-Uebernahme steht noch aus.

## Uebernehmen

### Auth und Sessions

Relevante Dateien aus `erpforai`:

```text
src/ERP.Api/Endpoints/AuthEndpoints.cs
src/ERP.Infrastructure/Security/CurrentUserService.cs
src/ERP.Infrastructure/Security/PasswordHashingService.cs
src/ERP.Infrastructure/Security/SessionTokenService.cs
src/ERP.Infrastructure/Security/RequestUserContext.cs
src/ERP.Core/Domain/Auth/AuthSession.cs
src/ERP.Core/Domain/Users/User.cs
```

Uebernahme:

- Passwort-Hashing mit ASP.NET PasswordHasher uebernehmen
- Session-Token mit SHA256-Hash uebernehmen
- HttpOnly-Cookie-Pattern uebernehmen
- Login, Logout und Me-Endpunkte uebernehmen
- User-Kontext fuer Rechtepruefung uebernehmen

Anpassen:

- Cookie-Name von `erp_auth` auf z.B. `mixingai_auth`
- ERP-spezifische Felder entfernen
- Department-/Employee-Abhaengigkeiten entfernen
- User-Kontext auf MixingAI-Rechte reduzieren

### Rollen und Rechte

Urspruenglich relevante Dateien:

```text
src/ERP.Core/Domain/Roles/Role.cs
src/ERP.Core/Domain/Roles/RolePermission.cs
src/ERP.Core/Domain/Permissions/Permission.cs
src/ERP.Core/Domain/Users/UserRole.cs
src/ERP.Core/Authorization/PermissionCodes.cs
src/ERP.Api/Authorization/EndpointAuthorization.cs
```

Aktuelle Entscheidung:

- Rollen-/Permission-Modell aus `erpforai` wird im MVP nicht uebernommen.
- MixingAI nutzt einfache Anmeldung und optionales `IsAdmin`.
- Eine spaetere Rechteerweiterung bleibt moeglich, ist aber kein MVP-Ziel.

Nicht uebernehmen:

- `Role`
- `Permission`
- `UserRole`
- `RolePermission`
- komplexe PermissionCodes
- Rollen-/Rechte-UI

### DMS-Grundmodell

Relevante Dateien:

```text
src/ERP.Dms/Domain/Documents/Document.cs
src/ERP.Dms/Domain/Documents/DocumentVersion.cs
src/ERP.Infrastructure/Storage/LocalDocumentFileStorageService.cs
```

Uebernehmen:

- Dokumentmetadaten
- Dokumentversionen
- Storage-Pfad statt DB-Blob
- Originaldateiname
- MIME-Type
- Dateigroesse
- SHA256-Checksumme
- sichere Pfadaufloesung im Storage-Service
- Archivieren statt hart loeschen

Anpassen:

- QM-/ISO-Felder entfernen
- DMS-Freigabe-/Review-Workflow nicht 1:1 uebernehmen
- DocumentType einfacher halten
- Quellenverweise fuer Excel/PDF/OCR ergaenzen
- Importstatus und Extraktionsstatus ergaenzen

### Auditing

Relevante Konzepte:

```text
src/ERP.SharedKernel/Domain/Auditing
```

Uebernehmen:

- `CreatedAtUtc`
- `CreatedByUserId`
- `UpdatedAtUtc`
- `UpdatedByUserId`

Anpassen:

- Audit-Events fuer Import, Review, Freigabe und Rezeptaenderungen ergaenzen

## Nicht uebernehmen

Nicht uebernehmen:

- ERP-Modulstruktur
- Sales, SCM, HR, FiCo, Inventory
- Department- und Employee-Scope
- DMS-QM-/ISO-Speziallogik
- grosser `SeedDataRunner`
- statische Vanilla-JS-App als Produktfrontend
- ERP-Navigation und Shell-Komplexitaet

Begruendung:

MixingAI soll kleiner und fokussierter sein. Zu viel ERP-Struktur wuerde das Projekt schwerer machen, ohne dem Kundenproblem zu helfen.

## Zielstruktur in MixingAI

Aktuelle Struktur:

```text
backend/MixingAI.Api/
  Core/
  Dms/
  Import/
  Recipe/
  Ai/
  Infrastructure/

frontend/
nginx/
docker-compose.yml
docker-compose.dev.yml
```

Empfehlung:

Die aktuelle Single-API-Struktur ist fuer den MVP passend. Fachbereiche bleiben als Ordner getrennt, ohne frueh mehrere .NET-Projekte zu erzwingen.

## Migrationsstrategie

1. Neues .NET-Projekt anlegen. `erledigt`
2. Shared Auditing uebernehmen. `erledigt`
3. User/AuthSession uebernehmen und vereinfachen. `erledigt`
4. AuthEndpoints auf MixingAI-Namespace anpassen. `erledigt`
5. DbContext mit Core-Tabellen erstellen. `erledigt`
6. DMS-Entities vereinfacht uebernehmen. `offen`
7. Storage-Service uebernehmen und umbenennen. `offen`
8. Import- und Rezeptmodule neu bauen. `offen`
9. KI-Service fuer Ollama anbinden. `offen`

## Risiken beim Kopieren

- ERP-spezifische Abhaengigkeiten schleichen sich ein.
- Department-/DMS-Scope macht die Rechte zu komplex.
- Seed-Daten und Beispielmodule verwirren das neue Produkt.
- Frontend wird unnoetig schwer, wenn die alte Shell 1:1 uebernommen wird.

Deshalb: Code als Vorlage verwenden, aber bewusst neu zuschneiden.
