import { Link, NavLink, useLocation } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Film, UserRound } from 'lucide-react'
import { ThemeToggle } from '@/components/theme/ThemeToggle'
import { Button } from '@/components/ui/button'
import { getMe } from '@/lib/auth'
import { cn } from '@/lib/utils'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'rounded-full px-3.5 py-1.5 text-sm font-medium transition-colors',
    isActive ? 'bg-muted text-foreground' : 'text-muted-foreground hover:text-foreground',
  )

export function SiteHeader() {
  const location = useLocation()
  const isHome = location.pathname === '/'
  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })

  return (
    <header
      className={cn(
        'sticky top-0 z-50 transition-colors',
        isHome
          ? 'border-b border-transparent bg-gradient-to-b from-background/90 via-background/50 to-transparent backdrop-blur-sm'
          : 'border-b border-border bg-background/80 backdrop-blur-xl',
      )}
    >
      <div
        className={cn(
          'mx-auto flex h-16 items-center justify-between gap-4 px-4 sm:px-6',
          isHome ? 'max-w-[1920px] lg:px-12' : 'max-w-6xl',
        )}
      >
        <Link to="/" className="group flex items-center gap-2.5">
          <span className="flex size-9 items-center justify-center rounded-xl bg-primary/15 text-primary ring-1 ring-primary/30 transition group-hover:bg-primary/25">
            <Film className="size-4" strokeWidth={2.25} />
          </span>
          <span className="font-heading text-lg font-semibold tracking-tight">
            AKHRA
          </span>
        </Link>

        <nav className="hidden items-center gap-1 sm:flex" aria-label="Main">
          <NavLink to="/" className={navLinkClass} end>
            Home
          </NavLink>
          <NavLink to="/browse" className={navLinkClass}>
            Browse
          </NavLink>
          <NavLink to="/plans" className={navLinkClass}>
            Plans
          </NavLink>
          {meQuery.data && (
            <NavLink to="/library" className={navLinkClass}>
              Library
            </NavLink>
          )}
          {meQuery.data?.isAdmin && (
            <NavLink to="/admin" className={navLinkClass}>
              Admin
            </NavLink>
          )}
        </nav>

        <div className="flex items-center gap-2">
          <ThemeToggle className="hidden sm:flex" />
          {meQuery.data ? (
            <Link to="/account">
              <Button variant="secondary" size="sm">
                <UserRound className="size-3.5" />
                Account
              </Button>
            </Link>
          ) : (
            <>
              <Link to="/login">
                <Button variant="ghost" size="sm">Sign in</Button>
              </Link>
              <Link to="/register">
                <Button size="sm" className="shadow-lg shadow-primary/20">Get started</Button>
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}
