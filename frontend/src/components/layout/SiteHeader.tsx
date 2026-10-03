import { Link, NavLink, useLocation } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Film, Menu, UserRound, X } from 'lucide-react'
import { useEffect, useState } from 'react'
import { ThemeToggle } from '@/components/theme/ThemeToggle'
import { Button } from '@/components/ui/button'
import { getMe } from '@/lib/auth'
import { cn } from '@/lib/utils'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'block rounded-xl px-4 py-3 text-base font-medium transition-colors',
    isActive ? 'bg-muted text-foreground' : 'text-muted-foreground hover:bg-muted/60 hover:text-foreground',
  )

const desktopNavLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'rounded-full px-3.5 py-1.5 text-sm font-medium transition-colors',
    isActive ? 'bg-muted text-foreground' : 'text-muted-foreground hover:text-foreground',
  )

export function SiteHeader() {
  const location = useLocation()
  const isHome = location.pathname === '/'
  const [menuOpen, setMenuOpen] = useState(false)
  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })

  useEffect(() => {
    setMenuOpen(false)
  }, [location.pathname])

  useEffect(() => {
    document.body.style.overflow = menuOpen ? 'hidden' : ''
    return () => {
      document.body.style.overflow = ''
    }
  }, [menuOpen])

  const navItems = (
    <>
      <NavLink to="/" className={navLinkClass} end onClick={() => setMenuOpen(false)}>
        Home
      </NavLink>
      <NavLink to="/browse" className={navLinkClass} onClick={() => setMenuOpen(false)}>
        Browse
      </NavLink>
      <NavLink to="/plans" className={navLinkClass} onClick={() => setMenuOpen(false)}>
        Plans
      </NavLink>
      {meQuery.data && (
        <NavLink to="/library" className={navLinkClass} onClick={() => setMenuOpen(false)}>
          Library
        </NavLink>
      )}
      {meQuery.data?.isAdmin && (
        <NavLink to="/admin" className={navLinkClass} onClick={() => setMenuOpen(false)}>
          Admin
        </NavLink>
      )}
    </>
  )

  return (
    <header
      className={cn(
        'sticky top-0 z-50 transition-colors pt-[env(safe-area-inset-top)]',
        isHome
          ? 'border-b border-transparent bg-gradient-to-b from-background/90 via-background/50 to-transparent backdrop-blur-sm'
          : 'border-b border-border bg-background/80 backdrop-blur-xl',
      )}
    >
      <div
        className={cn(
          'mx-auto flex h-14 min-h-14 items-center justify-between gap-2 px-3 sm:h-16 sm:gap-4 sm:px-6',
          isHome ? 'max-w-[1920px] lg:px-12' : 'max-w-6xl',
        )}
      >
        <Link to="/" className="group flex min-w-0 items-center gap-2">
          <span className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-primary/15 text-primary ring-1 ring-primary/30 transition group-hover:bg-primary/25">
            <Film className="size-4" strokeWidth={2.25} />
          </span>
          <span className="truncate font-heading text-lg font-semibold tracking-tight">AKHRA</span>
        </Link>

        <nav className="hidden items-center gap-1 md:flex" aria-label="Main">
          <NavLink to="/" className={desktopNavLinkClass} end>Home</NavLink>
          <NavLink to="/browse" className={desktopNavLinkClass}>Browse</NavLink>
          <NavLink to="/plans" className={desktopNavLinkClass}>Plans</NavLink>
          {meQuery.data && (
            <NavLink to="/library" className={desktopNavLinkClass}>Library</NavLink>
          )}
          {meQuery.data?.isAdmin && (
            <NavLink to="/admin" className={desktopNavLinkClass}>Admin</NavLink>
          )}
        </nav>

        <div className="flex items-center gap-1.5 sm:gap-2">
          <ThemeToggle className="hidden sm:flex" />
          <div className="hidden items-center gap-2 md:flex">
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
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="md:hidden"
            aria-expanded={menuOpen}
            aria-label={menuOpen ? 'Close menu' : 'Open menu'}
            onClick={() => setMenuOpen((o) => !o)}
          >
            {menuOpen ? <X className="size-5" /> : <Menu className="size-5" />}
          </Button>
        </div>
      </div>

      {menuOpen && (
        <div className="fixed inset-0 top-14 z-40 md:hidden sm:top-16">
          <button
            type="button"
            className="absolute inset-0 bg-black/50"
            aria-label="Close menu"
            onClick={() => setMenuOpen(false)}
          />
          <div
            className="relative max-h-[calc(100dvh-3.5rem)] overflow-y-auto border-b border-border bg-background px-3 pb-6 pt-2 shadow-xl sm:max-h-[calc(100dvh-4rem)]"
            role="dialog"
            aria-modal="true"
            aria-label="Navigation menu"
          >
            <nav className="flex flex-col gap-1" aria-label="Main mobile">
              {navItems}
            </nav>
            <div className="mt-4 flex items-center justify-between border-t border-border pt-4">
              <span className="text-sm text-muted-foreground">Theme</span>
              <ThemeToggle />
            </div>
            <div className="mt-4 flex flex-col gap-2">
              {meQuery.data ? (
                <Link to="/account" onClick={() => setMenuOpen(false)}>
                  <Button variant="secondary" className="h-11 w-full">Account</Button>
                </Link>
              ) : (
                <>
                  <Link to="/login" onClick={() => setMenuOpen(false)}>
                    <Button variant="outline" className="h-11 w-full">Sign in</Button>
                  </Link>
                  <Link to="/register" onClick={() => setMenuOpen(false)}>
                    <Button className="h-11 w-full">Get started</Button>
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </header>
  )
}
