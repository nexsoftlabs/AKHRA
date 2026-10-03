import { Link } from 'react-router-dom'
import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Sparkles } from 'lucide-react'
import { HeroBillboard } from '@/components/home/HeroBillboard'
import { MovieRow } from '@/components/home/MovieRow'
import { Button } from '@/components/ui/button'
import { getMe } from '@/lib/auth'
import { fetchMovieBySlug, fetchMovies, type MovieListItem } from '@/lib/movies'

function groupByPrimaryGenre(movies: MovieListItem[]) {
  const map = new Map<string, MovieListItem[]>()
  for (const movie of movies) {
    const genre = movie.genres[0] ?? 'Spotlight'
    const list = map.get(genre) ?? []
    list.push(movie)
    map.set(genre, list)
  }
  return [...map.entries()].sort(([a], [b]) => a.localeCompare(b))
}

export function HomePage() {
  const moviesQuery = useQuery({
    queryKey: ['movies', 'home'],
    queryFn: () => fetchMovies(),
    retry: 1,
  })

  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })

  const movies = moviesQuery.data ?? []
  const heroCandidate = movies.find((m) => m.isFeatured) ?? movies[0]

  const heroDetailQuery = useQuery({
    queryKey: ['movie', 'hero', heroCandidate?.slug],
    queryFn: () => fetchMovieBySlug(heroCandidate!.slug),
    enabled: Boolean(heroCandidate?.slug),
  })

  const featured = useMemo(() => movies.filter((m) => m.isFeatured), [movies])
  const spotlight = useMemo(() => {
    if (featured.length >= 4) return featured
    return movies.slice(0, 8)
  }, [featured, movies])

  const genreRows = useMemo(() => groupByPrimaryGenre(movies), [movies])

  return (
    <div className="pb-6 md:pb-12">
      <HeroBillboard movie={heroDetailQuery.data} loading={moviesQuery.isLoading || heroDetailQuery.isLoading} />

      <div className="relative z-10 -mt-6 space-y-10 sm:-mt-10 sm:space-y-12">
        {moviesQuery.isError && (
          <p className="mx-auto max-w-[1920px] px-4 text-center text-sm text-muted-foreground sm:px-8">
            Catalog unavailable — start the API and refresh.
          </p>
        )}

        {moviesQuery.isLoading && (
          <div className="mx-auto max-w-[1920px] px-4 sm:px-8">
            <div className="flex gap-4 overflow-hidden">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="aspect-[2/3] w-40 shrink-0 animate-pulse rounded-md bg-muted" />
              ))}
            </div>
          </div>
        )}

        {movies.length > 0 && (
          <div className="mx-auto max-w-[1920px] space-y-10 px-4 sm:space-y-12 sm:px-8 lg:px-12">
            <MovieRow
              title="Featured for you"
              subtitle="Hand-picked licensed titles"
              movies={spotlight}
              seeAllHref="/browse"
            />

            {genreRows.map(([genre, titles]) => (
              <MovieRow
                key={genre}
                title={genre.charAt(0).toUpperCase() + genre.slice(1)}
                movies={titles}
                seeAllHref={`/browse?genre=${encodeURIComponent(titles[0]?.genres[0] ?? genre.toLowerCase())}`}
              />
            ))}

            {!meQuery.data && (
              <section className="relative overflow-hidden rounded-2xl border border-border bg-gradient-to-br from-primary/10 via-muted/50 to-violet-500/10 px-6 py-10 sm:px-10">
                <div className="relative max-w-lg">
                  <Sparkles className="size-8 text-primary" />
                  <h2 className="mt-4 text-2xl font-semibold tracking-tight">Start watching in minutes</h2>
                  <p className="mt-2 text-sm text-muted-foreground sm:text-base">
                    Create an account, confirm your email, and unlock rentals or an all-access plan for eligible titles.
                  </p>
                  <div className="mt-6 flex flex-wrap gap-3">
                    <Link to="/register">
                      <Button size="lg">Join AKHRA</Button>
                    </Link>
                    <Link to="/plans">
                      <Button size="lg" variant="outline">View plans</Button>
                    </Link>
                  </div>
                </div>
              </section>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
