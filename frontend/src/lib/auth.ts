import type { QueryClient } from '@tanstack/react-query'
import { z } from 'zod'
import { apiFetch, resetCsrfToken } from '@/lib/api'

export const registerSchema = z.object({
  email: z.string().email(),
  password: z.string().min(10),
  displayName: z.string().max(100).optional(),
})

export const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1),
})

export const userProfileSchema = z.object({
  id: z.string().uuid(),
  email: z.string().email().nullable().optional(),
  displayName: z.string().nullable().optional(),
  phoneNumber: z.string().nullable().optional(),
  emailConfirmed: z.boolean(),
  isPhoneVerified: z.boolean(),
  roles: z.array(z.string()),
  twoFactorEnabled: z.boolean(),
  isAdmin: z.boolean(),
})

export type UserProfile = z.infer<typeof userProfileSchema>

function normalizeEmail(email: string) {
  return email.trim().toLowerCase()
}

async function authFetch<T>(path: string, init?: RequestInit): Promise<T> {
  try {
    return await apiFetch<T>(path, init)
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Request failed'
    if (message.includes('account_locked') || message.includes('locked')) {
      throw new Error('Account is temporarily locked after too many failed attempts.')
    }
    throw err
  }
}

export function register(input: z.infer<typeof registerSchema>) {
  const data = registerSchema.parse(input)
  return authFetch<UserProfile>('/api/v1/auth/register', {
    method: 'POST',
    body: JSON.stringify({ ...data, email: normalizeEmail(data.email) }),
  })
}

export function login(input: z.infer<typeof loginSchema>) {
  const data = loginSchema.parse(input)
  return authFetch<UserProfile>('/api/v1/auth/login', {
    method: 'POST',
    body: JSON.stringify({ ...data, email: normalizeEmail(data.email) }),
  })
}

export function getMe() {
  return authFetch<UserProfile>('/api/v1/users/me')
}

export function logout() {
  return authFetch<{ status: string }>('/api/v1/auth/logout', { method: 'POST' })
}

/** Clears cached session state so UI reflects signed-out immediately (preview caps, hasAccess, etc.). */
export function clearClientAuthState(queryClient: QueryClient) {
  resetCsrfToken()
  queryClient.removeQueries({ queryKey: ['me'] })
  queryClient.removeQueries({ queryKey: ['playback'] })
  void queryClient.invalidateQueries({ queryKey: ['movie'] })
  void queryClient.invalidateQueries({ queryKey: ['movies'] })
}

export function confirmEmail(email: string, token: string) {
  return authFetch<UserProfile>('/api/v1/auth/email/confirm', {
    method: 'POST',
    body: JSON.stringify({ email: normalizeEmail(email), token }),
  })
}

export function googleSignIn(idToken: string) {
  return authFetch<UserProfile>('/api/v1/auth/google', {
    method: 'POST',
    body: JSON.stringify({ idToken }),
  })
}

export function linkGoogle(idToken: string) {
  return authFetch<UserProfile>('/api/v1/auth/google/link', {
    method: 'POST',
    body: JSON.stringify({ idToken }),
  })
}

export function resendEmailConfirmation() {
  return authFetch<{ status: string }>('/api/v1/auth/email/resend', { method: 'POST' })
}
