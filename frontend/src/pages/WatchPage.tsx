import { useQuery } from '@tanstack/react-query'
import Hls from 'hls.js'
import { useEffect, useRef } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { VimeoPlayer } from '@/components/player/VimeoPlayer'
import { getMe } from '@/lib/auth'
import { startPlayback } from '@/lib/playback'

export function WatchPage() {
  const { slug } = useParams<{ slug: string }>()
  const videoRef = useRef<HTMLVideoElement>(null)

  const meQuery = useQuery({
    queryKey: ['me'],
    queryFn: getMe,
    retry: false,
  })

  const playbackQuery = useQuery({
    queryKey: ['playback', slug],
    queryFn: () => startPlayback(slug!),
    enabled: Boolean(slug) && meQuery.isSuccess && Boolean(meQuery.data),
    retry: false,
  })

  const playback = playbackQuery.data
  const isVimeo = playback?.playbackType === 'vimeo' && playback.vimeoVideoId

  useEffect(() => {
    if (isVimeo) return

    const video = videoRef.current
    const manifestUrl = playback?.manifestUrl
    if (!video || !manifestUrl) return

    const absoluteUrl = manifestUrl.startsWith('http')
      ? manifestUrl
      : `${import.meta.env.VITE_API_BASE_URL ?? ''}${manifestUrl}`

    if (Hls.isSupported()) {
      const hls = new Hls()
      hls.loadSource(absoluteUrl)
      hls.attachMedia(video)
      return () => hls.destroy()
    }

    if (video.canPlayType('application/vnd.apple.mpegurl')) {
      video.src = absoluteUrl
    }
  }, [isVimeo, playback?.manifestUrl])

  if (meQuery.isLoading || meQuery.isFetching) {
    return <p className="mx-auto max-w-5xl px-4 py-16 text-muted-foreground">Checking sign-in…</p>
  }

  if (!meQuery.data) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16">
        <p className="text-destructive">Sign in to watch this title.</p>
        <Link to="/login" className="mt-4 inline-block">
          <Button>Sign in</Button>
        </Link>
      </div>
    )
  }

  if (playbackQuery.isLoading) {
    return <p className="mx-auto max-w-5xl px-4 py-16 text-muted-foreground">Preparing playback…</p>
  }

  if (playbackQuery.isError) {
    const message =
      playbackQuery.error instanceof Error
        ? playbackQuery.error.message
        : 'Playback unavailable.'
    return (
      <div className="mx-auto max-w-5xl px-4 py-16">
        <p className="text-destructive">{message}</p>
        <p className="mt-2 text-sm text-muted-foreground">
          If you just signed in, go back to the movie and try Watch now again.
        </p>
        <Link to={`/movies/${slug}`} className="mt-4 inline-block">
          <Button variant="outline">Back to movie</Button>
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-0 py-4 sm:px-6 sm:py-8">
      <div className="px-3 sm:px-0">
        <Link
          to={`/movies/${slug}`}
          className="inline-flex min-h-11 items-center text-sm text-muted-foreground hover:text-foreground"
        >
          ← Back
        </Link>
      </div>
      <div className="mt-2 overflow-hidden border-y border-border bg-black shadow-xl sm:mt-4 sm:rounded-2xl sm:border">
        {isVimeo ? (
          <VimeoPlayer
            videoId={playback!.vimeoVideoId!}
            className="aspect-video w-full min-h-[min(56.25vw,70dvh)] bg-black"
          />
        ) : (
          <video ref={videoRef} className="aspect-video w-full max-h-[70dvh]" controls playsInline />
        )}
      </div>
    </div>
  )
}
