import { Link, Outlet } from 'react-router-dom'
import { Film, ShieldCheck, Sparkles } from 'lucide-react'
import { CinemaBackground } from '@/components/layout/CinemaBackground'
import { ThemeToggle } from '@/components/theme/ThemeToggle'

export function AuthLayout() {
  return (
    <div className="relative flex min-h-screen">
      <CinemaBackground />
      <div className="hidden w-[44%] flex-col justify-between border-r border-border bg-muted/30 p-10 lg:flex xl:p-14">
        <Link to="/" className="flex items-center gap-2.5 text-foreground">
          <span className="flex size-10 items-center justify-center rounded-xl bg-primary/15 text-primary ring-1 ring-primary/25">
            <Film className="size-5" />
          </span>
          <span className="text-xl font-semibold tracking-tight">
            AKHRA
          </span>
        </Link>

        <div className="space-y-6">
          <p className="max-w-md text-3xl font-semibold leading-tight tracking-tight text-balance">
            Stream films you&apos;re licensed to distribute — beautifully and securely.
          </p>
          <ul className="space-y-4 text-sm text-muted-foreground">
            <li className="flex items-start gap-3">
              <ShieldCheck className="mt-0.5 size-4 shrink-0 text-primary" />
              Server-verified entitlements before every playback
            </li>
            <li className="flex items-start gap-3">
              <Sparkles className="mt-0.5 size-4 shrink-0 text-primary" />
              Pay-per-view and subscriptions with transparent pricing
            </li>
          </ul>
        </div>

        <p className="text-xs text-muted-foreground">Built for rights holders · India-first (INR)</p>
      </div>

      <div className="relative flex flex-1 flex-col items-center justify-center px-4 py-12 sm:px-8">
        <div className="absolute right-4 top-4 sm:right-8 sm:top-8">
          <ThemeToggle />
        </div>
        <Link to="/" className="mb-8 flex items-center gap-2 lg:hidden">
          <Film className="size-5 text-primary" />
          <span className="font-semibold">AKHRA</span>
        </Link>
        <div className="w-full max-w-md">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
