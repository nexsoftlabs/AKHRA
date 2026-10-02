import { z } from 'zod'

const movieListItemSchema = z.object({
  id: z.string().uuid(),
  title: z.string(),
  slug: z.string(),
  posterUrl: z.string().nullable().optional(),
  releaseYear: z.number().nullable().optional(),
  durationSeconds: z.number(),
  ageRating: z.string().nullable().optional(),
  priceMinorUnits: z.number(),
  currency: z.string(),
  isFeatured: z.boolean(),
  genres: z.array(z.string()),
})

export const movieDetailSchema = movieListItemSchema.extend({
  description: z.string().nullable().optional(),
  synopsis: z.string().nullable().optional(),
  language: z.string().nullable().optional(),
  backdropUrl: z.string().nullable().optional(),
  trailerUrl: z.string().nullable().optional(),
  purchaseType: z.string(),
  subscriptionEligible: z.boolean(),
  hasAccess: z.boolean(),
})

export type MovieListItem = z.infer<typeof movieListItemSchema>
export type MovieDetail = z.infer<typeof movieDetailSchema>

import { apiFetch } from '@/lib/api'

export function formatPrice(minorUnits: number, currency: string) {
  if (currency === 'INR') {
    return `₹${(minorUnits / 100).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`
  }
  return `${currency} ${(minorUnits / 100).toFixed(2)}`
}

export function formatDuration(seconds: number) {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  if (h > 0) return `${h}h ${m}m`
  return `${m} min`
}

export async function fetchMovies(search?: string, genre?: string) {
  const params = new URLSearchParams()
  if (search) params.set('q', search)
  if (genre) params.set('genre', genre)
  const qs = params.toString()
  const data = await apiFetch<unknown[]>(`/api/v1/movies${qs ? `?${qs}` : ''}`)
  return z.array(movieListItemSchema).parse(data)
}

export async function fetchMovieBySlug(slug: string) {
  const data = await apiFetch<unknown>(`/api/v1/movies/${slug}`)
  return movieDetailSchema.parse(data)
}

export async function fetchCatalogGenres() {
  const data = await apiFetch<unknown[]>('/api/v1/movies/genres')
  return z.array(z.string()).parse(data)
}
