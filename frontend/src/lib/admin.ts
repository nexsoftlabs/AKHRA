import { z } from 'zod'
import { apiFetch } from '@/lib/api'

const adminMovieListSchema = z.object({
  id: z.string().uuid(),
  title: z.string(),
  slug: z.string(),
  publicationStatus: z.string(),
  priceMinorUnits: z.number(),
  currency: z.string(),
  updatedAt: z.string(),
})

const licenseSchema = z.object({
  rightsHolder: z.string(),
  licenseReference: z.string(),
  validFrom: z.string(),
  validTo: z.string(),
  territories: z.array(z.string()),
  allowsStreaming: z.boolean(),
  notes: z.string().nullable().optional(),
})

export const adminMovieDetailSchema = z.object({
  id: z.string().uuid(),
  title: z.string(),
  slug: z.string(),
  description: z.string().nullable().optional(),
  synopsis: z.string().nullable().optional(),
  language: z.string().nullable().optional(),
  releaseYear: z.number().nullable().optional(),
  durationSeconds: z.number(),
  ageRating: z.string().nullable().optional(),
  posterUrl: z.string().nullable().optional(),
  backdropUrl: z.string().nullable().optional(),
  trailerUrl: z.string().nullable().optional(),
  priceMinorUnits: z.number(),
  currency: z.string(),
  purchaseType: z.string(),
  subscriptionEligible: z.boolean(),
  publicationStatus: z.string(),
  availabilityStart: z.string().nullable().optional(),
  availabilityEnd: z.string().nullable().optional(),
  licenseTerritories: z.array(z.string()),
  isFeatured: z.boolean(),
  genres: z.array(z.string()),
  license: licenseSchema.nullable().optional(),
})

export type AdminMovieDetail = z.infer<typeof adminMovieDetailSchema>

export async function fetchAdminMovies(q?: string) {
  const params = q ? `?q=${encodeURIComponent(q)}` : ''
  const data = await apiFetch<unknown[]>(`/api/v1/admin/movies${params}`)
  return z.array(adminMovieListSchema).parse(data)
}

export async function fetchAdminMovie(id: string) {
  const data = await apiFetch<unknown>(`/api/v1/admin/movies/${id}`)
  return adminMovieDetailSchema.parse(data)
}

export async function publishMovie(id: string) {
  return apiFetch<{ status: string }>(`/api/v1/admin/movies/${id}/publish`, { method: 'POST' })
}

export async function createMovie(body: Record<string, unknown>) {
  const data = await apiFetch<unknown>('/api/v1/admin/movies', {
    method: 'POST',
    body: JSON.stringify(body),
  })
  return adminMovieDetailSchema.parse(data)
}

export async function updateAdminMovie(id: string, body: Record<string, unknown>) {
  const data = await apiFetch<unknown>(`/api/v1/admin/movies/${id}`, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
  return adminMovieDetailSchema.parse(data)
}
