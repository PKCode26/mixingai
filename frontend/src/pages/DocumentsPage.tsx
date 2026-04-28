import { useRef, useState, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api, DuplicateError } from '../lib/api'
import type { Document } from '../types/documents'
import styles from './DocumentsPage.module.css'

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('de-DE', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function TypeBadge({ type }: { type: Document['documentType'] }) {
  const cls = type === 'Pdf' ? styles.typePdf : type === 'Excel' ? styles.typeExcel : styles.typeOther
  const label = type === 'Pdf' ? 'PDF' : type === 'Excel' ? 'Excel' : 'Datei'
  return <span className={`${styles.typeBadge} ${cls}`}>{label}</span>
}

export default function DocumentsPage() {
  const qc = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [search, setSearch] = useState('')
  const [includeArchived, setIncludeArchived] = useState(false)
  const [dragging, setDragging] = useState(false)
  const [uploadMsg, setUploadMsg] = useState<{ type: 'success' | 'error' | 'duplicate'; text: string } | null>(null)

  const { data: docs = [], isLoading } = useQuery({
    queryKey: ['documents', search, includeArchived],
    queryFn: () => api.documents.list(search || undefined, includeArchived),
  })

  const uploadMutation = useMutation({
    mutationFn: (file: File) => api.documents.upload(file),
    onSuccess: (doc) => {
      qc.invalidateQueries({ queryKey: ['documents'] })
      setUploadMsg({ type: 'success', text: `„${doc.originalFileName}" erfolgreich hochgeladen.` })
    },
    onError: (err) => {
      if (err instanceof DuplicateError) {
        setUploadMsg({
          type: 'duplicate',
          text: `Duplikat: „${err.duplicate.originalFileName}" ist bereits im System vorhanden.`,
        })
      } else {
        setUploadMsg({ type: 'error', text: `Fehler beim Upload: ${err.message}` })
      }
    },
  })

  const archiveMutation = useMutation({
    mutationFn: (id: string) => api.documents.archive(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['documents'] }),
  })

  const unarchiveMutation = useMutation({
    mutationFn: (id: string) => api.documents.unarchive(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['documents'] }),
  })

  const handleFiles = useCallback((files: FileList | null) => {
    if (!files || files.length === 0) return
    setUploadMsg(null)
    for (const file of Array.from(files)) {
      uploadMutation.mutate(file)
    }
  }, [uploadMutation])

  const onDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setDragging(false)
    handleFiles(e.dataTransfer.files)
  }, [handleFiles])

  const onDragOver = (e: React.DragEvent) => { e.preventDefault(); setDragging(true) }
  const onDragLeave = () => setDragging(false)

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <h1 className={styles.title}>Dokumente</h1>
        <button className={styles.uploadBtn} onClick={() => fileInputRef.current?.click()}>
          Datei hochladen
        </button>
      </div>

      <input
        ref={fileInputRef}
        type="file"
        multiple
        style={{ display: 'none' }}
        onChange={(e) => handleFiles(e.target.files)}
        accept=".pdf,.xlsx,.xls"
      />

      <div
        className={`${styles.dropzone} ${dragging ? styles.dropzoneActive : ''}`}
        onClick={() => fileInputRef.current?.click()}
        onDrop={onDrop}
        onDragOver={onDragOver}
        onDragLeave={onDragLeave}
      >
        {uploadMutation.isPending
          ? <p className={styles.uploadingText}>Wird hochgeladen …</p>
          : <p className={styles.dropzoneText}>
              Dateien hierher ziehen oder <strong>klicken</strong> zum Auswählen (PDF, Excel)
            </p>
        }
      </div>

      {uploadMsg && (
        <div className={`${styles.alert} ${
          uploadMsg.type === 'success' ? styles.alertSuccess
          : uploadMsg.type === 'duplicate' ? styles.alertWarning
          : styles.alertError
        }`}>
          {uploadMsg.text}
        </div>
      )}

      <div className={styles.toolbar}>
        <input
          className={styles.searchInput}
          type="text"
          placeholder="Dateiname suchen …"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <label className={styles.checkLabel}>
          <input
            type="checkbox"
            checked={includeArchived}
            onChange={(e) => setIncludeArchived(e.target.checked)}
          />
          Archivierte anzeigen
        </label>
      </div>

      {isLoading ? (
        <p className={styles.empty}>Lade …</p>
      ) : docs.length === 0 ? (
        <p className={styles.empty}>Keine Dokumente gefunden.</p>
      ) : (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Name</th>
              <th>Typ</th>
              <th>Größe</th>
              <th>Hash</th>
              <th>Hochgeladen</th>
              <th>Aktionen</th>
            </tr>
          </thead>
          <tbody>
            {docs.map((doc) => (
              <tr key={doc.id} className={doc.isArchived ? styles.archived : ''}>
                <td>{doc.displayName}</td>
                <td><TypeBadge type={doc.documentType} /></td>
                <td>{formatBytes(doc.fileSizeBytes)}</td>
                <td><span className={styles.hashText}>{doc.contentHash.slice(0, 12)}…</span></td>
                <td>{formatDate(doc.createdAtUtc)}</td>
                <td>
                  <div className={styles.actions}>
                    <button
                      className={styles.btnSmall}
                      onClick={() => api.documents.download(doc.id)}
                      title="Herunterladen"
                    >
                      ↓
                    </button>
                    {doc.isArchived ? (
                      <button
                        className={styles.btnSmall}
                        onClick={() => unarchiveMutation.mutate(doc.id)}
                        title="Wiederherstellen"
                      >
                        ↺
                      </button>
                    ) : (
                      <button
                        className={`${styles.btnSmall} ${styles.btnDanger}`}
                        onClick={() => archiveMutation.mutate(doc.id)}
                        title="Archivieren"
                      >
                        Archiv
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
