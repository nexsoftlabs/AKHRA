import { useMutation } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { createMovie } from '@/lib/admin'

export function AdminMovieNewPage() {
  const navigate = useNavigate()
  const mutation = useMutation({
    mutationFn: createMovie,
    onSuccess: (movie) => navigate(`/admin/movies/${movie.id}`),
  })

  return (
    <div className="mx-auto max-w-2xl px-4 py-10">
      <Link to="/admin/movies" className="text-sm text-muted-foreground hover:text-foreground">← Admin</Link>
      <h1 className="mt-4 text-2xl font-semibold">New movie</h1>
      <form
        className="mt-8 space-y-4"
        onSubmit={(e) => {
          e.preventDefault()
          const form = new FormData(e.currentTarget)
          mutation.mutate({
            title: String(form.get('title')),
            durationSeconds: Number(form.get('durationSeconds') || 3600),
            priceMinorUnits: Number(form.get('priceMinorUnits') || 5000),
            currency: 'INR',
            purchaseType: 'Lifetime',
            subscriptionEligible: true,
            isFeatured: false,
            genres: [String(form.get('genre') || 'drama')],
            license: {
              rightsHolder: String(form.get('rightsHolder') || 'AKHRA'),
              licenseReference: `LIC-${Date.now()}`,
              validFrom: new Date().toISOString(),
              validTo: new Date(Date.now() + 5 * 365 * 86400000).toISOString(),
              territories: ['IN'],
              allowsStreaming: true,
            },
          })
        }}
      >
        <Input name="title" placeholder="Title" required />
        <Input name="genre" placeholder="Genre slug (e.g. drama)" />
        <Input name="durationSeconds" type="number" placeholder="Duration (seconds)" />
        <Input name="priceMinorUnits" type="number" placeholder="Price in paise (5000 = ₹50)" />
        <Input name="rightsHolder" placeholder="Rights holder" />
        <Button type="submit" disabled={mutation.isPending}>{mutation.isPending ? 'Creating…' : 'Create draft'}</Button>
      </form>
      {mutation.isError && <p className="mt-4 text-sm text-destructive">{(mutation.error as Error).message}</p>}
    </div>
  )
}
