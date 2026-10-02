import type { CheckoutSession } from '@/lib/checkout'
import { verifyCheckout } from '@/lib/checkout'

type RazorpayHandlerResponse = {
  razorpay_payment_id: string
  razorpay_order_id: string
  razorpay_signature: string
}

type RazorpaySubscriptionHandlerResponse = {
  razorpay_payment_id: string
  razorpay_subscription_id: string
  razorpay_signature: string
}

type RazorpayOptions = {
  key: string
  amount?: number
  currency?: string
  name: string
  description: string
  order_id?: string
  subscription_id?: string
  handler: (response: RazorpayHandlerResponse | RazorpaySubscriptionHandlerResponse) => void
  prefill?: { name?: string; email?: string }
  theme?: { color?: string }
  modal?: { ondismiss?: () => void }
}

type RazorpayInstance = {
  open: () => void
  on: (event: 'payment.failed', handler: (response: { error: { description: string } }) => void) => void
}

declare global {
  interface Window {
    Razorpay?: new (options: RazorpayOptions) => RazorpayInstance
  }
}

const SCRIPT_URL = 'https://checkout.razorpay.com/v1/checkout.js'

let scriptPromise: Promise<void> | null = null

function loadRazorpayScript() {
  if (window.Razorpay) {
    return Promise.resolve()
  }
  if (!scriptPromise) {
    scriptPromise = new Promise((resolve, reject) => {
      const existing = document.querySelector(`script[src="${SCRIPT_URL}"]`)
      if (existing) {
        existing.addEventListener('load', () => resolve())
        existing.addEventListener('error', () => reject(new Error('Failed to load Razorpay.')))
        return
      }
      const script = document.createElement('script')
      script.src = SCRIPT_URL
      script.async = true
      script.onload = () => resolve()
      script.onerror = () => reject(new Error('Failed to load Razorpay checkout.'))
      document.body.appendChild(script)
    })
  }
  return scriptPromise
}

export async function openRazorpayCheckout(
  session: CheckoutSession,
  prefill?: { name?: string; email?: string },
): Promise<{ success: boolean; message?: string }> {
  await loadRazorpayScript()
  if (!window.Razorpay) {
    throw new Error('Razorpay is unavailable.')
  }

  const subscriptionId = session.razorpaySubscriptionId?.trim()
  const orderId = session.razorpayOrderId?.trim()
  const isSubscription = Boolean(subscriptionId)

  if (!isSubscription && !orderId) {
    throw new Error('Checkout could not be started. Try again or contact support.')
  }

  if (!session.razorpayKeyId?.trim()) {
    throw new Error('Payments are not configured on the server.')
  }

  return new Promise((resolve, reject) => {
    const rzp = new window.Razorpay!({
      key: session.razorpayKeyId,
      ...(isSubscription
        ? { subscription_id: subscriptionId! }
        : {
            amount: session.amountMinorUnits,
            currency: session.currency,
            order_id: orderId!,
          }),
      name: 'AKHRA',
      description: session.movieTitle,
      prefill,
      theme: { color: '#7c3aed' },
      handler: async (response) => {
        try {
          const subResponse = response as RazorpaySubscriptionHandlerResponse
          const orderResponse = response as RazorpayHandlerResponse
          const result = await verifyCheckout(
            isSubscription
              ? {
                  razorpaySubscriptionId: subResponse.razorpay_subscription_id,
                  razorpayPaymentId: subResponse.razorpay_payment_id,
                  razorpaySignature: subResponse.razorpay_signature,
                }
              : {
                  razorpayOrderId: orderResponse.razorpay_order_id,
                  razorpayPaymentId: orderResponse.razorpay_payment_id,
                  razorpaySignature: orderResponse.razorpay_signature,
                },
          )
          resolve({
            success: result.success,
            message: result.message ?? 'Payment complete.',
          })
        } catch (err) {
          reject(err)
        }
      },
      modal: {
        ondismiss: () => reject(new Error('Payment cancelled.')),
      },
    })

    rzp.on('payment.failed', (response) => {
      reject(new Error(response.error?.description ?? 'Payment failed.'))
    })

    rzp.open()
  })
}
