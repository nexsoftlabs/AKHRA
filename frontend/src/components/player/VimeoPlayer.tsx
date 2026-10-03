import Player from '@vimeo/player'
import { useEffect, useId, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { vimeoEmbedUrl } from '@/lib/vimeo'

type VimeoPlayerProps = {
  videoId: string
  title?: string
  className?: string
  /** When set, playback pauses at this time (e.g. 60s guest preview). */
  maxDurationSeconds?: number
  onPreviewLimitReached?: () => void
  /** Shown when preview cap is hit (default: sign-in prompt). */
  limitReachedMessage?: string
  limitReachedActions?: React.ReactNode
}

/** In-app Vimeo embed (no redirect to vimeo.com). */
export function VimeoPlayer({
  videoId,
  title,
  className,
  maxDurationSeconds,
  onPreviewLimitReached,
  limitReachedMessage,
  limitReachedActions,
}: VimeoPlayerProps) {
  const iframeRef = useRef<HTMLIFrameElement>(null)
  const [limitReached, setLimitReached] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)
  const iframeTitle = title ?? 'Video'
  const titleId = useId()

  const usesPlayerApi = maxDurationSeconds != null && maxDurationSeconds > 0
  const src = vimeoEmbedUrl(videoId, {
    playerApi: usesPlayerApi,
    autoplay: usesPlayerApi,
  })

  useEffect(() => {
    setLimitReached(false)
    setLoadError(null)

    if (!usesPlayerApi) {
      return
    }

    const iframe = iframeRef.current
    if (!iframe) {
      return
    }

    let player: Player | undefined
    let stopped = false

    const onTimeUpdate = (data: { seconds: number }) => {
      if (stopped || maxDurationSeconds == null || data.seconds < maxDurationSeconds) {
        return
      }
      stopped = true
      void player?.pause().then(() => {
        setLimitReached(true)
        onPreviewLimitReached?.()
      })
    }

    const onError = () => {
      setLoadError(
        'This preview could not be loaded. In Vimeo, allow embedding for this video and add http://localhost:5173 to allowed domains.',
      )
    }

    const attach = async () => {
      try {
        player = new Player(iframe)
        await player.ready()
        if (stopped) {
          return
        }
        player.on('timeupdate', onTimeUpdate)
        player.on('error', onError)
      } catch {
        onError()
      }
    }

    void attach()

    return () => {
      stopped = true
      if (player) {
        player.off('timeupdate', onTimeUpdate)
        player.off('error', onError)
      }
    }
  }, [videoId, maxDurationSeconds, usesPlayerApi, onPreviewLimitReached])

  return (
    <div className={`relative ${className ?? 'aspect-video w-full min-h-[200px] bg-black'}`}>
      <iframe
        key={`${videoId}-${usesPlayerApi ? 'preview' : 'full'}`}
        ref={iframeRef}
        src={src}
        title={iframeTitle}
        className="absolute inset-0 h-full w-full border-0"
        allow="autoplay; fullscreen; picture-in-picture; encrypted-media"
        allowFullScreen
        referrerPolicy="strict-origin-when-cross-origin"
      />
      {loadError && (
        <div
          className="absolute inset-0 flex items-center justify-center bg-black/90 p-4 text-center text-sm text-muted-foreground"
          role="alert"
        >
          {loadError}
        </div>
      )}
      {limitReached && maxDurationSeconds != null && (
        <div
          className="absolute inset-0 z-10 flex flex-col items-center justify-center gap-3 bg-black/85 px-4 text-center"
          role="status"
          aria-labelledby={titleId}
        >
          <p id={titleId} className="text-sm text-foreground sm:text-base">
            {limitReachedMessage ??
              `Preview ended (${maxDurationSeconds}s). Sign in to watch the full title.`}
          </p>
          <div className="flex flex-wrap justify-center gap-2">
            {limitReachedActions ?? (
              <>
                <Link to="/login">
                  <Button size="sm">Sign in</Button>
                </Link>
                <Link to="/register">
                  <Button size="sm" variant="outline">Create account</Button>
                </Link>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
