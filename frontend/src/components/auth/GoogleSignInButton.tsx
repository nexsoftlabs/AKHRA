import { GoogleLogin } from '@react-oauth/google'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { resetCsrfToken } from '@/lib/api'
import { googleSignIn, linkGoogle } from '@/lib/auth'
import {
  isGoogleAuthEnabled,
  resolveGoogleClientId,
  useGoogleAuthConfig,
} from '@/lib/googleAuthConfig'
import { GoogleAuthProvider } from '@/providers/GoogleAuthProvider'

type GoogleSignInButtonProps = {
  mode?: 'signin' | 'signup'
  intent?: 'session' | 'link'
  redirectTo?: string
}

export function GoogleSignInButton({
  mode = 'signin',
  intent = 'session',
  redirectTo = '/browse',
}: GoogleSignInButtonProps) {
  const configQuery = useGoogleAuthConfig()
  const clientId = resolveGoogleClientId(configQuery.data)
  const enabled = isGoogleAuthEnabled(configQuery.data)

  if (configQuery.isLoading) {
    return (
      <p className="text-center text-xs text-muted-foreground">Loading Google sign-in…</p>
    )
  }

  if (!enabled || !clientId) {
    return (
      <p className="rounded-xl border border-border bg-muted/40 px-3 py-2 text-center text-xs text-muted-foreground">
        Google sign-in is not configured. Set{' '}
        <code className="text-foreground">Auth__GoogleClientId</code> on the API and optionally{' '}
        <code className="text-foreground">VITE_GOOGLE_CLIENT_ID</code> in{' '}
        <code className="text-foreground">frontend/.env</code> (same OAuth client ID).
      </p>
    )
  }

  return (
    <GoogleAuthProvider clientId={clientId}>
      <GoogleSignInButtonInner
        mode={mode}
        intent={intent}
        redirectTo={redirectTo}
      />
    </GoogleAuthProvider>
  )
}

function GoogleSignInButtonInner({
  mode,
  intent,
  redirectTo,
}: {
  mode: 'signin' | 'signup'
  intent: 'session' | 'link'
  redirectTo: string
}) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: intent === 'link' ? linkGoogle : googleSignIn,
    onSuccess: () => {
      resetCsrfToken()
      queryClient.invalidateQueries({ queryKey: ['me'] })
      queryClient.removeQueries({ queryKey: ['playback'] })
      queryClient.invalidateQueries({ queryKey: ['movie'] })
      if (intent === 'link') {
        return
      }
      navigate(redirectTo)
    },
  })

  const buttonText = mode === 'signup' ? 'signup_with' : 'continue_with'

  return (
    <div className="space-y-3">
      <div className="flex justify-center [&>div]:w-full">
        <GoogleLogin
          width="100%"
          theme="outline"
          size="large"
          text={buttonText}
          onSuccess={(response) => {
            if (response.credential) {
              mutation.mutate(response.credential)
            }
          }}
          onError={() => {
            mutation.reset()
          }}
        />
      </div>
      {mutation.isError && (
        <p className="rounded-xl border border-destructive/30 bg-destructive/10 px-3.5 py-2.5 text-sm text-destructive">
          {(mutation.error as Error).message}
        </p>
      )}
      {mutation.isPending && (
        <p className="text-center text-xs text-muted-foreground">Signing in with Google…</p>
      )}
      {intent === 'link' && mutation.isSuccess && (
        <p className="text-center text-xs text-emerald-500">Google account linked.</p>
      )}
    </div>
  )
}
