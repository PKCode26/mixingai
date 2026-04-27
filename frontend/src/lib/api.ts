import type { AuthUser, LoginRequest, LoginResponse } from '../types/auth'

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

export const api = {
  auth: {
    login: (data: LoginRequest) =>
      request<LoginResponse>('/auth/login', { method: 'POST', body: JSON.stringify(data) }),

    logout: () =>
      request<void>('/auth/logout', { method: 'POST' }),

    me: () =>
      request<AuthUser>('/auth/me'),
  },
}
