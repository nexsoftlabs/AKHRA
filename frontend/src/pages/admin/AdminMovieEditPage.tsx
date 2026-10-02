import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { fetchAdminMovie, publishMovie, updateAdminMovie } from '@/lib/admin'

export function AdminMovieEditPage() {
  const { id } = useParams<{ id: string }>()
  const queryClient = useQueryClient()
  const movieQuery = useQuery({
    queryKey: ['admin-movie', id],
    queryFn: () => fetchAdminMovie(id!),
    enabled: Boolean(id),
  })

  const saveMutation = useMutation({
    mutationFn: (body: Record<string, unknown>) => updateAdminMovie(id!, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-movie', id] })
      queryClient.invalidateQueries({ queryKey: ['admin-movies'] })
    },
  })

  if (!id) {
    return <p className="p-10">Missing movie id.</p>
  }

  if (movieQuery.isLoading) {
    return <p className="p-10 text-muted-foreground">Loading…</p>
  }

  if (movieQuery.isError || !movieQuery.data) {
    return <p className="p-10 text-destructive">Movie not found.</p>
  }

  const movie = movieQuery.data

  return (
    <div className="mx-auto max-w-2xl px-4 py-10">
      <Link to="/admin/movies" className="text-sm text-muted-foreground hover:text-foreground">← Catalog admin</Link>
      <h1 className="mt-4 text-2xl font-semibold">Edit movie</h1>
      <p className="mt-1 text-sm text-muted-foreground">{movie.slug}</p>

      <form
        className="mt-8 space-y-4"
        onSubmit={(e) => {
          e.preventDefault()
          const form = new FormData(e.currentTarget)
          saveMutation.mutate({
            title: String(form.get('title')),
            slug: String(form.get('slug')),
            description: String(form.get('description') || ''),
            synopsis: movie.synopsis,
            language: movie.language,
            releaseYear: movie.releaseYear,
            durationSeconds: Number(form.get('durationSeconds')),
            ageRating: movie.ageRating,
            posterUrl: movie.posterUrl,
            backdropUrl: movie.backdropUrl,
            trailerUrl: movie.trailerUrl,
            priceMinorUnits: Number(form.get('priceMinorUnits')),
            currency: movie.currency,
            purchaseType: movie.purchaseType,
            subscriptionEligible: form.get('subscriptionEligible') === 'on',
            publicationStatus: movie.publicationStatus,
            availabilityStart: movie.availabilityStart,
            availabilityEnd: movie.availabilityEnd,
            licenseTerritories: movie.licenseTerritories,
            isFeatured: form.get('isFeatured') === 'on',
            genres: [String(form.get('genre') || 'drama')],
            license: movie.license,
          })
        }}
      >
        <Input name="title" defaultValue={movie.title} required />
        <Input name="slug" defaultValue={movie.slug} required />
        <Input name="description" defaultValue={movie.description ?? ''} />
        <Input name="genre" defaultValue={movie.genres[0] ?? 'drama'} />
        <Input name="durationSeconds" type="number" defaultValue={movie.durationSeconds} />
        <Input name="priceMinorUnits" type="number" defaultValue={movie.priceMinorUnits} />
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" name="subscriptionEligible" defaultChecked={movie.subscriptionEligible} />
          Subscription eligible
        </label>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" name="isFeatured" defaultChecked={movie.isFeatured} />
          Featured on browse
        </label>
        <div className="flex flex-wrap gap-2">
          <Button type="submit" disabled={saveMutation.isPending}>
            {saveMutation.isPending ? 'Saving…' : 'Save changes'}
          </Button>
          {movie.publicationStatus !== 'Published' && (
            <Button
              type="button"
              variant="secondary"
              onClick={() => publishMovie(id).then(() => movieQuery.refetch())}
            >
              Publish
            </Button>
          )}
        </div>
      </form>
      {saveMutation.isSuccess && <p className="mt-4 text-sm text-primary">Saved.</p>}
      {saveMutation.isError && (
        <p className="mt-4 text-sm text-destructive">{(saveMutation.error as Error).message}</p>
      )}
    </div>
  )
}
