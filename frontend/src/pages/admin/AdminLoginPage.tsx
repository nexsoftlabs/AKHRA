import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { Lock, Mail, Shield } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Field } from '@/components/ui/field'
import { GlassCard } from '@/components/ui/glass-card'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/ui/password-input'
import { getMe, login } from '@/lib/auth'

export function AdminLoginPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: async (input: { email: string; password: string }) => {
      await login(input)
      const profile = await getMe()
      if (!profile.isAdmin) {
        throw new Error('This account is not authorized for the admin console.')
      }
      return profile
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
      navigate('/admin', { replace: true })
    },
  })

  return (
    <div className="relative flex min-h-screen items-center justify-center bg-muted/40 px-4">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_top,var(--primary)_0%,transparent_50%)] opacity-20" />
      <GlassCard className="relative w-full max-w-md">
        <div className="mb-8 flex flex-col items-center text-center">
          <span className="flex size-14 items-center justify-center rounded-2xl bg-primary/15 text-primary ring-1 ring-primary/25">
            <Shield className="size-7" />
          </span>
          <h1 className="mt-4 text-2xl font-semibold tracking-tight">Staff sign in</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Admin console for catalog, finance, and business metrics.
          </p>
        </div>

        <form
          className="space-y-5"
          onSubmit={(e) => {
            e.preventDefault()
            const form = new FormData(e.currentTarget)
            mutation.mutate({
              email: String(form.get('email')),
              password: String(form.get('password')),
            })
          }}
        >
          <Field id="email" label="Work email">
            <Input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              required
              placeholder="admin@akhra.app"
              leadingIcon={<Mail />}
            />
          </Field>
          <Field id="password" label="Password">
            <PasswordInput
              id="password"
              name="password"
              autoComplete="current-password"
              required
              placeholder="••••••••"
              leadingIcon={<Lock />}
            />
          </Field>
          {mutation.isError && (
            <p className="rounded-xl border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {(mutation.error as Error).message}
            </p>
          )}
          <Button type="submit" className="h-11 w-full" disabled={mutation.isPending}>
            {mutation.isPending ? 'Signing in…' : 'Enter admin console'}
          </Button>
        </form>

        <p className="mt-6 text-center text-xs text-muted-foreground">
          Dev: <span className="font-medium text-foreground">admin@akhra.app</span> /{' '}
          <span className="font-medium text-foreground">AdminPass123!</span>
        </p>
        <p className="mt-4 text-center text-sm">
          <Link to="/" className="text-muted-foreground hover:text-primary">← Back to streaming site</Link>
        </p>
      </GlassCard>
    </div>
  )
}
