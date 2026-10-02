import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { IndianRupee, RefreshCw, Users, Video, Wallet } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { fetchAdminDashboardStats } from '@/lib/admin-dashboard'

function formatInr(minor: number) {
  return `₹${(minor / 100).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`
}

function StatCard({
  label,
  value,
  hint,
  icon: Icon,
}: {
  label: string
  value: string
  hint?: string
  icon: typeof Users
}) {
  return (
    <div className="rounded-2xl border border-border bg-card p-5 shadow-sm">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium text-muted-foreground">{label}</p>
        <span className="flex size-9 items-center justify-center rounded-lg bg-primary/10 text-primary">
          <Icon className="size-4" />
        </span>
      </div>
      <p className="mt-3 text-3xl font-semibold tracking-tight">{value}</p>
      {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
    </div>
  )
}

export function AdminDashboardPage() {
  const statsQuery = useQuery({
    queryKey: ['admin-dashboard'],
    queryFn: fetchAdminDashboardStats,
  })

  const stats = statsQuery.data

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">Business overview</h1>
          <p className="mt-1 text-sm text-muted-foreground">Last 30 days unless noted otherwise.</p>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => statsQuery.refetch()}
          disabled={statsQuery.isFetching}
        >
          <RefreshCw className={statsQuery.isFetching ? 'size-4 animate-spin' : 'size-4'} />
          Refresh
        </Button>
      </div>

      {statsQuery.isError && (
        <p className="mt-6 text-sm text-destructive">Could not load dashboard stats.</p>
      )}

      {stats && (
        <>
          <div className="mt-8 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <StatCard
              label="Revenue (30d)"
              value={formatInr(stats.revenueCapturedMinorUnits30d)}
              hint={`${stats.paymentsCaptured30d} captured payments`}
              icon={IndianRupee}
            />
            <StatCard
              label="Active subscriptions"
              value={String(stats.activeSubscriptions)}
              icon={Wallet}
            />
            <StatCard
              label="Registered users"
              value={String(stats.totalUsers)}
              icon={Users}
            />
            <StatCard
              label="Catalog"
              value={`${stats.publishedMovies} live`}
              hint={`${stats.draftMovies} drafts`}
              icon={Video}
            />
          </div>

          <div className="mt-6 grid gap-4 lg:grid-cols-3">
            <div className="rounded-2xl border border-border bg-card p-5 lg:col-span-1">
              <h2 className="font-semibold">Quick actions</h2>
              <ul className="mt-4 space-y-2 text-sm">
                <li>
                  <Link className="text-primary hover:underline" to="/admin/movies/new">Add a movie</Link>
                </li>
                <li>
                  <Link className="text-primary hover:underline" to="/admin/movies">Manage catalog</Link>
                </li>
                <li>
                  <Link className="text-primary hover:underline" to="/admin/finance">Refunds & payments</Link>
                </li>
              </ul>
              <p className="mt-4 text-xs text-muted-foreground">
                Refunds (30d): <span className="font-medium text-foreground">{stats.refunds30d}</span>
              </p>
            </div>

            <div className="overflow-hidden rounded-2xl border border-border bg-card lg:col-span-2">
              <div className="border-b border-border px-5 py-4">
                <h2 className="font-semibold">Recent payments</h2>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead className="bg-muted/40 text-muted-foreground">
                    <tr>
                      <th className="px-5 py-2 font-medium">Customer</th>
                      <th className="px-5 py-2 font-medium">Amount</th>
                      <th className="px-5 py-2 font-medium">Status</th>
                      <th className="px-5 py-2 font-medium">When</th>
                    </tr>
                  </thead>
                  <tbody>
                    {stats.recentPayments.length === 0 && (
                      <tr>
                        <td colSpan={4} className="px-5 py-8 text-muted-foreground">No payments yet.</td>
                      </tr>
                    )}
                    {stats.recentPayments.map((p) => (
                      <tr key={p.paymentId} className="border-t border-border/60">
                        <td className="px-5 py-3">{p.customerEmail ?? '—'}</td>
                        <td className="px-5 py-3">{formatInr(p.amountMinorUnits)}</td>
                        <td className="px-5 py-3">{p.status}</td>
                        <td className="px-5 py-3 text-muted-foreground">
                          {new Date(p.updatedAt).toLocaleString()}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
