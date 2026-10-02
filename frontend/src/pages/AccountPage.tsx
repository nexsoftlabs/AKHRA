import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { BadgeCheck, LogOut, Mail, Phone, Shield } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { GlassCard } from '@/components/ui/glass-card'
import { useMutation } from '@tanstack/react-query'
import { GoogleSignInButton } from '@/components/auth/GoogleSignInButton'
import { getMe, logout, resendEmailConfirmation } from '@/lib/auth'
import { cn } from '@/lib/utils'

export function AccountPage() {
  const queryClient = useQueryClient()
  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })
  const resendMutation = useMutation({
    mutationFn: resendEmailConfirmation,
  })

  return (
    <div className="mx-auto max-w-2xl px-4 py-12 sm:px-6 sm:py-16">
      <div className="mb-8">
        <h1 className="text-3xl font-semibold tracking-tight">My account</h1>
        <p className="mt-2 text-muted-foreground">
          Profile and session. Purchased titles will appear in your library after checkout (Phase 5).
        </p>
        <Link to="/browse" className="mt-4 inline-block">
          <Button>Browse movies</Button>
        </Link>
      </div>

      {meQuery.isLoading && (
        <GlassCard className="animate-pulse space-y-4">
          <div className="h-6 w-40 rounded bg-white/10" />
          <div className="h-4 w-full rounded bg-white/10" />
          <div className="h-4 w-2/3 rounded bg-white/10" />
        </GlassCard>
      )}

      {meQuery.isError && (
        <GlassCard className="text-center">
          <p className="text-muted-foreground">You&apos;re not signed in.</p>
          <Link to="/login" className="mt-4 inline-block">
            <Button className="mt-4">Sign in</Button>
          </Link>
        </GlassCard>
      )}

      {meQuery.data && (
        <div className="space-y-6">
          {!meQuery.data.emailConfirmed && (
            <div className="rounded-2xl border border-amber-500/30 bg-amber-500/10 px-4 py-3 text-sm">
              <p className="font-medium text-amber-200">Confirm your email to unlock checkout and subscriptions.</p>
              <Button
                className="mt-3"
                size="sm"
                variant="outline"
                disabled={resendMutation.isPending}
                onClick={() => resendMutation.mutate()}
              >
                {resendMutation.isPending ? 'Sending…' : 'Resend confirmation email'}
              </Button>
              {resendMutation.isSuccess && (
                <p className="mt-2 text-xs text-muted-foreground">If an account exists, a new link was sent.</p>
              )}
            </div>
          )}
          <GlassCard>
            <div className="flex items-start gap-4">
              <div className="flex size-14 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-primary/30 to-violet-500/20 text-xl font-semibold ring-1 ring-border">
                {(meQuery.data.displayName ?? meQuery.data.email ?? '?').charAt(0).toUpperCase()}
              </div>
              <div className="min-w-0 flex-1">
                <h2 className="truncate text-xl font-semibold">
                  {meQuery.data.displayName ?? 'Viewer'}
                </h2>
                <p className="truncate text-sm text-muted-foreground">{meQuery.data.email ?? 'No email'}</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  {meQuery.data.emailConfirmed && (
                    <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/10 px-2.5 py-0.5 text-xs text-emerald-400 ring-1 ring-emerald-500/25">
                      <BadgeCheck className="size-3" />
                      Email verified
                    </span>
                  )}
                  {meQuery.data.isPhoneVerified && (
                    <span className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-0.5 text-xs text-primary ring-1 ring-primary/25">
                      <Phone className="size-3" />
                      Phone verified
                    </span>
                  )}
                </div>
              </div>
            </div>
          </GlassCard>

          <GlassCard className="space-y-4">
            <h3 className="text-sm font-medium uppercase tracking-wider text-muted-foreground">Details</h3>
            <dl className="space-y-3 text-sm">
              <div className="flex items-center gap-3">
                <Mail className="size-4 text-muted-foreground" />
                <dt className="text-muted-foreground">Email</dt>
                <dd className="ml-auto font-medium">{meQuery.data.email ?? '—'}</dd>
              </div>
              <div className="flex items-center gap-3">
                <Phone className="size-4 text-muted-foreground" />
                <dt className="text-muted-foreground">Phone</dt>
                <dd className="ml-auto font-medium">{meQuery.data.phoneNumber ?? '—'}</dd>
              </div>
              <div className="flex items-start gap-3">
                <Shield className="mt-0.5 size-4 text-muted-foreground" />
                <dt className="text-muted-foreground">Roles</dt>
                <dd className="ml-auto flex flex-wrap justify-end gap-1.5">
                  {meQuery.data.roles.map((role) => (
                    <span
                      key={role}
                      className={cn(
                        'rounded-md bg-muted px-2 py-0.5 text-xs font-medium ring-1 ring-border',
                      )}
                    >
                      {role}
                    </span>
                  ))}
                </dd>
              </div>
            </dl>
          </GlassCard>

          <GlassCard className="space-y-4">
            <h3 className="text-sm font-medium uppercase tracking-wider text-muted-foreground">
              Connected accounts
            </h3>
            <p className="text-sm text-muted-foreground">
              Link Google to sign in without a password. Use the same email as your AKHRA account.
            </p>
            <GoogleSignInButton intent="link" mode="signin" />
          </GlassCard>

          <Button
            variant="outline"
            className="w-full"
            onClick={async () => {
              await logout()
              queryClient.invalidateQueries({ queryKey: ['me'] })
              meQuery.refetch()
            }}
          >
            <LogOut className="size-4" />
            Sign out
          </Button>
        </div>
      )}
    </div>
  )
}
