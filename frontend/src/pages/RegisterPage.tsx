import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { Lock, Mail, UserRound } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Field } from '@/components/ui/field'
import { GlassCard } from '@/components/ui/glass-card'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/ui/password-input'
import { GoogleSignInButton } from '@/components/auth/GoogleSignInButton'
import { resetCsrfToken } from '@/lib/api'
import { register } from '@/lib/auth'

export function RegisterPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: register,
    onSuccess: () => {
      resetCsrfToken()
      queryClient.invalidateQueries({ queryKey: ['me'] })
      queryClient.removeQueries({ queryKey: ['playback'] })
      queryClient.invalidateQueries({ queryKey: ['movie'] })
      navigate('/browse')
    },
  })

  return (
    <GlassCard>
      <div className="mb-8 space-y-2">
        <h1 className="text-2xl font-semibold tracking-tight">Create your account</h1>
        <p className="text-sm text-muted-foreground">
          Use a strong password — at least 10 characters with mixed case, numbers, and symbols.
        </p>
      </div>

      <form
        className="space-y-6"
        onSubmit={(e) => {
          e.preventDefault()
          const form = new FormData(e.currentTarget)
          mutation.mutate({
            email: String(form.get('email')),
            password: String(form.get('password')),
            displayName: String(form.get('displayName') || '') || undefined,
          })
        }}
      >
        <Field id="displayName" label="Display name" hint="Optional — shown on your profile">
          <Input
            id="displayName"
            name="displayName"
            autoComplete="name"
            placeholder="How friends see you"
            leadingIcon={<UserRound />}
          />
        </Field>
        <Field id="email" label="Email address">
          <Input
            id="email"
            name="email"
            type="email"
            autoComplete="email"
            required
            placeholder="name@studio.in"
            leadingIcon={<Mail />}
          />
        </Field>
        <Field id="password" label="Password">
          <PasswordInput
            id="password"
            name="password"
            autoComplete="new-password"
            required
            minLength={10}
            placeholder="Min. 10 characters"
            leadingIcon={<Lock />}
          />
        </Field>
        {mutation.isError && (
          <p className="rounded-xl border border-destructive/30 bg-destructive/10 px-3.5 py-2.5 text-sm text-destructive">
            {(mutation.error as Error).message}
          </p>
        )}
        <p className="text-xs leading-relaxed text-muted-foreground">
          By creating an account, you agree to our{' '}
          <Link to="/terms" className="font-medium text-primary hover:underline">Terms of Service</Link>
          {' '}and{' '}
          <Link to="/privacy" className="font-medium text-primary hover:underline">Privacy Policy</Link>.
        </p>
        <Button type="submit" className="h-12 w-full text-base shadow-lg shadow-primary/20" disabled={mutation.isPending}>
          {mutation.isPending ? 'Creating account…' : 'Get started'}
        </Button>
      </form>

      <div className="relative my-6">
        <div className="absolute inset-0 flex items-center">
          <span className="w-full border-t border-border" />
        </div>
        <p className="relative mx-auto w-fit bg-card px-2 text-xs text-muted-foreground">or</p>
      </div>
      <GoogleSignInButton mode="signup" />

      <p className="mt-6 text-center text-sm text-muted-foreground">
        Already have an account?{' '}
        <Link to="/login" className="font-medium text-primary hover:underline">
          Sign in
        </Link>
      </p>
    </GlassCard>
  )
}
