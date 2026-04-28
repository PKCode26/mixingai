export type ImportRunStatus =
  | 'Queued'
  | 'Extracting'
  | 'NeedsReview'
  | 'Approved'
  | 'Published'
  | 'Archived'
  | 'Failed'
  | 'Rejected'
  | 'NeedsRework'

export interface ImportRun {
  id: string
  documentId: string
  documentName: string
  status: ImportRunStatus
  operatorNotes: string | null
  errorMessage: string | null
  extractedAtUtc: string | null
  createdAtUtc: string
  stagedFieldCount: number
  validationIssueCount: number
}

export interface StagedField {
  id: string
  fieldKey: string
  fieldValue: string | null
  confidence: number | null
  sourceRef: string | null
  isConfirmed: boolean
}

export interface ValidationIssue {
  id: string
  severity: string
  fieldKey: string | null
  message: string
}
