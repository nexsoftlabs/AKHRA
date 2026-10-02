export function CinemaBackground() {
  return (
    <div className="pointer-events-none fixed inset-0 -z-10 overflow-hidden" aria-hidden>
      <div className="cinema-mesh absolute inset-0" />
      <div className="absolute -left-32 top-1/4 size-[28rem] rounded-full bg-primary/15 blur-3xl dark:bg-primary/10" />
      <div className="absolute -right-24 top-0 size-[22rem] rounded-full bg-violet-400/20 blur-3xl dark:bg-violet-500/15" />
      <div className="absolute bottom-0 left-1/3 size-[32rem] rounded-full bg-amber-300/25 blur-3xl dark:bg-amber-500/10" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,transparent_0%,var(--background)_72%)]" />
      <div
        className="absolute inset-0 opacity-[0.04] dark:opacity-[0.03]"
        style={{
          backgroundImage:
            'linear-gradient(currentColor 1px, transparent 1px), linear-gradient(90deg, currentColor 1px, transparent 1px)',
          backgroundSize: '64px 64px',
        }}
      />
    </div>
  )
}
