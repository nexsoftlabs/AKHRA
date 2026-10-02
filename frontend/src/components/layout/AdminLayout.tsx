import { NavLink, Outlet, Navigate, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  Film,
  LayoutDashboard,
  LogOut,
  Receipt,
  Store,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { getMe, logout } from '@/lib/auth'
import { cn } from '@/lib/utils'

const navClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
    isActive
      ? 'bg-primary/15 text-primary'
      : 'text-muted-foreground hover:bg-muted hover:text-foreground',
  )

export function AdminLayout() {
  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })

  if (meQuery.isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background text-muted-foreground">
        Loading admin…
      </div>
    )
  }

  if (meQuery.isError || !meQuery.data) {
    return <Navigate to="/admin/login" replace />
  }

  if (!meQuery.data.isAdmin) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-4 px-4">
        <p className="text-center text-muted-foreground">This account does not have staff access.</p>
        <Link to="/">
          <Button variant="outline">Back to site</Button>
        </Link>
      </div>
    )
  }

  const showFinance = meQuery.data.roles.some((r) =>
    ['FinanceAdmin', 'PlatformAdmin', 'SuperAdmin'].includes(r),
  )

  return (
    <div className="flex min-h-screen bg-muted/30">
      <aside className="hidden w-60 shrink-0 border-r border-border bg-background lg:flex lg:flex-col">
        <div className="flex h-16 items-center gap-2 border-b border-border px-5">
          <span className="flex size-9 items-center justify-center rounded-lg bg-primary/15 text-primary">
            <Film className="size-4" />
          </span>
          <div>
            <p className="text-sm font-semibold leading-none">AKHRA</p>
            <p className="text-xs text-muted-foreground">Admin console</p>
          </div>
        </div>
        <nav className="flex flex-1 flex-col gap-1 p-3" aria-label="Admin">
          <NavLink to="/admin" end className={navClass}>
            <LayoutDashboard className="size-4" />
            Dashboard
          </NavLink>
          <NavLink to="/admin/movies" className={navClass}>
            <Film className="size-4" />
            Catalog
          </NavLink>
          {showFinance && (
            <NavLink to="/admin/finance" className={navClass}>
              <Receipt className="size-4" />
              Finance
            </NavLink>
          )}
        </nav>
        <div className="space-y-1 border-t border-border p-3">
          <Link
            to="/"
            className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <Store className="size-4" />
            View storefront
          </Link>
          <button
            type="button"
            className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
            onClick={() => logout().then(() => window.location.assign('/admin/login'))}
          >
            <LogOut className="size-4" />
            Sign out
          </button>
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex h-14 items-center justify-between border-b border-border bg-background px-4 lg:px-8">
          <p className="truncate text-sm text-muted-foreground">
            Signed in as <span className="font-medium text-foreground">{meQuery.data.email}</span>
          </p>
          <div className="flex gap-2 lg:hidden">
            <NavLink to="/admin" className="text-sm text-primary">Dashboard</NavLink>
            <NavLink to="/admin/movies" className="text-sm text-primary">Catalog</NavLink>
            {showFinance && (
              <NavLink to="/admin/finance" className="text-sm text-primary">Finance</NavLink>
            )}
          </div>
        </header>
        <main className="flex-1 p-4 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
