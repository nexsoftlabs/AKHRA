import { z } from 'zod'
import { apiFetch } from '@/lib/api'

const paymentSchema = z.object({
  paymentId: z.string().uuid(),
  orderId: z.string().uuid(),
  customerEmail: z.string().nullable().optional(),
  amountMinorUnits: z.number(),
  currency: z.string(),
  status: z.string(),
  razorpayPaymentId: z.string().nullable().optional(),
  updatedAt: z.string(),
})

const refundHistorySchema = z.object({
  refundId: z.string().uuid(),
  paymentId: z.string().uuid(),
  customerEmail: z.string().nullable().optional(),
  amountMinorUnits: z.number(),
  currency: z.string(),
  status: z.string(),
  reason: z.string().nullable().optional(),
  createdAt: z.string(),
})

const refundSchema = z.object({
  id: z.string().uuid(),
  paymentId: z.string().uuid(),
  status: z.string(),
  amountMinorUnits: z.number(),
  currency: z.string(),
  providerRefundId: z.string().nullable().optional(),
})

export async function fetchRefundablePayments(search?: string) {
  const params = search ? `?q=${encodeURIComponent(search)}` : ''
  const data = await apiFetch<unknown[]>(`/api/v1/admin/finance/payments${params}`)
  return z.array(paymentSchema).parse(data)
}

export async function fetchRefundHistory(search?: string) {
  const params = search ? `?q=${encodeURIComponent(search)}` : ''
  const data = await apiFetch<unknown[]>(`/api/v1/admin/finance/refunds${params}`)
  return z.array(refundHistorySchema).parse(data)
}

export function exportPaymentsCsv(search?: string) {
  const params = search ? `?q=${encodeURIComponent(search)}` : ''
  window.open(`/api/v1/admin/finance/payments/export${params}`, '_blank')
}

export async function requestRefund(paymentId: string, reason?: string, amountMinorUnits?: number) {
  const data = await apiFetch<unknown>('/api/v1/admin/refunds', {
    method: 'POST',
    body: JSON.stringify({ paymentId, reason, amountMinorUnits }),
  })
  return refundSchema.parse(data)
}
