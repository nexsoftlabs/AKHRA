import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { fetchLibrary } from '@/lib/playback'
import { formatDuration } from '@/lib/movies'

export function LibraryPage() {
  const libraryQuery = useQuery({ queryKey: ['library'], queryFn: fetchLibrary })

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6 sm:py-14">
      <h1 className="text-3xl font-semibold tracking-tight">Your library</h1>
      <p className="mt-2 text-muted-foreground">Titles you own and can stream.</p>

      {libraryQuery.isLoading && <p className="mt-8 text-muted-foreground">Loading…</p>}

      {libraryQuery.data?.length === 0 && (
        <div className="mt-10 rounded-2xl border border-border p-8 text-center">
          <p className="text-muted-foreground">No purchases yet.</p>
          <Link to="/browse" className="mt-4 inline-block">
            <Button>Browse movies</Button>
          </Link>
        </div>
      )}

      <ul className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {libraryQuery.data?.map((item) => (
          <li key={item.movieId} className="rounded-2xl border border-border p-4">
            <h2 className="font-semibold">{item.title}</h2>
            <p className="text-sm text-muted-foreground">{formatDuration(item.durationSeconds)}</p>
            <div className="mt-4 flex gap-2">
              <Link to={`/watch/${item.slug}`}>
                <Button size="sm">Watch</Button>
              </Link>
              <Link to={`/movies/${item.slug}`}>
                <Button size="sm" variant="outline">Details</Button>
              </Link>
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}
