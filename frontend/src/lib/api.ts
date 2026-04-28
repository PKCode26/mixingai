import type { AuthUser, LoginRequest, LoginResponse } from '../types/auth'
import type { Document, DocumentDuplicateError } from '../types/documents'

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
}
