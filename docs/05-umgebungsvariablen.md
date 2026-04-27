# Umgebungsvariablen

Echte Secrets werden nicht ins Git geschrieben.

Die Datei `.env.example` ist die Vorlage fuer lokale Entwicklung. Fuer echte Umgebungen wird daraus lokal eine `.env` oder es werden System-/Deployment-Umgebungsvariablen gesetzt.

## Grundvariablen

```text
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5085
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=mixingai_dev;Username=postgres;Password=CHANGE_ME
```

## Storage

```text
Storage__DocumentRootPath=storage/documents
Storage__ImportRootPath=storage/imports
```

Der Storage enthaelt Originaldateien, Importkopien und spaeter ggf. OCR-Artefakte. Diese Ordner werden nicht committed.

## Auth

```text
Auth__CookieName=mixingai_auth
Auth__SessionDays=7
```

Das Cookie bleibt HttpOnly. Die genaue Implementierung wird aus dem `erpforai`-Auth-Pattern abgeleitet und fuer MixingAI vereinfacht.

## Feature Flags

```text
Features__EnableAdminSeedEndpoint=true
Features__EnableAiChat=false
Features__EnableOcrProvider=false
```

KI und OCR sollen explizit aktivierbar sein. Der MVP muss ohne KI lauffaehig bleiben.

## OCR

```text
Ocr__Provider=disabled
AzureDocumentIntelligence__Endpoint=
AzureDocumentIntelligence__ApiKey=
```

OCR-Provider werden austauschbar gehalten. Azure Document Intelligence ist ein moeglicher Provider, aber keine harte Architektur-Abhaengigkeit.

## KI

```text
Ai__Provider=disabled
OpenAI__ApiKey=
OpenAI__Model=
```

KI-Zugangsdaten duerfen nie im Frontend landen. Der Chatbot wird spaeter nur ueber Backend-Tools auf freigegebene Daten zugreifen.
