const apiBase = import.meta.env.VITE_API_BASE_URL ?? ''

let csrfToken: string | null = null

export function resetCsrfToken() {
  csrfToken = null
}

export async function ensureCsrfToken() {
  if (csrfToken) return csrfToken
  const response = await fetch(`${apiBase}/api/v1/antiforgery/token`, { credentials: 'include' })
  if (!response.ok) throw new Error('Could not obtain CSRF token.')
  const data = (await response.json()) as { token: string }
  csrfToken = data.token
  return csrfToken
}

const unsafeMethods = new Set(['POST', 'PUT', 'PATCH', 'DELETE'])

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const method = (init?.method ?? 'GET').toUpperCase()
  const headers = new Headers(init?.headers)

  if (!headers.has('Content-Type') && init?.body && typeof init.body === 'string') {
    headers.set('Content-Type', 'application/json')
  }

  if (unsafeMethods.has(method)) {
    const token = await ensureCsrfToken()
    headers.set('X-XSRF-TOKEN', token)
  }

  const response = await fetch(`${apiBase}${path}`, {
    ...init,
    credentials: 'include',
    headers,
  })

  if (!response.ok) {
    const body = (await response.json().catch(() => ({}))) as {
      message?: string
      error?: string
    }
    const detail = body.message ?? body.error
    throw new Error(detail ?? `Request failed (${response.status})`)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}

export async function fetchApiVersion() {
  return apiFetch<{ name: string; version: string }>('/api/v1/version')
}
