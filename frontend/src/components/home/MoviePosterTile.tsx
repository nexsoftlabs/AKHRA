import { Link } from 'react-router-dom'
import { Play } from 'lucide-react'
import { formatDuration, type MovieListItem } from '@/lib/movies'

export function MoviePosterTile({ movie }: { movie: MovieListItem }) {
  return (
    <Link
      to={`/movies/${movie.slug}`}
      className="group relative block aspect-[2/3] overflow-hidden rounded-md bg-muted shadow-lg ring-1 ring-white/5 transition duration-300 hover:z-10 hover:scale-[1.06] hover:shadow-2xl hover:ring-primary/40"
    >
      {movie.posterUrl ? (
        <img
          src={movie.posterUrl}
          alt=""
          className="absolute inset-0 size-full object-cover"
          loading="lazy"
        />
      ) : (
        <div className="absolute inset-0 bg-gradient-to-br from-primary/40 to-violet-900/60" />
      )}
      <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-transparent to-transparent opacity-80 transition group-hover:opacity-100" />
      <div className="absolute inset-0 flex items-center justify-center opacity-0 transition group-hover:opacity-100">
        <span className="flex size-11 items-center justify-center rounded-full bg-white/25 text-white ring-1 ring-white/40 backdrop-blur-sm">
          <Play className="size-5 fill-white" />
        </span>
      </div>
      <div className="absolute inset-x-0 bottom-0 p-2.5">
        <p className="line-clamp-2 text-xs font-semibold leading-tight text-white sm:text-sm">{movie.title}</p>
        <p className="mt-0.5 text-[0.65rem] text-white/70 sm:text-xs">
          {movie.releaseYear ?? '—'} · {formatDuration(movie.durationSeconds)}
        </p>
      </div>
    </Link>
  )
}
