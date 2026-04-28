export type DocumentType = 'Unknown' | 'Pdf' | 'Excel' | 'Other'

export interface Document {
  id: string
  originalFileName: string
  displayName: string
  mimeContentType: string
  fileSizeBytes: number
  contentHash: string
  documentType: DocumentType
  isArchived: boolean
  archivedAtUtc: string | null
  createdAtUtc: string
  createdByUserId: string | null
}

export interface DocumentDuplicateError {
  existingDocumentId: string
  originalFileName: string
}
