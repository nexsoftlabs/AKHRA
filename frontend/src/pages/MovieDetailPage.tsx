import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { ArrowLeft, Loader2, Play, ShoppingBag } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { GlassCard } from '@/components/ui/glass-card'
import { getMe } from '@/lib/auth'
import { createMovieCheckout } from '@/lib/checkout'
import { fetchMovieBySlug, formatDuration, formatPrice } from '@/lib/movies'
import { openRazorpayCheckout } from '@/lib/razorpay'
import { parseVimeoVideoId } from '@/lib/vimeo'
import { VimeoPlayer } from '@/components/player/VimeoPlayer'

export function MovieDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [checkoutError, setCheckoutError] = useState<string | null>(null)
  const [checkoutPending, setCheckoutPending] = useState(false)
  const [previewVimeoId, setPreviewVimeoId] = useState<string | null>(null)
  const [previewSession, setPreviewSession] = useState(0)

  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })

  useEffect(() => {
    if (!previewVimeoId) return
    setPreviewSession((n) => n + 1)
  }, [meQuery.data?.id, previewVimeoId])

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
  const isFree = movie.priceMinorUnits === 0
  const previewId =
    movie.vimeoVideoId ?? parseVimeoVideoId(movie.trailerUrl ?? undefined)
  const guestPreviewSeconds = 60
  /** Preview modal is always capped; full playback is only on /watch with server checks. */
  const previewMaxSeconds = guestPreviewSeconds

  function openPreview(videoId: string) {
    setPreviewSession((n) => n + 1)
    setPreviewVimeoId(videoId)
  }

  return (
    <div className="mx-auto max-w-5xl px-3 pb-24 pt-6 sm:px-6 sm:pb-16 sm:pt-8">
      <Link to="/browse" className="mb-4 inline-flex min-h-11 items-center gap-2 text-sm text-muted-foreground hover:text-foreground sm:mb-6">
        <ArrowLeft className="size-4" />
        Back to browse
      </Link>

      <div className="grid gap-6 sm:gap-8 lg:grid-cols-[280px_1fr]">
        <div className="mx-auto w-full max-w-[220px] overflow-hidden rounded-2xl border border-border shadow-lg sm:max-w-none lg:mx-0">
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
            <h1 className="mt-4 text-2xl font-semibold tracking-tight sm:text-4xl">{movie.title}</h1>
            <p className="mt-2 text-muted-foreground">
              {movie.releaseYear} · {formatDuration(movie.durationSeconds)} · {movie.language?.toUpperCase()}
            </p>
          </div>

          <p className="text-lg leading-relaxed text-foreground/90">{movie.synopsis}</p>

          <GlassCard className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-muted-foreground">{isFree ? 'Stream' : 'Buy to stream'}</p>
              <p className="text-2xl font-semibold text-primary">{isFree ? 'Free' : price}</p>
              <p className="text-xs text-muted-foreground">
                {isFree ? 'Sign in to watch in AKHRA' : 'Lifetime access · INR · licensed for India'}
              </p>
            </div>
            <div className="flex w-full flex-col gap-2 sm:w-auto sm:flex-row sm:flex-wrap sm:gap-3">
              {previewId && (
                <Button
                  variant="outline"
                  type="button"
                  className="h-11 w-full sm:w-auto"
                  onClick={() => openPreview(previewId)}
                >
                  <Play className="size-4" />
                  Preview
                </Button>
              )}
              {movie.hasAccess ? (
                <Link to={`/watch/${movie.slug}`} className="w-full sm:w-auto">
                  <Button className="h-11 w-full shadow-lg shadow-primary/20 sm:w-auto">Watch now</Button>
                </Link>
              ) : isFree ? (
                <Link to="/login" className="w-full sm:w-auto">
                  <Button className="h-11 w-full shadow-lg shadow-primary/20 sm:w-auto">Sign in to watch</Button>
                </Link>
              ) : (
                <Button
                  className="h-11 w-full shadow-lg shadow-primary/20 sm:w-auto"
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

      {previewVimeoId && (
        <div
          className="fixed inset-0 z-[60] flex flex-col bg-black sm:items-center sm:justify-center sm:bg-black/80 sm:p-4"
          role="dialog"
          aria-modal="true"
          onClick={() => setPreviewVimeoId(null)}
        >
          <div
            className="flex min-h-0 flex-1 flex-col overflow-hidden sm:max-h-[90dvh] sm:w-full sm:max-w-4xl sm:flex-none sm:rounded-2xl sm:border sm:border-border sm:shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <p className="shrink-0 border-b border-border px-4 py-2.5 text-center text-xs text-muted-foreground pt-[max(0.5rem,env(safe-area-inset-top))]">
              Preview — first {guestPreviewSeconds} seconds
              {movie.hasAccess ? ' · use Watch now for the full title' : ''}
            </p>
            <div className="flex min-h-0 flex-1 items-center">
              <VimeoPlayer
                key={`${previewVimeoId}-${previewSession}`}
                videoId={previewVimeoId}
                title={movie.title}
                className="aspect-video w-full min-h-[min(56vw,50dvh)] bg-black sm:min-h-[200px]"
                maxDurationSeconds={previewMaxSeconds}
                limitReachedMessage={
                  movie.hasAccess
                    ? `Preview ended (${guestPreviewSeconds}s). Use Watch now for the full title.`
                    : meQuery.data
                      ? `Preview ended (${guestPreviewSeconds}s). Purchase or sign in with access to watch the full title.`
                      : undefined
                }
                limitReachedActions={
                  movie.hasAccess ? (
                    <Link to={`/watch/${movie.slug}`}>
                      <Button size="sm">Watch now</Button>
                    </Link>
                  ) : undefined
                }
              />
            </div>
            <div className="flex shrink-0 justify-end border-t border-border p-3 pb-[max(0.75rem,env(safe-area-inset-bottom))]">
              <Button variant="outline" type="button" className="h-11 min-w-[5.5rem]" onClick={() => setPreviewVimeoId(null)}>
                Close
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
