import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { StagedField, OllamaStatus } from '../types/imports'
import styles from './ImportReviewPage.module.css'

const BASE = '/api'

const REQUIRED_FIELDS = new Set([
  'Datum',
  'Versuchsnummer',
  'Kunde',
  'Mischzeit',
  'Gesamtmenge',
])

const STATUS_LABELS: Record<string, string> = {
  Queued: 'Warteschlange', Extracting: 'Extraktion…', NeedsReview: 'Review nötig',
  Approved: 'Freigegeben', Published: 'Publiziert', Archived: 'Archiviert',
  Failed: 'Fehler', Rejected: 'Abgelehnt', NeedsRework: 'Nacharbeit',
}

const CONFIDENCE_CLASS = (c: number | null) => {
  if (c === null) return styles.confHigh
  if (c >= 0.85) return styles.confHigh
  if (c >= 0.70) return styles.confMid
  return styles.confLow
}

function formatConfidence(c: number | null) {
  if (c === null) return '—'
  return `${Math.round(c * 100)} %`
}

function EditableField({
  field, runId, onSaved,
}: { field: StagedField; runId: string; onSaved: () => void }) {
  const [editing, setEditing] = useState(false)
  const [value, setValue] = useState(field.fieldValue ?? '')
  const qc = useQueryClient()

  const saveMutation = useMutation({
    mutationFn: () => api.imports.confirmField(runId, field.id, true, value),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['imports', runId, 'staged'] })
      setEditing(false)
      onSaved()
    },
  })

  if (editing) {
    return (
      <div className={styles.editWrap}>
        <input
          className={styles.editInput}
          value={value}
          onChange={e => setValue(e.target.value)}
          onKeyDown={e => {
            if (e.key === 'Enter') saveMutation.mutate()
            if (e.key === 'Escape') { setValue(field.fieldValue ?? ''); setEditing(false) }
          }}
          autoFocus
        />
        <button className={styles.btnSave} onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending}>✓</button>
        <button className={styles.btnCancel} onClick={() => { setValue(field.fieldValue ?? ''); setEditing(false) }}>✕</button>
      </div>
    )
  }

  return (
    <div
      className={`${styles.fieldValue} ${field.isConfirmed ? styles.fieldConfirmed : ''}`}
      onClick={() => setEditing(true)}
      title="Klicken zum Bearbeiten"
    >
      {field.fieldValue ?? <span className={styles.empty}>—</span>}
      {field.isConfirmed && <span className={styles.confirmedBadge}>✓</span>}
    </div>
  )
}

