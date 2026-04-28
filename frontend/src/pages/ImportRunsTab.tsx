import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api'
import type { ImportRun, StagedField } from '../types/imports'
import styles from './ImportRunsTab.module.css'

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('de-DE', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function formatConfidence(c: number | null) {
  if (c === null) return '—'
  return `${Math.round(c * 100)} %`
}

const STATUS_LABELS: Record<string, string> = {
  Queued: 'Warteschlange',
  Extracting: 'Extraktion…',
  NeedsReview: 'Review nötig',
  Approved: 'Freigegeben',
  Published: 'Publiziert',
  Archived: 'Archiviert',
  Failed: 'Fehler',
  Rejected: 'Abgelehnt',
  NeedsRework: 'Nacharbeit',
}

const STATUS_CLASS: Record<string, string> = {
  Queued: styles.statusQueued,
  Extracting: styles.statusExtracting,
  NeedsReview: styles.statusReview,
  Approved: styles.statusApproved,
  Published: styles.statusApproved,
  Failed: styles.statusFailed,
  Rejected: styles.statusFailed,
  NeedsRework: styles.statusReview,
  Archived: styles.statusArchived,
}

function StatusBadge({ status }: { status: string }) {
  return (
    <span className={`${styles.statusBadge} ${STATUS_CLASS[status] ?? ''}`}>
      {STATUS_LABELS[status] ?? status}
    </span>
  )
}

function StagedFieldsPanel({ runId }: { runId: string }) {
  const { data: fields = [], isLoading } = useQuery({
    queryKey: ['imports', runId, 'staged'],
    queryFn: () => api.imports.stagedFields(runId),
  })

  const { data: issues = [] } = useQuery({
    queryKey: ['imports', runId, 'issues'],
    queryFn: () => api.imports.issues(runId),
  })

  if (isLoading) return <p className={styles.panelEmpty}>Lade Felder…</p>

  const rawText = fields.find(f => f.fieldKey === '_RawText')
  const visibleFields = fields.filter(f => f.fieldKey !== '_RawText')

  return (
    <div className={styles.panel}>
      {issues.length > 0 && (
        <div className={styles.issuesList}>
          {issues.map(i => (
            <div key={i.id} className={styles.issue}>
              <span className={styles.issueSeverity}>{i.severity}</span>
              {i.fieldKey && <span className={styles.issueKey}>{i.fieldKey}:</span>}
              {i.message}
            </div>
          ))}
        </div>
      )}

      {visibleFields.length === 0 ? (
        <p className={styles.panelEmpty}>Keine Felder extrahiert.</p>
      ) : (
        <table className={styles.fieldTable}>
          <thead>
            <tr>
              <th>Feldname</th>
              <th>Wert</th>
              <th>Konfidenz</th>
              <th>Quelle</th>
            </tr>
          </thead>
          <tbody>
            {visibleFields.map(f => (
              <tr key={f.id} className={f.isConfirmed ? styles.confirmed : ''}>
                <td className={styles.fieldKey}>{f.fieldKey}</td>
                <td>{f.fieldValue ?? <span className={styles.empty}>—</span>}</td>
                <td>{formatConfidence(f.confidence)}</td>
                <td className={styles.sourceRef}>{f.sourceRef ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {rawText && (
        <details className={styles.rawText}>
          <summary>Volltext anzeigen</summary>
          <pre>{rawText.fieldValue}</pre>
        </details>
      )}
    </div>
  )
}

function RunRow({ run }: { run: ImportRun }) {
  const [expanded, setExpanded] = useState(false)
  const qc = useQueryClient()
  const navigate = useNavigate()

  const approveMutation = useMutation({
    mutationFn: () => api.imports.approve(run.id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['imports'] }),
  })

  const rejectMutation = useMutation({
    mutationFn: () => api.imports.reject(run.id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['imports'] }),
  })

  const canReview = run.status === 'NeedsReview' || run.status === 'NeedsRework'

  return (
    <>
      <tr
        className={`${styles.runRow} ${expanded ? styles.runRowExpanded : ''}`}
        onClick={() => setExpanded(e => !e)}
      >
        <td className={styles.docName}>{run.documentName}</td>
        <td><StatusBadge status={run.status} /></td>
        <td>{run.stagedFieldCount}</td>
        <td>{run.validationIssueCount > 0
          ? <span className={styles.issueCount}>{run.validationIssueCount}</span>
          : '—'}
        </td>
        <td>{formatDate(run.createdAtUtc)}</td>
        <td onClick={e => e.stopPropagation()}>
          <div className={styles.actions}>
            <button
              className={`${styles.btn} ${styles.btnReview}`}
              onClick={() => navigate(`/imports/${run.id}/review`)}
            >
              Review
            </button>
            {canReview && (
              <>
                <button
                  className={`${styles.btn} ${styles.btnSuccess}`}
                  onClick={() => approveMutation.mutate()}
                  disabled={approveMutation.isPending}
                >
                  Freigeben
                </button>
                <button
                  className={`${styles.btn} ${styles.btnDanger}`}
                  onClick={() => rejectMutation.mutate()}
                  disabled={rejectMutation.isPending}
                >
                  Ablehnen
                </button>
              </>
            )}
            {run.status === 'Failed' && run.errorMessage && (
              <span className={styles.errorMsg} title={run.errorMessage}>Fehler</span>
            )}
          </div>
        </td>
      </tr>
      {expanded && (
        <tr className={styles.detailRow}>
          <td colSpan={6}>
            <StagedFieldsPanel runId={run.id} />
          </td>
        </tr>
      )}
    </>
  )
}

export default function ImportRunsTab() {
  const { data: runs = [], isLoading } = useQuery({
    queryKey: ['imports'],
    queryFn: () => api.imports.list(),
    refetchInterval: 5000,
  })

  return (
    <div>
      {isLoading ? (
        <p className={styles.empty}>Lade Importläufe…</p>
      ) : runs.length === 0 ? (
        <p className={styles.empty}>
          Noch keine Importläufe. Starte einen Import über den Tab „Dateien".
        </p>
      ) : (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Dokument</th>
              <th>Status</th>
              <th>Felder</th>
              <th>Probleme</th>
              <th>Erstellt</th>
              <th>Aktionen</th>
            </tr>
          </thead>
          <tbody>
            {runs.map(run => <RunRow key={run.id} run={run} />)}
          </tbody>
        </table>
      )}
    </div>
  )
}
