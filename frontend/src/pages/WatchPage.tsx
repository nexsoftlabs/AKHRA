import { useQuery } from '@tanstack/react-query'
import Hls from 'hls.js'
import { useEffect, useRef } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { startPlayback } from '@/lib/playback'

export function WatchPage() {
  const { slug } = useParams<{ slug: string }>()
  const videoRef = useRef<HTMLVideoElement>(null)

  const playbackQuery = useQuery({
    queryKey: ['playback', slug],
    queryFn: () => startPlayback(slug!),
    enabled: Boolean(slug),
    retry: false,
  })

  useEffect(() => {
    const video = videoRef.current
    const manifestUrl = playbackQuery.data?.manifestUrl
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
  }, [playbackQuery.data?.manifestUrl])

  if (playbackQuery.isLoading) {
    return <p className="mx-auto max-w-5xl px-4 py-16 text-muted-foreground">Preparing playback…</p>
  }

  if (playbackQuery.isError) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16">
        <p className="text-destructive">Playback unavailable. Purchase this title or sign in.</p>
        <Link to={`/movies/${slug}`} className="mt-4 inline-block">
          <Button variant="outline">Back to movie</Button>
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
      <Link to={`/movies/${slug}`} className="text-sm text-muted-foreground hover:text-foreground">← Back</Link>
      <div className="mt-4 overflow-hidden rounded-2xl border border-border bg-black shadow-xl">
        <video ref={videoRef} className="aspect-video w-full" controls playsInline />
      </div>
    </div>
  )
}