export default function ImportReviewPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [ocrMsg,  setOcrMsg]  = useState<{ ok: boolean; text: string } | null>(null)
  const [aiMsg,   setAiMsg]   = useState<{ ok: boolean; text: string } | null>(null)

  const { data: run, isLoading: runLoading } = useQuery({
    queryKey: ['imports', id],
    queryFn: () => api.imports.get(id!),
    refetchInterval: run => run.state.data?.status === 'Extracting' ? 3000 : false,
  })

  const { data: fields = [] } = useQuery({
    queryKey: ['imports', id, 'staged'],
    queryFn: () => api.imports.stagedFields(id!),
    enabled: !!id,
  })

  const { data: issues = [] } = useQuery({
    queryKey: ['imports', id, 'issues'],
    queryFn: () => api.imports.issues(id!),
    enabled: !!id,
  })

  const { data: images = [] } = useQuery({
    queryKey: ['imports', id, 'images'],
    queryFn: () => api.imports.images(id!),
    enabled: !!id,
  })

  const { data: ocrStatus } = useQuery({
    queryKey: ['ocr', 'status'],
    queryFn: () => api.imports.ocrStatus(),
  })

  const { data: ollamaStatus } = useQuery({
    queryKey: ['ollama', 'status'],
    queryFn: () => api.imports.ollamaStatus(),
  })

  const approveMutation = useMutation({
    mutationFn: () => api.imports.approve(id!),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['imports'] })
      navigate('/dokumente')
    },
  })

  const rejectMutation = useMutation({
    mutationFn: () => api.imports.reject(id!),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['imports'] })
      navigate('/dokumente')
    },
  })

  const ocrMutation = useMutation({
    mutationFn: () => api.imports.triggerOcr(id!),
    onSuccess: res => {
      qc.invalidateQueries({ queryKey: ['imports', id, 'staged'] })
      setOcrMsg({ ok: true, text: `OCR abgeschlossen: ${res.fieldsFound} Felder erkannt auf ${res.pagesProcessed} Seiten.` })
    },
    onError: err => setOcrMsg({ ok: false, text: `OCR fehlgeschlagen: ${err.message}` }),
  })

  const analyzeMutation = useMutation({
    mutationFn: () => api.imports.analyze(id!),
    onSuccess: res => {
      qc.invalidateQueries({ queryKey: ['imports', id, 'staged'] })
      setAiMsg({ ok: true, text: `KI-Analyse abgeschlossen: ${res.fieldsFound} Felder erkannt.` })
    },
    onError: (err: Error) => setAiMsg({ ok: false, text: `KI-Analyse fehlgeschlagen: ${err.message}` }),
  })

  const visibleFields = fields.filter(f => !f.fieldKey.startsWith('_'))
  const foundKeys = new Set(visibleFields.map(f => f.fieldKey))
  const missingRequired = [...REQUIRED_FIELDS].filter(k => !foundKeys.has(k))

  // Required fields first, then optional — sorted alphabetically within each group
  const sortedFields = [
    ...visibleFields.filter(f => REQUIRED_FIELDS.has(f.fieldKey)).sort((a, b) => a.fieldKey.localeCompare(b.fieldKey)),
    ...visibleFields.filter(f => !REQUIRED_FIELDS.has(f.fieldKey)).sort((a, b) => a.fieldKey.localeCompare(b.fieldKey)),
  ]

  const errorIssues = issues.filter(i => i.severity === 'Error')
  const warnIssues  = issues.filter(i => i.severity !== 'Error')

  const canReview = run?.status === 'NeedsReview' || run?.status === 'NeedsRework'
  const docDownloadUrl = run ? `${BASE}/documents/${run.documentId}/download` : ''

  if (runLoading) return <div className={styles.loading}>Lade…</div>
  if (!run) return <div className={styles.loading}>Importlauf nicht gefunden.</div>

  return (
    <div className={styles.page}>
      {/* Header */}
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <button className={styles.backBtn} onClick={() => navigate('/dokumente')}>← Zurück</button>
          <div>
            <h1 className={styles.title}>{run.documentName}</h1>
            <span className={styles.statusBadge} data-status={run.status}>
              {STATUS_LABELS[run.status] ?? run.status}
            </span>
          </div>
        </div>
        <div className={styles.headerActions}>
          <button
            className={`${styles.btn} ${styles.btnAi}`}
            onClick={() => { setAiMsg(null); analyzeMutation.mutate() }}
            disabled={analyzeMutation.isPending || !ollamaStatus?.isAvailable}
            title={ollamaStatus?.isAvailable
              ? `KI-Analyse mit ${ollamaStatus.modelName}`
              : (ollamaStatus?.message ?? 'Ollama nicht verfügbar')}
          >
            {analyzeMutation.isPending ? 'KI analysiert…' : '✦ KI-Analyse'}
          </button>
          <button
            className={`${styles.btn} ${styles.btnOcr}`}
            onClick={() => { setOcrMsg(null); ocrMutation.mutate() }}
            disabled={ocrMutation.isPending || !ocrStatus?.isAvailable}
            title={ocrStatus?.isAvailable ? 'OCR starten' : (ocrStatus?.message ?? 'OCR nicht konfiguriert')}
          >
            {ocrMutation.isPending ? 'OCR läuft…' : '⊙ OCR'}
          </button>
          {canReview && (
            <>
              <button
                className={`${styles.btn} ${styles.btnApprove}`}
                onClick={() => approveMutation.mutate()}
                disabled={approveMutation.isPending}
                title={missingRequired.length > 0 ? `${missingRequired.length} Pflichtfeld(er) fehlen` : undefined}
              >
                ✓ Freigeben
              </button>
              <button
                className={`${styles.btn} ${styles.btnReject}`}
                onClick={() => rejectMutation.mutate()}
                disabled={rejectMutation.isPending}
              >
                ✕ Ablehnen
              </button>
            </>
          )}
        </div>
      </div>

      {aiMsg && (
        <div className={`${styles.alert} ${aiMsg.ok ? styles.alertAi : styles.alertError}`}>
          {aiMsg.text}
        </div>
      )}

      {ocrMsg && (
        <div className={`${styles.alert} ${ocrMsg.ok ? styles.alertSuccess : styles.alertError}`}>
          {ocrMsg.text}
          {!ocrMsg.ok && !ocrStatus?.isAvailable && (
            <span className={styles.alertNote}> — OCR-Provider in den Einstellungen konfigurieren.</span>
          )}
        </div>
      )}

      {run.errorMessage && (
        <div className={`${styles.alert} ${styles.alertError}`}>
          Fehler bei der Extraktion: {run.errorMessage}
        </div>
      )}

      {/* Two-column layout */}
      <div className={styles.columns}>
        {/* Left: Document preview */}
        <div className={styles.pdfPane}>
          <div className={styles.paneHeader}>
            Originaldokument
            <a className={styles.downloadLink} href={docDownloadUrl} target="_blank" rel="noreferrer">
              ↓ Herunterladen
            </a>
          </div>
          {run.documentName.toLowerCase().endsWith('.pdf') ? (
            <iframe
              className={styles.pdfFrame}
              src={docDownloadUrl}
              title="Dokument"
            />
          ) : (
            <div className={styles.noPreview}>
              <p>Vorschau nur für PDF-Dateien verfügbar.</p>
              <a className={styles.downloadLink} href={docDownloadUrl} target="_blank" rel="noreferrer">
                Datei herunterladen
              </a>
            </div>
          )}

          {images.length > 0 && (
            <div className={styles.imagesSection}>
              <div className={styles.imagesSectionTitle}>Extrahierte Bilder ({images.length})</div>
              <div className={styles.imageGrid}>
                {images.map(img => (
                  <div key={img.id} className={styles.imageCard}>
                    <img
                      src={api.imports.imageUrl(id!, img.id)}
                      alt={`Seite ${img.pageNumber} Bild ${img.imageIndex}`}
                      className={styles.imageThumb}
                      loading="lazy"
                    />
                    <span className={styles.imageLabel}>S.{img.pageNumber}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Right: Fields */}
        <div className={styles.fieldsPane}>
          <div className={styles.paneHeader}>
            Extrahierte Felder
            <div className={styles.paneHeaderRight}>
              {missingRequired.length > 0 && (
                <span className={styles.missingBadge}>
                  {missingRequired.length} Pflichtfeld{missingRequired.length > 1 ? 'er' : ''} fehlt{missingRequired.length === 1 ? '' : 'en'}
                </span>
              )}
              <span className={styles.fieldCount}>{visibleFields.length} Felder</span>
            </div>
          </div>

          {/* Error issues (missing required fields) */}
          {errorIssues.length > 0 && (
            <div className={styles.issuesListError}>
              {errorIssues.map(i => (
                <div key={i.id} className={styles.issueError}>
                  <span className={styles.issueSeverity}>Fehler</span>
                  {i.message}
                </div>
              ))}
            </div>
          )}

          {/* Warning issues */}
          {warnIssues.length > 0 && (
            <div className={styles.issuesList}>
              {warnIssues.map(i => (
                <div key={i.id} className={styles.issue}>
                  <span className={styles.issueSeverity}>{i.severity}</span>
                  {i.message}
                </div>
              ))}
            </div>
          )}

          {visibleFields.length === 0 && missingRequired.length === 0 ? (
            <p className={styles.noFields}>
              Keine Felder extrahiert.
              {!ocrStatus?.isAvailable && ' OCR ist nicht konfiguriert.'}
            </p>
          ) : (
            <div className={styles.fieldList}>
              {/* Missing required fields as placeholder rows */}
              {missingRequired.map(key => (
                <div key={`missing-${key}`} className={`${styles.fieldRow} ${styles.fieldRowMissing}`}>
                  <div className={styles.fieldMeta}>
                    <span className={styles.fieldKey}>{key}</span>
                    <span className={styles.requiredBadge}>PFLICHT</span>
                    <span className={styles.sourceRef} />
                  </div>
                  <div className={styles.fieldValueMissing}>Nicht erkannt — bitte manuell eintragen</div>
                </div>
              ))}

              {/* Extracted fields */}
              {sortedFields.map(field => (
                <div key={field.id} className={`${styles.fieldRow} ${field.isConfirmed ? styles.fieldRowConfirmed : ''}`}>
                  <div className={styles.fieldMeta}>
                    <span className={styles.fieldKey}>{field.fieldKey}</span>
                    {REQUIRED_FIELDS.has(field.fieldKey) && (
                      <span className={styles.requiredBadgeFound}>PFLICHT</span>
                    )}
                    <span className={`${styles.confidence} ${CONFIDENCE_CLASS(field.confidence)}`}>
                      {formatConfidence(field.confidence)}
                    </span>
                    <span className={styles.sourceRef}>{field.sourceRef ?? '—'}</span>
                  </div>
                  <EditableField
                    field={field}
                    runId={id!}
                    onSaved={() => {}}
                  />
                </div>
              ))}
            </div>
          )}

          <div className={styles.fieldFooter}>
            <span className={styles.footerHint}>Klick auf einen Wert zum Bearbeiten · Enter zum Speichern</span>
          </div>
        </div>
      </div>
    </div>
  )
}
