import { Link } from 'react-router-dom'
import { Play } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { formatDuration, formatPrice, type MovieListItem } from '@/lib/movies'

export function MovieCard({ movie }: { movie: MovieListItem }) {
  const genre = movie.genres[0] ?? 'Film'

  return (
    <Link
      to={`/movies/${movie.slug}`}
      className="group relative block aspect-[2/3] overflow-hidden rounded-2xl border border-border bg-muted/40 shadow-md transition hover:-translate-y-1 hover:border-primary/35 hover:shadow-xl"
    >
      {movie.posterUrl ? (
        <img
          src={movie.posterUrl}
          alt=""
          className="absolute inset-0 size-full object-cover transition duration-500 group-hover:scale-105"
          loading="lazy"
        />
      ) : (
        <div className="absolute inset-0 bg-gradient-to-br from-primary/30 to-violet-600/40" />
      )}
      <div className="absolute inset-0 bg-gradient-to-t from-black/95 via-black/30 to-transparent" />
      {movie.isFeatured && (
        <Badge className="absolute left-3 top-3 border-0 bg-primary/90 text-primary-foreground">Featured</Badge>
      )}
      <div className="absolute inset-0 flex items-center justify-center opacity-70 transition sm:opacity-0 sm:group-hover:opacity-100">
        <span className="flex size-12 items-center justify-center rounded-full bg-white/20 text-white ring-1 ring-white/30 backdrop-blur-md">
          <Play className="size-5 fill-white" />
        </span>
      </div>
      <div className="absolute inset-x-0 bottom-0 p-4 text-white">
        <p className="text-[0.65rem] font-semibold uppercase tracking-[0.14em] text-primary-foreground/90">
          {genre}
        </p>
        <h3 className="mt-1 line-clamp-2 text-sm font-semibold leading-snug sm:text-base">{movie.title}</h3>
        <p className="mt-2 text-xs text-white/75">
          {movie.releaseYear ?? '—'} · {formatDuration(movie.durationSeconds)} ·{' '}
          <span className="font-medium text-white">{formatPrice(movie.priceMinorUnits, movie.currency)}</span>
        </p>
      </div>
    </Link>
  )
}
