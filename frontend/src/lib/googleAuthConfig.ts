import { useQuery } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api'

export type GoogleAuthConfig = {
  enabled: boolean
  clientId: string | null
}

const envClientId = (import.meta.env.VITE_GOOGLE_CLIENT_ID as string | undefined)?.trim()

export function getGoogleClientIdFromEnv() {
  return envClientId || null
}

export async function fetchGoogleAuthConfig(): Promise<GoogleAuthConfig> {
  return apiFetch<GoogleAuthConfig>('/api/v1/auth/google/config')
}

export function useGoogleAuthConfig() {
  return useQuery({
    queryKey: ['auth', 'google-config'],
    queryFn: fetchGoogleAuthConfig,
    staleTime: 5 * 60 * 1000,
    retry: 1,
  })
}

export function resolveGoogleClientId(config: GoogleAuthConfig | undefined) {
  return getGoogleClientIdFromEnv() ?? config?.clientId ?? null
}

export function isGoogleAuthEnabled(config: GoogleAuthConfig | undefined) {
  return Boolean(resolveGoogleClientId(config))
}
