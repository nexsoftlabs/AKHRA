import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Check, Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { createPlanCheckout, fetchPlans } from '@/lib/subscriptions'
import { openRazorpayCheckout } from '@/lib/razorpay'
import { getMe } from '@/lib/auth'
import { cn } from '@/lib/utils'

function formatInr(minor: number) {
  return `₹${(minor / 100).toLocaleString('en-IN')}`
}

const selectedPlanStyles =
  'border-primary bg-primary/5 shadow-lg shadow-primary/10 ring-2 ring-primary/40 scale-[1.02]'

export function PlansPage() {
  const queryClient = useQueryClient()
  const plansQuery = useQuery({ queryKey: ['plans'], queryFn: fetchPlans })
  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, retry: false })
  const [selectedSlug, setSelectedSlug] = useState<string | null>(null)
  const [checkoutSlug, setCheckoutSlug] = useState<string | null>(null)
  const [checkoutError, setCheckoutError] = useState<string | null>(null)
  const [checkoutSuccess, setCheckoutSuccess] = useState<string | null>(null)

  const plans = plansQuery.data ?? []

  useEffect(() => {
    if (plans.length === 0 || selectedSlug) return
    const defaultPlan = plans.find((p) => p.slug === '6-months') ?? plans[0]
    setSelectedSlug(defaultPlan.slug)
  }, [plans, selectedSlug])

  async function handleSubscribe(slug: string) {
    setSelectedSlug(slug)
    setCheckoutError(null)
    setCheckoutSuccess(null)

    if (!meQuery.data) {
      return
    }

    setCheckoutSlug(slug)
    try {
      const session = await createPlanCheckout(slug)
      const result = await openRazorpayCheckout(session, {
        email: meQuery.data.email ?? undefined,
        name: meQuery.data.displayName ?? undefined,
      })
      if (result.success) {
        setCheckoutSuccess(result.message ?? 'Subscription active. Enjoy streaming!')
        await queryClient.invalidateQueries({ queryKey: ['library'] })
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Checkout failed.'
      if (message === 'email_not_confirmed' || message.includes('email')) {
        setCheckoutError('Confirm your email from Account before subscribing.')
      } else if (message === 'razorpay_not_configured' || message.includes('not configured')) {
        setCheckoutError('Payments are not configured. Add Razorpay keys to the API (.env).')
      } else if (message !== 'Payment cancelled.') {
        setCheckoutError(message)
      }
    } finally {
      setCheckoutSlug(null)
    }
  }

  const monthly = plans.find((p) => p.slug === 'monthly')
  const monthlyPaise = monthly?.priceMinorUnits ?? 19900
  const isCheckoutPending = checkoutSlug !== null

  return (
    <div className="mx-auto max-w-6xl px-4 py-12 sm:px-6 sm:py-16">
      <div className="max-w-2xl">
        <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">Choose your plan</h1>
        <p className="mt-3 text-muted-foreground">
          Unlimited streaming on every subscription-eligible title. Select a plan, then subscribe.
        </p>
      </div>

      {checkoutError && (
        <p className="mt-6 rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          {checkoutError}
        </p>
      )}
      {checkoutSuccess && (
        <p className="mt-6 rounded-xl border border-emerald-500/30 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-600 dark:text-emerald-400">
          {checkoutSuccess}{' '}
          <Link to="/library" className="font-medium underline">Go to library</Link>
        </p>
      )}

      <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {plansQuery.isLoading &&
          Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="h-72 animate-pulse rounded-2xl bg-muted" />
          ))}

        {plans.map((plan) => {
          const perMonth = Math.round(plan.priceMinorUnits / plan.billingPeriodMonths)
          const savings =
            plan.slug !== 'monthly' && monthlyPaise > 0
              ? Math.max(0, Math.round((1 - perMonth / monthlyPaise) * 100))
              : 0
          const isSelected = selectedSlug === plan.slug
          const isPopular = plan.slug === '6-months'
          const isThisCheckout = checkoutSlug === plan.slug

          return (
            <article
              key={plan.id}
              onClick={() => setSelectedSlug(plan.slug)}
              className={cn(
                'relative flex cursor-pointer flex-col rounded-2xl border p-6 transition-all duration-200',
                isSelected ? selectedPlanStyles : 'border-border bg-card/60 hover:border-primary/25 hover:bg-card',
              )}
            >
              {isSelected && (
                <span className="absolute -top-3 left-1/2 -translate-x-1/2 rounded-full bg-primary px-3 py-0.5 text-xs font-semibold text-primary-foreground shadow-md">
                  Selected
                </span>
              )}
              {!isSelected && isPopular && (
                <span className="absolute -top-3 left-1/2 -translate-x-1/2 rounded-full border border-primary/40 bg-background px-3 py-0.5 text-xs font-medium text-primary">
                  Popular
                </span>
              )}
              {savings > 0 && (
                <span className="text-xs font-medium text-emerald-500">Save ~{savings}% vs monthly</span>
              )}
              <h2 className="mt-1 text-lg font-semibold">{plan.name}</h2>
              <p className="mt-2 min-h-[2.5rem] text-sm text-muted-foreground">{plan.description}</p>
              <p className="mt-6">
                <span className="text-3xl font-bold tracking-tight">{formatInr(plan.priceMinorUnits)}</span>
                <span className="text-sm text-muted-foreground"> / {plan.billingLabel}</span>
              </p>
              {plan.billingPeriodMonths > 1 && (
                <p className="mt-1 text-xs text-muted-foreground">
                  {formatInr(perMonth)} effective per month
                </p>
              )}
              <ul className="mt-6 flex-1 space-y-2 text-sm text-muted-foreground">
                <li className="flex items-center gap-2">
                  <Check className={cn('size-4 shrink-0', isSelected ? 'text-primary' : 'text-muted-foreground')} />
                  All-access catalog
                </li>
                <li className="flex items-center gap-2">
                  <Check className={cn('size-4 shrink-0', isSelected ? 'text-primary' : 'text-muted-foreground')} />
                  HD streaming
                </li>
                <li className="flex items-center gap-2">
                  <Check className={cn('size-4 shrink-0', isSelected ? 'text-primary' : 'text-muted-foreground')} />
                  Cancel anytime
                </li>
              </ul>
              <div className="mt-6" onClick={(e) => e.stopPropagation()}>
                {meQuery.data ? (
                  <Button
                    type="button"
                    className="w-full"
                    variant={isSelected ? 'default' : 'outline'}
                    disabled={isCheckoutPending}
                    onClick={() => handleSubscribe(plan.slug)}
                  >
                    {isThisCheckout ? (
                      <>
                        <Loader2 className="size-4 animate-spin" />
                        Opening checkout…
                      </>
                    ) : isSelected ? (
                      'Subscribe'
                    ) : (
                      'Choose plan'
                    )}
                  </Button>
                ) : (
                  <Link to="/login">
                    <Button type="button" className="w-full" variant={isSelected ? 'default' : 'outline'}>
                      Sign in to subscribe
                    </Button>
                  </Link>
                )}
              </div>
            </article>
          )
        })}
      </div>
    </div>
  )
}
