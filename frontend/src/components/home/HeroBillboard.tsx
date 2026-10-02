import { Link } from 'react-router-dom'
import { Info, Play } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { formatDuration, formatPrice, type MovieDetail } from '@/lib/movies'

type HeroBillboardProps = {
  movie?: MovieDetail
  loading?: boolean
}

export function HeroBillboard({ movie, loading }: HeroBillboardProps) {
  const image = movie?.backdropUrl ?? movie?.posterUrl

  return (
    <section className="relative -mt-16 min-h-[min(88vh,720px)] w-full overflow-hidden sm:min-h-[78vh]">
      {loading && (
        <div className="absolute inset-0 animate-pulse bg-muted" aria-hidden />
      )}
      {!loading && image && (
        <img
          src={image}
          alt=""
          className="absolute inset-0 size-full object-cover object-top"
          fetchPriority="high"
        />
      )}
      {!loading && !image && (
        <div className="absolute inset-0 bg-gradient-to-br from-violet-950 via-background to-black" />
      )}

      <div className="absolute inset-0 bg-gradient-to-r from-background via-background/85 to-background/20" />
      <div className="absolute inset-0 bg-gradient-to-t from-background via-background/40 to-transparent" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_70%_20%,transparent_0%,var(--background)_55%)] opacity-80" />

      <div className="relative flex min-h-[min(88vh,720px)] flex-col justify-end px-4 pb-12 pt-28 sm:min-h-[78vh] sm:px-8 sm:pb-16 lg:px-12">
        <div className="mx-auto w-full max-w-[1920px]">
          {loading && (
            <div className="max-w-xl space-y-4">
              <div className="h-4 w-24 rounded bg-muted/80" />
              <div className="h-12 w-full max-w-md rounded bg-muted/80" />
              <div className="h-16 w-full max-w-lg rounded bg-muted/60" />
            </div>
          )}

          {!loading && movie && (
            <div className="max-w-2xl animate-in fade-in slide-in-from-bottom-4 duration-700">
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-primary sm:text-sm">
                {movie.genres.slice(0, 3).join(' · ')}
                {movie.ageRating ? ` · ${movie.ageRating}` : ''}
              </p>
              <h1 className="mt-3 font-heading text-4xl font-bold tracking-tight text-foreground drop-shadow-sm sm:text-5xl lg:text-6xl">
                {movie.title}
              </h1>
              <p className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-1 text-sm text-muted-foreground">
                {movie.releaseYear && <span>{movie.releaseYear}</span>}
                <span className="text-muted-foreground/50">|</span>
                <span>{formatDuration(movie.durationSeconds)}</span>
                <span className="text-muted-foreground/50">|</span>
                <span className="font-medium text-foreground">
                  {formatPrice(movie.priceMinorUnits, movie.currency)}
                </span>
              </p>
              {(movie.synopsis ?? movie.description) && (
                <p className="mt-4 line-clamp-3 text-sm leading-relaxed text-muted-foreground sm:text-base sm:line-clamp-4">
                  {movie.synopsis ?? movie.description}
                </p>
              )}
              <div className="mt-8 flex flex-wrap gap-3">
                <Link to={`/movies/${movie.slug}`}>
                  <Button size="lg" className="h-12 gap-2 px-8 text-base shadow-lg shadow-primary/30">
                    <Play className="size-5 fill-current" />
                    Watch trailer
                  </Button>
                </Link>
                <Link to={`/movies/${movie.slug}`}>
                  <Button size="lg" variant="secondary" className="h-12 gap-2 px-8 text-base bg-muted/80 backdrop-blur-sm">
                    <Info className="size-5" />
                    More info
                  </Button>
                </Link>
              </div>
            </div>
          )}

          {!loading && !movie && (
            <div className="max-w-xl">
              <h1 className="font-heading text-4xl font-bold tracking-tight sm:text-5xl">
                Stories worth the rights
              </h1>
              <p className="mt-4 text-muted-foreground">
                Licensed films, ready when you are. Browse the catalog to get started.
              </p>
              <Link to="/browse" className="mt-8 inline-block">
                <Button size="lg">Explore catalog</Button>
              </Link>
            </div>
          )}
        </div>
      </div>
    </section>
  )
}
