import { Outlet, useLocation } from 'react-router-dom'
import { CinemaBackground } from '@/components/layout/CinemaBackground'
import { MobileBottomNav } from '@/components/layout/MobileBottomNav'
import { SiteFooter } from '@/components/layout/SiteFooter'
import { SiteHeader } from '@/components/layout/SiteHeader'

export function AppShell() {
  const location = useLocation()
  const isWatch = /^\/watch\//.test(location.pathname)
  const showMobileNav = !isWatch

  return (
    <div className="relative flex min-h-[100dvh] flex-col overflow-x-hidden">
      <CinemaBackground />
      <SiteHeader />
      <main
        className={
          showMobileNav
            ? 'flex-1 pb-[calc(4.5rem+env(safe-area-inset-bottom))] md:pb-0'
            : 'flex-1'
        }
      >
        <Outlet />
      </main>
      <SiteFooter className={showMobileNav ? undefined : 'pb-0'} />
      {showMobileNav && <MobileBottomNav />}
    </div>
  )
}
