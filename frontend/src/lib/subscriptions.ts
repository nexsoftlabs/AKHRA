import { z } from 'zod'
import { apiFetch } from '@/lib/api'
import { checkoutSessionSchema } from '@/lib/checkout'

const planSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  slug: z.string(),
  description: z.string().nullable().optional(),
  priceMinorUnits: z.number(),
  currency: z.string(),
  billingInterval: z.string(),
  billingPeriodMonths: z.number(),
  billingLabel: z.string(),
})

export async function fetchPlans() {
  const data = await apiFetch<unknown[]>('/api/v1/subscriptions/plans')
  return z.array(planSchema).parse(data)
}

export async function createPlanCheckout(planSlug: string) {
  const data = await apiFetch<unknown>(`/api/v1/subscriptions/plans/${encodeURIComponent(planSlug)}/checkout`, {
    method: 'POST',
  })
  return checkoutSessionSchema.parse(data)
}
