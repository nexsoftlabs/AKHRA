import { cn } from '@/lib/utils'

export function GlassCard({
  className,
  children,
  ...props
}: React.ComponentProps<'div'>) {
  return (
    <div
      className={cn(
        'rounded-2xl border border-border bg-card/80 p-6 shadow-lg shadow-black/5 backdrop-blur-xl dark:bg-card/40 dark:shadow-black/40',
        className,
      )}
      {...props}
    >
      {children}
    </div>
  )
}
