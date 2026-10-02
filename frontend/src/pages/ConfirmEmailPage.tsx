import { useMutation } from '@tanstack/react-query'
import { Link, useSearchParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { GlassCard } from '@/components/ui/glass-card'
import { confirmEmail } from '@/lib/auth'

export function ConfirmEmailPage() {
  const [params] = useSearchParams()
  const email = params.get('email') ?? ''
  const token = params.get('token') ?? ''

  const mutation = useMutation({
    mutationFn: () => confirmEmail(email, token),
  })

  return (
    <div className="mx-auto max-w-md px-4 py-16">
      <GlassCard>
        <h1 className="text-xl font-semibold">Confirm email</h1>
        {!email || !token ? (
          <p className="mt-4 text-sm text-muted-foreground">Invalid confirmation link.</p>
        ) : mutation.isSuccess ? (
          <p className="mt-4 text-sm text-emerald-600 dark:text-emerald-400">Email confirmed. You can purchase and stream.</p>
        ) : (
          <Button className="mt-6 w-full" disabled={mutation.isPending} onClick={() => mutation.mutate()}>
            {mutation.isPending ? 'Confirming…' : 'Confirm my email'}
          </Button>
        )}
        {mutation.isError && (
          <p className="mt-4 text-sm text-destructive">{(mutation.error as Error).message}</p>
        )}
        <Link to="/account" className="mt-6 block text-center text-sm text-primary hover:underline">Account</Link>
      </GlassCard>
    </div>
  )
}
