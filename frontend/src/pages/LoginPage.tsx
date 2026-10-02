import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { Lock, Mail } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Field } from '@/components/ui/field'
import { GlassCard } from '@/components/ui/glass-card'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/ui/password-input'
import { GoogleSignInButton } from '@/components/auth/GoogleSignInButton'
import { login } from '@/lib/auth'

export function LoginPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
      navigate('/browse')
    },
  })

  return (
    <GlassCard>
      <div className="mb-8 space-y-2">
        <h1 className="text-2xl font-semibold tracking-tight">Welcome back</h1>
        <p className="text-sm text-muted-foreground">
          Sign in with your email. Dev demo:{' '}
          <span className="font-medium text-foreground/80">demo@akhra.app</span> /{' '}
          <span className="font-medium text-foreground/80">DemoPass123!</span>
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
          })
        }}
      >
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
            autoComplete="current-password"
            required
            placeholder="Enter your password"
            leadingIcon={<Lock />}
          />
        </Field>
        {mutation.isError && (
          <p className="rounded-xl border border-destructive/30 bg-destructive/10 px-3.5 py-2.5 text-sm text-destructive">
            {(mutation.error as Error).message}
          </p>
        )}
        <Button type="submit" className="h-12 w-full text-base shadow-lg shadow-primary/20" disabled={mutation.isPending}>
          {mutation.isPending ? 'Signing in…' : 'Sign in'}
        </Button>
      </form>

      <div className="relative my-6">
        <div className="absolute inset-0 flex items-center">
          <span className="w-full border-t border-border" />
        </div>
        <p className="relative mx-auto w-fit bg-card px-2 text-xs text-muted-foreground">or</p>
      </div>
      <GoogleSignInButton mode="signin" />

      <p className="mt-6 text-center text-sm text-muted-foreground">
        New here?{' '}
        <Link to="/register" className="font-medium text-primary hover:underline">
          Create an account
        </Link>
      </p>
    </GlassCard>
  )
}
