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

Das Cookie bleibt HttpOnly.

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
Ai__Provider=ollama
Ollama__BaseUrl=http://localhost:11434
Ollama__ChatModel=CHANGE_ME
Ollama__EmbeddingModel=CHANGE_ME
```

Die Offline-KI soll mit Ollama laufen. KI-Zugriff laeuft nur ueber das Backend, nie direkt aus dem Frontend. Der Chatbot wird spaeter nur ueber Backend-Tools auf freigegebene Daten zugreifen.

## Docker Compose

Die `.env.example` enthaelt zusaetzlich Variablen fuer Docker Compose:

```text
POSTGRES_PASSWORD=CHANGE_ME
FEATURE_ADMIN_SEED=false
FEATURE_AI_CHAT=false
FEATURE_OCR=false
AI_PROVIDER=ollama
OLLAMA_CHAT_MODEL=CHANGE_ME
OLLAMA_EMBEDDING_MODEL=CHANGE_ME
```

In Produktion duerfen echte Werte nur in `.env` oder in der Deployment-Umgebung liegen. `.env` bleibt aus dem Git ausgeschlossen.
