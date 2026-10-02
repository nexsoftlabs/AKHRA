import { Outlet } from 'react-router-dom'
import { CinemaBackground } from '@/components/layout/CinemaBackground'
import { SiteFooter } from '@/components/layout/SiteFooter'
import { SiteHeader } from '@/components/layout/SiteHeader'

export function AppShell() {
  return (
    <div className="relative flex min-h-screen flex-col">
      <CinemaBackground />
      <SiteHeader />
      <main className="flex-1">
        <Outlet />
      </main>
      <SiteFooter />
    </div>
  )
}
