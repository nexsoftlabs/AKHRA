import { LegalPageLayout } from '@/components/legal/LegalPageLayout'

const LAST_UPDATED = 'September 28, 2026'

export function TermsOfServicePage() {
  return (
    <LegalPageLayout
      title="Terms of Service"
      lastUpdated={LAST_UPDATED}
      summary="These Terms of Service (“Terms”) govern your access to and use of AKHRA websites, applications, and streaming services. By creating an account, making a purchase, or using the Service, you agree to these Terms."
    >
      <section id="agreement">
        <h2>1. Agreement to terms</h2>
        <p>
          AKHRA (“AKHRA”, “we”, “us”) provides a licensed digital video service. These Terms form a binding
          agreement between you and AKHRA. If you do not agree, do not use the Service. If you use the Service on
          behalf of an organization, you represent that you have authority to bind that organization.
        </p>
      </section>

      <section id="service">
        <h2>2. The service</h2>
        <p>
          The Service allows you to browse a catalog of motion pictures and related content (“Content”), preview
          trailers where offered, purchase or rent access where available, and stream Content you are entitled to view.
          Availability, pricing, format (SD/HD), languages, subtitles, and features vary by title and territory. We may
          change, suspend, or discontinue any part of the Service or any Content at any time, including for licensing,
          technical, or legal reasons.
        </p>
      </section>

      <section id="eligibility">
        <h2>3. Eligibility and accounts</h2>
        <ul>
          <li>You must be at least 18 years old (or the age of majority in your jurisdiction) to create an account and purchase Content, unless a parent or guardian creates a supervised profile as permitted by the Service.</li>
          <li>You must provide accurate registration information and keep it current.</li>
          <li>You are responsible for all activity under your account and for maintaining the confidentiality of your credentials.</li>
          <li>One household may use the Service on a limited number of simultaneous streams and devices as described in your plan or product page; sharing credentials outside your household may violate these Terms.</li>
          <li>We may refuse registration, suspend, or terminate accounts for violation of these Terms, suspected fraud, or abuse.</li>
        </ul>
      </section>

      <section id="purchases">
        <h2>4. Purchases, rentals, and subscriptions</h2>
        <h3>4.1 Pricing and payment</h3>
        <p>
          Prices are shown at checkout in the applicable currency (e.g. INR) and include applicable taxes where required.
          You authorize us and our payment partners (such as Razorpay) to charge your selected payment method for all
          fees. Failed or reversed payments may result in loss of access until resolved.
        </p>
        <h3>4.2 Entitlements</h3>
        <p>
          Upon successful payment verification, we grant a limited right to stream the purchased or rented Content for
          the period stated at purchase (e.g. lifetime purchase for a title, rental window, or subscription period).
          Entitlements are personal, non-transferable, and tied to your account unless we expressly allow gifting.
        </p>
        <h3>4.3 Subscriptions</h3>
        <p>
          If we offer subscription plans, they renew automatically at the then-current rate unless you cancel before the
          renewal date. Cancellation stops future charges; you retain access through the end of the paid period. Free
          trials convert to paid plans unless cancelled before the trial ends.
        </p>
        <h3>4.4 Refunds</h3>
        <p>
          Except where required by law or stated in a specific offer, all sales are final once streaming access is
          granted. Refund requests for technical failures preventing playback may be considered case-by-case. Chargebacks
          without contacting support may lead to account suspension.
        </p>
      </section>

      <section id="license">
        <h2>5. License to Content</h2>
        <p>
          Content is licensed, not sold. Subject to your compliance with these Terms and payment of applicable fees, we
          grant you a limited, non-exclusive, non-transferable, revocable license to access and view Content for
          personal, non-commercial, private use only, in supported territories and on supported devices. You may not:
        </p>
        <ul>
          <li>Copy, download (except where offline viewing is expressly permitted), redistribute, broadcast, or publicly perform Content.</li>
          <li>Circumvent geographic, DRM, or other technical protection measures.</li>
          <li>Use automated means to scrape, index, or extract Content or metadata.</li>
          <li>Remove copyright, trademark, or other proprietary notices.</li>
          <li>Use the Service or Content for any commercial exhibition or derivative works without separate written permission.</li>
        </ul>
      </section>

      <section id="conduct">
        <h2>6. Acceptable use</h2>
        <p>You agree not to:</p>
        <ul>
          <li>Violate any law or third-party rights, including intellectual property and privacy rights.</li>
          <li>Upload malware, interfere with the Service, or attempt unauthorized access to systems or accounts.</li>
          <li>Harass, impersonate, or engage in fraudulent activity.</li>
          <li>Share account credentials beyond permitted household use or resell access.</li>
        </ul>
        <p>
          We may investigate violations and cooperate with rights holders and law enforcement regarding piracy or
          circumvention.
        </p>
      </section>

      <section id="ip">
        <h2>7. Intellectual property</h2>
        <p>
          The Service, including software, design, logos, and compilation of Content, is owned by AKHRA or its
          licensors and protected by copyright, trademark, and other laws. Content is owned by studios, distributors, and
          other licensors. Nothing in these Terms grants you ownership of any intellectual property.
        </p>
      </section>

      <section id="third-party">
        <h2>8. Third-party services and links</h2>
        <p>
          Payment, authentication, analytics, and device integrations may be provided by third parties. Your use of those
          services may be subject to separate terms. We are not responsible for third-party sites or services.
        </p>
      </section>

      <section id="disclaimers">
        <h2>9. Disclaimers</h2>
        <p>
          THE SERVICE AND CONTENT ARE PROVIDED “AS IS” AND “AS AVAILABLE.” TO THE MAXIMUM EXTENT PERMITTED BY LAW, WE
          DISCLAIM ALL WARRANTIES, EXPRESS OR IMPLIED, INCLUDING MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND
          NON-INFRINGEMENT. WE DO NOT WARRANT UNINTERRUPTED OR ERROR-FREE STREAMING, OR THAT CONTENT WILL MEET YOUR
          EXPECTATIONS. SOME JURISDICTIONS DO NOT ALLOW CERTAIN DISCLAIMERS; IN THOSE CASES, OUR LIABILITY IS LIMITED
          TO THE FULLEST EXTENT PERMITTED.
        </p>
      </section>

      <section id="liability">
        <h2>10. Limitation of liability</h2>
        <p>
          TO THE MAXIMUM EXTENT PERMITTED BY LAW, AKHRA AND ITS AFFILIATES, LICENSORS, AND SUPPLIERS WILL NOT BE
          LIABLE FOR ANY INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL, OR PUNITIVE DAMAGES, OR LOSS OF PROFITS, DATA,
          OR GOODWILL, ARISING FROM YOUR USE OF THE SERVICE. OUR TOTAL LIABILITY FOR ANY CLAIM RELATING TO THE SERVICE
          OR CONTENT SHALL NOT EXCEED THE GREATER OF (A) THE AMOUNT YOU PAID US FOR THE RELEVANT PURCHASE IN THE TWELVE
          (12) MONTHS BEFORE THE CLAIM, OR (B) ONE THOUSAND INDIAN RUPEES (₹1,000), EXCEPT WHERE LIABILITY CANNOT BE
          LIMITED UNDER APPLICABLE CONSUMER PROTECTION LAW.
        </p>
      </section>

      <section id="indemnity">
        <h2>11. Indemnification</h2>
        <p>
          You will defend, indemnify, and hold harmless AKHRA and its officers, directors, employees, and agents
          from claims, damages, losses, and expenses (including reasonable legal fees) arising from your misuse of the
          Service, violation of these Terms, or infringement of third-party rights.
        </p>
      </section>

      <section id="termination">
        <h2>12. Suspension and termination</h2>
        <p>
          We may suspend or terminate your access immediately for breach of these Terms, suspected fraud, or legal
          requirement. You may close your account at any time through account settings. Upon termination, your license to
          Content ends except as required by law. Provisions that by nature should survive (licensing restrictions,
          disclaimers, liability limits, dispute resolution) will survive.
        </p>
      </section>

      <section id="disputes">
        <h2>13. Governing law and disputes</h2>
        <p>
          These Terms are governed by the laws of India, without regard to conflict-of-law principles. Courts in
          Bengaluru, Karnataka shall have exclusive jurisdiction over disputes, subject to mandatory consumer forums
          where you qualify. Before filing suit, you agree to contact us at legal@akhra.app to attempt informal
          resolution within thirty (30) days.
        </p>
      </section>

      <section id="changes">
        <h2>14. Changes to terms</h2>
        <p>
          We may modify these Terms from time to time. Material changes will be notified via the Service or email.
          Continued use after the effective date constitutes acceptance where permitted. If you do not agree, you must
          stop using the Service and cancel any recurring plan.
        </p>
      </section>

      <section id="misc">
        <h2>15. General</h2>
        <ul>
          <li><strong>Entire agreement:</strong> These Terms, the Privacy Policy, and purchase-specific terms constitute the entire agreement.</li>
          <li><strong>Severability:</strong> If any provision is unenforceable, the remainder remains in effect.</li>
          <li><strong>No waiver:</strong> Failure to enforce a provision is not a waiver.</li>
          <li><strong>Assignment:</strong> You may not assign these Terms; we may assign them in connection with a business transfer.</li>
        </ul>
      </section>

      <section id="contact">
        <h2>16. Contact</h2>
        <p>
          Legal notices: <a href="mailto:legal@akhra.app" className="text-primary underline">legal@akhra.app</a>
          <br />
          Customer support: support@akhra.app
        </p>
      </section>
    </LegalPageLayout>
  )
}
