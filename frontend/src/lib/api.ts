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

async function apiFetchOnce<T>(
  path: string,
  init?: RequestInit,
  csrfRetry = false,
): Promise<Response> {
  const method = (init?.method ?? 'GET').toUpperCase()
  const headers = new Headers(init?.headers)

  if (!headers.has('Content-Type') && init?.body && typeof init.body === 'string') {
    headers.set('Content-Type', 'application/json')
  }

  if (unsafeMethods.has(method)) {
    if (csrfRetry) {
      resetCsrfToken()
    }
    const token = await ensureCsrfToken()
    headers.set('X-XSRF-TOKEN', token)
  }

  return fetch(`${apiBase}${path}`, {
    ...init,
    credentials: 'include',
    headers,
  })
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  let response = await apiFetchOnce(path, init)

  if (!response.ok && response.status === 400) {
    const body = (await response.clone().json().catch(() => ({}))) as { error?: string }
    if (body.error === 'invalid_csrf' && unsafeMethods.has((init?.method ?? 'GET').toUpperCase())) {
      response = await apiFetchOnce(path, init, true)
    }
  }

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
