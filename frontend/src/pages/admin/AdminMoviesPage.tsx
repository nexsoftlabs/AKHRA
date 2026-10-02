import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { fetchAdminMovies, publishMovie } from '@/lib/admin'

export function AdminMoviesPage() {
  const [q, setQ] = useState('')
  const moviesQuery = useQuery({ queryKey: ['admin-movies', q], queryFn: () => fetchAdminMovies(q) })

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-semibold">Catalog admin</h1>
          <p className="mt-1 text-sm text-muted-foreground">Manage movies, licenses, and publication.</p>
        </div>
        <Link to="/admin/movies/new">
          <Button>Add movie</Button>
        </Link>
      </div>

      <div className="mt-6 max-w-md">
        <Input placeholder="Search title or slug…" value={q} onChange={(e) => setQ(e.target.value)} />
      </div>

      <div className="mt-8 overflow-x-auto rounded-2xl border border-border">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-border bg-muted/40">
            <tr>
              <th className="px-4 py-3 font-medium">Title</th>
              <th className="px-4 py-3 font-medium">Status</th>
              <th className="px-4 py-3 font-medium">Price</th>
              <th className="px-4 py-3 font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {moviesQuery.data?.map((m) => (
              <tr key={m.id} className="border-b border-border/60">
                <td className="px-4 py-3">
                  <p className="font-medium">{m.title}</p>
                  <p className="text-xs text-muted-foreground">{m.slug}</p>
                </td>
                <td className="px-4 py-3">{m.publicationStatus}</td>
                <td className="px-4 py-3">₹{m.priceMinorUnits / 100}</td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap gap-2">
                    <Link to={`/admin/movies/${m.id}`}>
                      <Button size="sm" variant="outline">Edit</Button>
                    </Link>
                    {m.publicationStatus !== 'Published' && (
                      <Button
                        size="sm"
                        onClick={() => publishMovie(m.id).then(() => moviesQuery.refetch())}
                      >
                        Publish
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
