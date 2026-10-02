import { z } from 'zod'
import { apiFetch } from '@/lib/api'

export const checkoutSessionSchema = z.object({
  paymentId: z.string().uuid(),
  razorpayOrderId: z.string(),
  razorpayKeyId: z.string(),
  amountMinorUnits: z.number(),
  currency: z.string(),
  movieTitle: z.string(),
  razorpaySubscriptionId: z.string().nullable().optional(),
})

const verifyResponseSchema = z.object({
  success: z.boolean(),
  message: z.string().nullable().optional(),
  movieId: z.string().uuid().nullable().optional(),
  alreadyOwned: z.boolean(),
})

export type CheckoutSession = z.infer<typeof checkoutSessionSchema>

export function createMovieCheckout(movieSlug: string) {
  return apiFetch<unknown>(`/api/v1/checkout/movies/${encodeURIComponent(movieSlug)}`, {
    method: 'POST',
  }).then((data) => checkoutSessionSchema.parse(data))
}

export function verifyCheckout(input: {
  razorpayOrderId?: string
  razorpayPaymentId: string
  razorpaySignature: string
  razorpaySubscriptionId?: string
}) {
  return apiFetch<unknown>('/api/v1/checkout/verify', {
    method: 'POST',
    body: JSON.stringify(input),
  }).then((data) => verifyResponseSchema.parse(data))
}
