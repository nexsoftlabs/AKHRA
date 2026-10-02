import { GoogleOAuthProvider } from '@react-oauth/google'
import type { ReactNode } from 'react'

const envClientId = (import.meta.env.VITE_GOOGLE_CLIENT_ID as string | undefined)?.trim()

type GoogleAuthProviderProps = {
  children: ReactNode
  clientId?: string | null
}

export function GoogleAuthProvider({ children, clientId }: GoogleAuthProviderProps) {
  const resolved = (clientId ?? envClientId)?.trim()
  if (!resolved) {
    return children
  }

  return <GoogleOAuthProvider clientId={resolved}>{children}</GoogleOAuthProvider>
}
