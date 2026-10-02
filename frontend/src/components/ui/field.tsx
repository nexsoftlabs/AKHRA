import { cn } from '@/lib/utils'

export function Field({
  id,
  label,
  hint,
  error,
  className,
  children,
}: {
  id: string
  label: string
  hint?: string
  error?: string
  className?: string
  children: React.ReactNode
}) {
  return (
    <div className={cn('space-y-2.5', className)}>
      <label
        htmlFor={id}
        className="block text-[0.6875rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground/90"
      >
        {label}
      </label>
      {children}
      {error ? (
        <p className="text-xs font-medium tracking-wide text-destructive" role="alert">
          {error}
        </p>
      ) : hint ? (
        <p className="text-xs leading-relaxed text-muted-foreground/80">{hint}</p>
      ) : null}
    </div>
  )
}
