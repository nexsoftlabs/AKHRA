import { Link } from 'react-router-dom'

import { cn } from '@/lib/utils'

export function SiteFooter({ className }: { className?: string }) {
  return (
    <footer
      className={cn(
        'mt-auto border-t border-border bg-background/60 pb-[calc(4.5rem+env(safe-area-inset-bottom))] md:pb-0',
        className,
      )}
    >
      <div className="mx-auto flex max-w-6xl flex-col gap-4 px-4 py-8 sm:flex-row sm:items-center sm:justify-between sm:px-6 sm:py-10">
        <p className="text-sm text-muted-foreground">
          © {new Date().getFullYear()} AKHRA — licensed streaming only.
        </p>
        <div className="flex flex-wrap gap-4 text-sm text-muted-foreground">
          <Link to="/browse" className="transition hover:text-foreground">Catalog</Link>
          <Link to="/account" className="transition hover:text-foreground">Account</Link>
          <Link to="/terms" className="transition hover:text-foreground">Terms</Link>
          <Link to="/privacy" className="transition hover:text-foreground">Privacy</Link>
          <Link to="/admin/login" className="transition hover:text-foreground">Staff login</Link>
        </div>
      </div>
    </footer>
  )
}
