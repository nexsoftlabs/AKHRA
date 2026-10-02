import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  exportPaymentsCsv,
  fetchRefundablePayments,
  fetchRefundHistory,
  requestRefund,
} from '@/lib/finance'

function formatInr(minor: number) {
  return `₹${(minor / 100).toLocaleString('en-IN')}`
}

export function AdminFinancePage() {
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [reason, setReason] = useState('Customer request')
  const [message, setMessage] = useState<string | null>(null)

  const paymentsQuery = useQuery({
    queryKey: ['finance-payments', search],
    queryFn: () => fetchRefundablePayments(search || undefined),
  })
  const refundsQuery = useQuery({
    queryKey: ['finance-refunds', search],
    queryFn: () => fetchRefundHistory(search || undefined),
  })

  const refundMutation = useMutation({
    mutationFn: ({ paymentId, amountMinorUnits }: { paymentId: string; amountMinorUnits?: number }) =>
      requestRefund(paymentId, reason, amountMinorUnits),
    onSuccess: () => {
      setMessage('Refund submitted to Razorpay.')
      queryClient.invalidateQueries({ queryKey: ['finance-payments'] })
      queryClient.invalidateQueries({ queryKey: ['finance-refunds'] })
    },
    onError: (err) => setMessage(err instanceof Error ? err.message : 'Refund failed.'),
  })

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
      <h1 className="text-3xl font-semibold">Finance</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Search payments, export CSV, issue full or partial refunds. Requires FinanceAdmin and MFA step-up in production.
      </p>

      <div className="mt-6 flex flex-wrap items-end gap-3">
        <div className="min-w-[200px] flex-1 max-w-md">
          <label className="text-sm font-medium">Search (Razorpay payment id)</label>
          <Input className="mt-1" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="pay_…" />
        </div>
        <Button type="button" variant="outline" onClick={() => exportPaymentsCsv(search || undefined)}>
          Export CSV
        </Button>
      </div>

      <div className="mt-6 max-w-md">
        <label className="text-sm font-medium">Default refund reason</label>
        <Input className="mt-1" value={reason} onChange={(e) => setReason(e.target.value)} />
      </div>

      {message && <p className="mt-4 text-sm text-primary">{message}</p>}

      <h2 className="mt-10 text-lg font-semibold">Captured payments</h2>
      <div className="mt-4 overflow-x-auto rounded-2xl border border-border">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-border bg-muted/40">
            <tr>
              <th className="px-4 py-3">Customer</th>
              <th className="px-4 py-3">Amount</th>
              <th className="px-4 py-3">Razorpay payment</th>
              <th className="px-4 py-3">Action</th>
            </tr>
          </thead>
          <tbody>
            {paymentsQuery.isLoading && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-muted-foreground">Loading…</td>
              </tr>
            )}
            {paymentsQuery.data?.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-muted-foreground">No captured payments to refund.</td>
              </tr>
            )}
            {paymentsQuery.data?.map((p) => (
              <tr key={p.paymentId} className="border-b border-border/60">
                <td className="px-4 py-3">
                  <p>{p.customerEmail ?? '—'}</p>
                  <p className="text-xs text-muted-foreground">{p.paymentId}</p>
                </td>
                <td className="px-4 py-3">{formatInr(p.amountMinorUnits)}</td>
                <td className="px-4 py-3 font-mono text-xs">{p.razorpayPaymentId ?? '—'}</td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap gap-2">
                    <Button
                      size="sm"
                      variant="outline"
                      disabled={refundMutation.isPending}
                      onClick={() => {
                        if (window.confirm(`Refund ${formatInr(p.amountMinorUnits)} to ${p.customerEmail}?`)) {
                          refundMutation.mutate({ paymentId: p.paymentId })
                        }
                      }}
                    >
                      Full refund
                    </Button>
                    <Button
                      size="sm"
                      variant="ghost"
                      disabled={refundMutation.isPending}
                      onClick={() => {
                        const raw = window.prompt('Partial refund amount in paise:', String(p.amountMinorUnits / 2))
                        if (!raw) return
                        const amount = Number(raw)
                        if (!Number.isFinite(amount) || amount <= 0) return
                        refundMutation.mutate({ paymentId: p.paymentId, amountMinorUnits: amount })
                      }}
                    >
                      Partial
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <h2 className="mt-10 text-lg font-semibold">Refund history</h2>
      <div className="mt-4 overflow-x-auto rounded-2xl border border-border">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-border bg-muted/40">
            <tr>
              <th className="px-4 py-3">When</th>
              <th className="px-4 py-3">Customer</th>
              <th className="px-4 py-3">Amount</th>
              <th className="px-4 py-3">Reason</th>
            </tr>
          </thead>
          <tbody>
            {refundsQuery.data?.map((r) => (
              <tr key={r.refundId} className="border-b border-border/60">
                <td className="px-4 py-3 text-xs">{new Date(r.createdAt).toLocaleString()}</td>
                <td className="px-4 py-3">{r.customerEmail ?? '—'}</td>
                <td className="px-4 py-3">{formatInr(r.amountMinorUnits)}</td>
                <td className="px-4 py-3 text-muted-foreground">{r.reason ?? r.status}</td>
              </tr>
            ))}
            {refundsQuery.data?.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-muted-foreground">No refunds yet.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
