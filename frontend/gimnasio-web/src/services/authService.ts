import type { AuthUser, LoginRequest, RegisterRequest } from '../types/auth'

async function parseResponse(response: Response): Promise<AuthUser> {
  if (!response.ok) {
    const body = await response.json().catch(() => null) as { message?: string; errors?: Record<string, string[]> } | null
    const validationMessage = body?.errors ? Object.values(body.errors).flat()[0] : null
    throw new Error(body?.message ?? validationMessage ?? 'No se pudo completar la operación.')
  }

  return response.json() as Promise<AuthUser>
}

export async function login(request: LoginRequest): Promise<AuthUser> {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(request),
  })
  return parseResponse(response)
}

export async function register(request: RegisterRequest): Promise<AuthUser> {
  const response = await fetch('/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(request),
  })
  return parseResponse(response)
}

export async function getCurrentUser(): Promise<AuthUser | null> {
  const response = await fetch('/api/auth/me', { credentials: 'include' })
  if (response.status === 401) return null
  return parseResponse(response)
}

export async function logout(): Promise<void> {
  await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' })
}
