import { useQuery } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { MovieCard } from '@/components/movies/MovieCard'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { fetchCatalogGenres, fetchMovies } from '@/lib/movies'

export function BrowsePage() {
  const [searchParams] = useSearchParams()
  const [q, setQ] = useState('')
  const [genre, setGenre] = useState<string | undefined>(() => searchParams.get('genre') ?? undefined)

  useEffect(() => {
    const g = searchParams.get('genre')
    setGenre(g ?? undefined)
  }, [searchParams])
  const genresQuery = useQuery({ queryKey: ['catalog-genres'], queryFn: fetchCatalogGenres })
  const moviesQuery = useQuery({
    queryKey: ['movies', q, genre],
    queryFn: () => fetchMovies(q || undefined, genre),
  })

  return (
    <div className="mx-auto max-w-6xl px-3 py-8 sm:px-6 sm:py-14">
      <header className="mb-8 max-w-2xl sm:mb-10">
        <h1 className="text-2xl font-semibold tracking-tight sm:text-4xl">Browse movies</h1>
        <p className="mt-3 text-muted-foreground">
          Licensed titles available for rent or purchase. Streaming unlocks after verified payment.
        </p>
        <Input
          className="mt-6 h-11 w-full max-w-md"
          placeholder="Search catalog…"
          value={q}
          onChange={(e) => setQ(e.target.value)}
        />
        {genresQuery.data && genresQuery.data.length > 0 && (
          <div className="mt-4 flex flex-wrap gap-2">
            <Button
              size="sm"
              variant={genre ? 'outline' : 'default'}
              onClick={() => setGenre(undefined)}
            >
              All genres
            </Button>
            {genresQuery.data.map((g) => (
              <Button
                key={g}
                size="sm"
                variant={genre === g ? 'default' : 'outline'}
                onClick={() => setGenre(g)}
              >
                {g}
              </Button>
            ))}
          </div>
        )}
      </header>

      {moviesQuery.isLoading && (
        <div className="grid grid-cols-2 gap-3 sm:gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="aspect-[2/3] animate-pulse rounded-2xl bg-muted" />
          ))}
        </div>
      )}

      {moviesQuery.isError && (
        <p className="rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Could not load catalog. Make sure the API is running and restart it once to seed demo movies.
        </p>
      )}

      {moviesQuery.data && (
        <div className="grid grid-cols-2 gap-3 sm:gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {moviesQuery.data.map((movie) => (
            <MovieCard key={movie.id} movie={movie} />
          ))}
        </div>
      )}
    </div>
  )
}
