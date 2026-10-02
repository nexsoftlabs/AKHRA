import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useState } from 'react'
import { ArrowLeft, Loader2, Play, ShoppingBag } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { GlassCard } from '@/components/ui/glass-card'
import { getMe } from '@/lib/auth'
import { createMovieCheckout } from '@/lib/checkout'
import { fetchMovieBySlug, formatDuration, formatPrice } from '@/lib/movies'
import { openRazorpayCheckout } from '@/lib/razorpay'

export function MovieDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [checkoutError, setCheckoutError] = useState<string | null>(null)
  const [checkoutPending, setCheckoutPending] = useState(false)

  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })

  const movieQuery = useQuery({
    queryKey: ['movie', slug],
    queryFn: () => fetchMovieBySlug(slug!),
    enabled: Boolean(slug),
  })

  async function handleBuyNow() {
    if (!slug) return
    setCheckoutError(null)

    if (!meQuery.data) {
      navigate('/login')
      return
    }

    setCheckoutPending(true)
    try {
      const session = await createMovieCheckout(slug)
      const result = await openRazorpayCheckout(session, {
        email: meQuery.data.email ?? undefined,
        name: meQuery.data.displayName ?? undefined,
      })
      if (result.success) {
        await queryClient.invalidateQueries({ queryKey: ['movie', slug] })
        await movieQuery.refetch()
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Checkout failed.'
      if (message !== 'Payment cancelled.') {
        setCheckoutError(message)
      }
    } finally {
      setCheckoutPending(false)
    }
  }

  if (movieQuery.isLoading) {
    return <div className="mx-auto max-w-5xl px-4 py-16 text-muted-foreground">Loading…</div>
  }

  if (movieQuery.isError || !movieQuery.data) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16">
        <p className="text-destructive">Movie not found.</p>
        <Link to="/browse" className="mt-4 inline-block text-primary underline">Back to browse</Link>
      </div>
    )
  }

  const movie = movieQuery.data
  const price = formatPrice(movie.priceMinorUnits, movie.currency)

  return (
    <div className="mx-auto max-w-5xl px-4 pb-16 pt-8 sm:px-6">
      <Link to="/browse" className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="size-4" />
        Back to browse
      </Link>

      <div className="grid gap-8 lg:grid-cols-[280px_1fr]">
        <div className="overflow-hidden rounded-2xl border border-border shadow-lg">
          {movie.posterUrl ? (
            <img src={movie.posterUrl} alt="" className="aspect-[2/3] w-full object-cover" />
          ) : (
            <div className="aspect-[2/3] bg-muted" />
          )}
        </div>

        <div className="space-y-6">
          <div>
            <div className="flex flex-wrap gap-2">
              {movie.genres.map((g) => (
                <Badge key={g} variant="secondary">{g}</Badge>
              ))}
              {movie.ageRating && <Badge variant="outline">{movie.ageRating}</Badge>}
            </div>
            <h1 className="mt-4 text-3xl font-semibold tracking-tight sm:text-4xl">{movie.title}</h1>
            <p className="mt-2 text-muted-foreground">
              {movie.releaseYear} · {formatDuration(movie.durationSeconds)} · {movie.language?.toUpperCase()}
            </p>
          </div>

          <p className="text-lg leading-relaxed text-foreground/90">{movie.synopsis}</p>

          <GlassCard className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Buy to stream</p>
              <p className="text-2xl font-semibold text-primary">{price}</p>
              <p className="text-xs text-muted-foreground">Lifetime access · INR · licensed for India</p>
            </div>
            <div className="flex flex-wrap gap-3">
              {movie.trailerUrl && (
                <Button
                  variant="outline"
                  type="button"
                  onClick={() => window.open(movie.trailerUrl!, '_blank', 'noopener,noreferrer')}
                >
                  <Play className="size-4" />
                  Preview trailer
                </Button>
              )}
              {movie.hasAccess ? (
                <Link to={`/watch/${movie.slug}`}>
                  <Button className="shadow-lg shadow-primary/20">Watch now</Button>
                </Link>
              ) : (
                <Button
                  className="shadow-lg shadow-primary/20"
                  disabled={checkoutPending}
                  onClick={() => void handleBuyNow()}
                >
                  {checkoutPending ? (
                    <Loader2 className="size-4 animate-spin" />
                  ) : (
                    <ShoppingBag className="size-4" />
                  )}
                  {checkoutPending ? 'Opening Razorpay…' : `Buy now — ${price}`}
                </Button>
              )}
            </div>
            {checkoutError && (
              <p className="w-full text-sm text-destructive">{checkoutError}</p>
            )}
          </GlassCard>
        </div>
      </div>
    </div>
  )
}
