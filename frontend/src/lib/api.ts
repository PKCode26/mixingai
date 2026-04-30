import type { AuthUser, LoginRequest, LoginResponse } from '../types/auth'
import type { Document, DocumentDuplicateError } from '../types/documents'
import type { ImportRun, StagedField, ValidationIssue, ExtractedImage, OcrStatus, OllamaStatus, OllamaAnalysisResult } from '../types/imports'

const BASE = '/api'

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })
  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new Error(`${res.status} ${res.statusText}${text ? `: ${text}` : ''}`)
  }
  if (res.status === 204) return undefined as T
  return res.json()
}

export class DuplicateError extends Error {
  constructor(public readonly duplicate: DocumentDuplicateError) {
    super('Datei bereits vorhanden')
  }
}

export const api = {
  auth: {
    login: (data: LoginRequest) =>
      request<LoginResponse>('/auth/login', { method: 'POST', body: JSON.stringify(data) }),

    logout: () =>
      request<void>('/auth/logout', { method: 'POST' }),

    me: () =>
      request<AuthUser>('/auth/me'),
  },

  documents: {
    list: (search?: string, includeArchived = false) => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (includeArchived) params.set('includeArchived', 'true')
      return request<Document[]>(`/documents?${params}`)
    },

    get: (id: string) => request<Document>(`/documents/${id}`),

    upload: async (file: File): Promise<Document> => {
      const form = new FormData()
      form.append('file', file)
      const res = await fetch(`${BASE}/documents`, {
        method: 'POST',
        credentials: 'include',
        body: form,
      })
      if (res.status === 409) {
        const data: DocumentDuplicateError = await res.json()
        throw new DuplicateError(data)
      }
      if (!res.ok) {
        const text = await res.text().catch(() => '')
        throw new Error(`${res.status} ${res.statusText}${text ? `: ${text}` : ''}`)
      }
      return res.json()
    },

    download: (id: string) => {
      window.open(`${BASE}/documents/${id}/download`, '_blank')
    },

    archive: (id: string) =>
      request<Document>(`/documents/${id}/archive`, { method: 'POST' }),

    unarchive: (id: string) =>
      request<Document>(`/documents/${id}/unarchive`, { method: 'POST' }),
  },

  imports: {
    list: (status?: string, documentId?: string) => {
      const params = new URLSearchParams()
      if (status) params.set('status', status)
      if (documentId) params.set('documentId', documentId)
      const qs = params.toString()
      return request<ImportRun[]>(`/imports${qs ? `?${qs}` : ''}`)
    },

    get: (id: string) => request<ImportRun>(`/imports/${id}`),

    create: (documentId: string) =>
      request<ImportRun>('/imports', { method: 'POST', body: JSON.stringify({ documentId }) }),

    stagedFields: (id: string) => request<StagedField[]>(`/imports/${id}/staged`),

    issues: (id: string) => request<ValidationIssue[]>(`/imports/${id}/issues`),

    approve: (id: string) =>
      request<ImportRun>(`/imports/${id}/approve`, { method: 'POST' }),

    reject: (id: string, notes?: string) =>
      request<ImportRun>(`/imports/${id}/reject`, { method: 'POST', body: JSON.stringify({ notes: notes ?? null }) }),

    confirmField: (runId: string, fieldId: string, isConfirmed: boolean, fieldValue?: string) =>
      request<StagedField>(`/imports/${runId}/staged/${fieldId}`, {
        method: 'PATCH',
        body: JSON.stringify({ isConfirmed, fieldValue: fieldValue ?? null }),
      }),

    images: (runId: string) => request<ExtractedImage[]>(`/imports/${runId}/images`),

    imageUrl: (runId: string, imageId: string) => `/api/imports/${runId}/images/${imageId}`,

    triggerOcr: (runId: string) =>
      request<{ fieldsFound: number; pagesProcessed: number }>(`/imports/${runId}/ocr`, { method: 'POST' }),

    ocrStatus: () => request<OcrStatus>('/imports/ocr/status'),

    analyze: (runId: string) =>
      request<OllamaAnalysisResult>(`/imports/${runId}/analyze`, { method: 'POST' }),

    ollamaStatus: () => request<OllamaStatus>('/imports/ollama/status'),
  },
}
