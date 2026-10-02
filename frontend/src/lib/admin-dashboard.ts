import { z } from 'zod'
import { apiFetch } from '@/lib/api'

const recentPaymentSchema = z.object({
  paymentId: z.string().uuid(),
  customerEmail: z.string().nullable().optional(),
  amountMinorUnits: z.number(),
  currency: z.string(),
  status: z.string(),
  updatedAt: z.string(),
})

const statsSchema = z.object({
  totalUsers: z.number(),
  publishedMovies: z.number(),
  draftMovies: z.number(),
  activeSubscriptions: z.number(),
  revenueCapturedMinorUnits30d: z.number(),
  paymentsCaptured30d: z.number(),
  refunds30d: z.number(),
  recentPayments: z.array(recentPaymentSchema),
})

export type AdminDashboardStats = z.infer<typeof statsSchema>

export function fetchAdminDashboardStats() {
  return apiFetch<unknown>('/api/v1/admin/dashboard/stats').then((data) => statsSchema.parse(data))
}
