import { Link } from 'react-router-dom'
import { ChevronRight } from 'lucide-react'
import { MoviePosterTile } from '@/components/home/MoviePosterTile'
import type { MovieListItem } from '@/lib/movies'

type MovieRowProps = {
  title: string
  subtitle?: string
  movies: MovieListItem[]
  seeAllHref?: string
}

export function MovieRow({ title, subtitle, movies, seeAllHref }: MovieRowProps) {
  if (movies.length === 0) {
    return null
  }

  return (
    <section className="group/row relative">
      <div className="mb-3 flex items-end justify-between gap-4 pr-1">
        <div>
          <h2 className="text-lg font-semibold tracking-tight sm:text-xl">{title}</h2>
          {subtitle && <p className="mt-0.5 text-sm text-muted-foreground">{subtitle}</p>}
        </div>
        {seeAllHref && (
          <Link
            to={seeAllHref}
            className="flex shrink-0 items-center gap-0.5 text-sm font-medium text-muted-foreground transition hover:text-primary"
          >
            See all
            <ChevronRight className="size-4" />
          </Link>
        )}
      </div>

      <div
        className="scrollbar-hide -mx-4 flex gap-3 overflow-x-auto px-4 pb-2 scroll-smooth snap-x snap-mandatory sm:-mx-6 sm:gap-4 sm:px-6 lg:-mx-10 lg:px-10"
        role="list"
      >
        {movies.map((movie) => (
          <div key={movie.id} className="w-[9.5rem] shrink-0 snap-start sm:w-[10.5rem] md:w-[11.5rem]" role="listitem">
            <MoviePosterTile movie={movie} />
          </div>
        ))}
      </div>
    </section>
  )
}
