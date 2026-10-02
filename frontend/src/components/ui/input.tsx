import { cn } from '@/lib/utils'

export type InputProps = React.ComponentProps<'input'> & {
  leadingIcon?: React.ReactNode
  trailingIcon?: React.ReactNode
  invalid?: boolean
}

function TopShine() {
  return (
    <span
      className="pointer-events-none absolute inset-x-4 top-0 z-[2] h-px bg-gradient-to-r from-transparent via-foreground/12 to-transparent"
      aria-hidden
    />
  )
}

function Input({
  className,
  type,
  leadingIcon,
  trailingIcon,
  invalid,
  ...props
}: InputProps) {
  const frameClass = cn(
    'input-premium-frame group/input relative rounded-[1.125rem] p-px transition-all duration-300 ease-out',
    invalid
      ? 'bg-gradient-to-br from-destructive/50 via-destructive/30 to-destructive/40'
      : 'bg-gradient-to-br from-border/90 via-border/70 to-border/90',
    !invalid && [
      'hover:from-primary/25 hover:via-violet-400/15 hover:to-primary/20',
      'focus-within:from-primary/55 focus-within:via-violet-400/35 focus-within:to-amber-400/45',
      'focus-within:shadow-[0_8px_32px_-8px_oklch(0.58_0.14_55/0.35)]',
    ],
    className,
  )

  const innerClass = cn(
    'input-premium-inner relative flex h-[3.25rem] w-full items-center gap-3 overflow-hidden rounded-[1.0625rem] px-4',
    'bg-gradient-to-b from-card to-card/95 dark:from-card/95 dark:to-card/80',
    'shadow-[inset_0_1px_0_0_oklch(1_0_0/0.12),0_1px_2px_oklch(0_0_0/0.04)]',
    'dark:shadow-[inset_0_1px_0_0_oklch(1_0_0/0.08),0_1px_3px_oklch(0_0_0/0.35)]',
    'has-[:disabled]:cursor-not-allowed has-[:disabled]:opacity-50',
  )

  const inputClass = cn(
    'relative z-[1] min-w-0 flex-1 bg-transparent py-3',
    'text-[0.9375rem] leading-none tracking-[0.01em] text-foreground outline-none',
    'placeholder:text-muted-foreground/45 placeholder:transition-colors',
    'focus:placeholder:text-muted-foreground/30',
    'disabled:cursor-not-allowed',
  )

  const iconPillClass = cn(
    'relative z-[1] flex size-9 shrink-0 items-center justify-center rounded-xl',
    'bg-primary/[0.07] text-muted-foreground ring-1 ring-primary/10',
    'transition-all duration-300 ease-out',
    'group-focus-within/input:bg-primary/12 group-focus-within/input:text-primary group-focus-within/input:ring-primary/25',
    '[&_svg]:size-[1.0625rem] [&_svg]:stroke-[1.75]',
  )

  return (
    <div className={frameClass} data-slot="input-shell">
      <div className={innerClass}>
        <TopShine />
        {leadingIcon && <span className={iconPillClass} aria-hidden>{leadingIcon}</span>}
        <input
          type={type}
          data-slot="input"
          aria-invalid={invalid || undefined}
          className={cn(inputClass, !leadingIcon && !trailingIcon && 'w-full')}
          {...props}
        />
        {trailingIcon && (
          <span className={cn(iconPillClass, 'bg-muted/50 ring-border/80 p-0')}>
            {trailingIcon}
          </span>
        )}
      </div>
    </div>
  )
}

export { Input }
