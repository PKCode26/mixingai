# Uebernahme aus erpforai

Quelle:

```text
C:\devprojekte\erpforai
```

Ziel:

Nicht das ERP kopieren, sondern die passenden Bausteine extrahieren und fuer MixingAI vereinfachen.

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

Relevante Dateien:

```text
src/ERP.Core/Domain/Roles/Role.cs
src/ERP.Core/Domain/Roles/RolePermission.cs
src/ERP.Core/Domain/Permissions/Permission.cs
src/ERP.Core/Domain/Users/UserRole.cs
src/ERP.Core/Authorization/PermissionCodes.cs
src/ERP.Api/Authorization/EndpointAuthorization.cs
```

Uebernahme:

- `User`
- `Role`
- `Permission`
- `UserRole`
- `RolePermission`
- `RequirePermissionAsync`
- `RequireAnyPermissionAsync`

Anpassen:

- PermissionCodes neu fuer MixingAI definieren
- keine ERP-Module uebernehmen
- Admin-Rolle beibehalten

Startrechte:

```text
admin.manage
document.upload
document.read
document.manage
import.review
recipe.read
recipe.manage
material.manage
analysis.run
ai.chat
```

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
- sichere Pfadauflösung im Storage-Service
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

Vorschlag:

```text
src/
  MixingAI.Api/
  MixingAI.Core/
  MixingAI.Dms/
  MixingAI.Import/
  MixingAI.Recipes/
  MixingAI.Infrastructure/
  MixingAI.SharedKernel/
tests/
  MixingAI.Tests/
```

Alternative fuer schnellen MVP:

```text
src/
  MixingAI.Api/
  MixingAI.Domain/
  MixingAI.Infrastructure/
tests/
  MixingAI.Tests/
```

Empfehlung:

Fuer den Start die zweite, kleinere Struktur. Modular schneiden, aber nicht zu viele Projekte erzeugen, bevor die Fachlogik stabil ist.

## Migrationsstrategie

1. Neues .NET-Projekt anlegen.
2. Shared Auditing uebernehmen.
3. User/Role/Permission/AuthSession uebernehmen und umbenennen.
4. AuthEndpoints auf MixingAI-Namespace und Rechte anpassen.
5. DbContext mit Core-Tabellen erstellen.
6. DMS-Entities vereinfacht uebernehmen.
7. Storage-Service uebernehmen und umbenennen.
8. Import- und Rezeptmodule neu bauen.
9. Erst danach KI-Service anbinden.

## Risiken beim Kopieren

- ERP-spezifische Abhaengigkeiten schleichen sich ein.
- Department-/DMS-Scope macht die Rechte zu komplex.
- Seed-Daten und Beispielmodule verwirren das neue Produkt.
- Frontend wird unnoetig schwer, wenn die alte Shell 1:1 uebernommen wird.

Deshalb: Code als Vorlage verwenden, aber bewusst neu zuschneiden.
