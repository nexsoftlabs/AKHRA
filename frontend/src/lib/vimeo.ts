/** Extract Vimeo numeric id from vimeo.com URLs or return id if already numeric. */
export function parseVimeoVideoId(urlOrId: string | null | undefined): string | null {
  if (!urlOrId?.trim()) return null
  const trimmed = urlOrId.trim()
  if (/^\d+$/.test(trimmed)) return trimmed
  try {
    const url = new URL(trimmed)
    if (!url.hostname.includes('vimeo.com')) return null
    const parts = url.pathname.split('/').filter(Boolean)
    const id = parts.find((p) => /^\d+$/.test(p))
    return id ?? null
  } catch {
    return null
  }
}

export function vimeoEmbedUrl(videoId: string, options?: { playerApi?: boolean; autoplay?: boolean }) {
  const params = new URLSearchParams({
    title: '0',
    byline: '0',
    portrait: '0',
    dnt: '1',
  })
  if (options?.playerApi) {
    params.set('api', '1')
  }
  if (options?.autoplay) {
    params.set('autoplay', '1')
  }
  return `https://player.vimeo.com/video/${videoId}?${params.toString()}`
}
