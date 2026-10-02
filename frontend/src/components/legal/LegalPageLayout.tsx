import { Link } from 'react-router-dom'
import type { ReactNode } from 'react'

type LegalPageLayoutProps = {
  title: string
  lastUpdated: string
  summary: string
  children: ReactNode
}

export function LegalPageLayout({ title, lastUpdated, summary, children }: LegalPageLayoutProps) {
  return (
    <article className="mx-auto max-w-3xl px-4 py-10 sm:px-6 sm:py-14">
      <header className="mb-10 border-b border-border pb-8">
        <p className="text-sm font-medium text-primary">Legal</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl">{title}</h1>
        <p className="mt-3 text-sm text-muted-foreground">Last updated: {lastUpdated}</p>
        <p className="mt-4 text-base leading-relaxed text-muted-foreground">{summary}</p>
        <p className="mt-4 rounded-xl border border-amber-500/30 bg-amber-500/10 px-4 py-3 text-sm text-foreground/90">
          AKHRA is a licensed streaming service. These documents describe how we operate today; have qualified
          legal counsel review them before commercial launch in your markets.
        </p>
      </header>
      <div className="legal-prose space-y-10 text-[0.9375rem] leading-relaxed text-foreground/90 [&_h2]:scroll-mt-24 [&_h2]:text-xl [&_h2]:font-semibold [&_h2]:tracking-tight [&_h3]:mt-6 [&_h3]:text-base [&_h3]:font-semibold [&_li]:mt-2 [&_ol]:mt-3 [&_ol]:list-decimal [&_ol]:pl-6 [&_p]:mt-3 [&_ul]:mt-3 [&_ul]:list-disc [&_ul]:pl-6">
        {children}
      </div>
      <footer className="mt-14 flex flex-wrap gap-4 border-t border-border pt-8 text-sm text-muted-foreground">
        <Link to="/terms" className="font-medium text-primary hover:underline">Terms of Service</Link>
        <Link to="/privacy" className="font-medium text-primary hover:underline">Privacy Policy</Link>
        <Link to="/" className="hover:text-foreground">Back to home</Link>
      </footer>
    </article>
  )
}
