import { z } from 'zod'
import { apiFetch } from '@/lib/api'

const playbackStartSchema = z.object({
  sessionToken: z.string(),
  expiresAt: z.union([z.string(), z.number()]).transform(String),
  playbackType: z.enum(['hls', 'vimeo']),
  manifestUrl: z.string().nullable().optional(),
  vimeoVideoId: z.string().nullable().optional(),
})

const libraryItemSchema = z.object({
  movieId: z.string().uuid(),
  title: z.string(),
  slug: z.string(),
  posterUrl: z.string().nullable().optional(),
  durationSeconds: z.number(),
  purchasedAt: z.string().nullable().optional(),
})

export type LibraryItem = z.infer<typeof libraryItemSchema>

export async function startPlayback(movieSlug: string) {
  const data = await apiFetch<unknown>(`/api/v1/playback/movies/${encodeURIComponent(movieSlug)}/start`, {
    method: 'POST',
  })
  return playbackStartSchema.parse(data)
}

export async function fetchLibrary() {
  const data = await apiFetch<unknown[]>('/api/v1/playback/library')
  return z.array(libraryItemSchema).parse(data)
}
