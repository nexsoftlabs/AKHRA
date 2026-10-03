import { NavLink } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Compass, Film, Home, UserRound } from 'lucide-react'
import { getMe } from '@/lib/auth'
import { cn } from '@/lib/utils'

const tabClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'flex min-h-11 min-w-0 flex-1 flex-col items-center justify-center gap-0.5 px-1 py-2 text-[0.65rem] font-medium sm:text-xs',
    isActive ? 'text-primary' : 'text-muted-foreground',
  )

export function MobileBottomNav() {
  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })

  return (
    <nav
      className="fixed inset-x-0 bottom-0 z-50 border-t border-border bg-background/95 backdrop-blur-lg pb-[env(safe-area-inset-bottom)] md:hidden"
      aria-label="Mobile"
    >
      <div className="mx-auto flex max-w-lg items-stretch justify-around">
        <NavLink to="/" className={tabClass} end>
          <Home className="size-5 shrink-0" aria-hidden />
          Home
        </NavLink>
        <NavLink to="/browse" className={tabClass}>
          <Compass className="size-5 shrink-0" aria-hidden />
          Browse
        </NavLink>
        {meQuery.data ? (
          <NavLink to="/library" className={tabClass}>
            <Film className="size-5 shrink-0" aria-hidden />
            Library
          </NavLink>
        ) : (
          <NavLink to="/login" className={tabClass}>
            <Film className="size-5 shrink-0" aria-hidden />
            Library
          </NavLink>
        )}
        <NavLink to={meQuery.data ? '/account' : '/login'} className={tabClass}>
          <UserRound className="size-5 shrink-0" aria-hidden />
          {meQuery.data ? 'Account' : 'Sign in'}
        </NavLink>
      </div>
    </nav>
  )
}
