import { LegalPageLayout } from '@/components/legal/LegalPageLayout'

const LAST_UPDATED = 'September 28, 2026'

export function PrivacyPolicyPage() {
  return (
    <LegalPageLayout
      title="Privacy Policy"
      lastUpdated={LAST_UPDATED}
      summary="This Privacy Policy explains how AKHRA (“we”, “us”, “our”) collects, uses, shares, and protects personal information when you use our websites, applications, and streaming services (the “Service”)."
    >
      <section id="scope">
        <h2>1. Who we are and scope</h2>
        <p>
          AKHRA operates a rights-managed video-on-demand platform. This policy applies to visitors, registered
          users, purchasers, and anyone who interacts with the Service. It does not apply to third-party sites or apps
          linked from the Service (including payment or identity providers), which have their own privacy practices.
        </p>
      </section>

      <section id="collect">
        <h2>2. Information we collect</h2>
        <h3>2.1 Information you provide</h3>
        <ul>
          <li>Account data: name, email address, phone number (if used for OTP), password (stored hashed), and profile preferences.</li>
          <li>Transaction data: purchase history, order identifiers, billing currency, and payment status (we do not store full card numbers).</li>
          <li>Communications: support requests, feedback, and correspondence with us.</li>
          <li>Age or eligibility attestations where required for certain titles or features.</li>
        </ul>
        <h3>2.2 Information collected automatically</h3>
        <ul>
          <li>Device and app data: device type, operating system, browser, app version, language, and time zone.</li>
          <li>Log and usage data: IP address, pages viewed, search queries, playback events (start, pause, progress), errors, and referral URLs.</li>
          <li>Identifiers: session cookies, device identifiers, and similar technologies used for authentication, security, and analytics.</li>
          <li>Approximate location derived from IP address for licensing, fraud prevention, and content availability.</li>
        </ul>
        <h3>2.3 Information from third parties</h3>
        <ul>
          <li>Payment processors (e.g. Razorpay): payment confirmation, risk signals, and dispute data.</li>
          <li>Sign-in providers (e.g. Google): name, email, and authentication tokens as permitted by your settings.</li>
          <li>Content licensors and anti-piracy partners: territorial rights and compliance signals where contractually required.</li>
        </ul>
      </section>

      <section id="use">
        <h2>3. How we use information</h2>
        <p>We use personal information to:</p>
        <ul>
          <li>Provide, personalize, and improve the Service, including recommendations and resume playback.</li>
          <li>Process purchases, grants entitlements, prevent fraud, and comply with tax and accounting obligations.</li>
          <li>Authenticate users, maintain sessions, and protect accounts.</li>
          <li>Enforce territorial and windowing restrictions required by content licenses.</li>
          <li>Send service messages (receipts, security alerts, policy updates) and, where permitted, marketing with opt-out.</li>
          <li>Conduct analytics, debugging, capacity planning, and product research using aggregated or de-identified data where possible.</li>
          <li>Comply with law, respond to lawful requests, and establish or defend legal claims.</li>
        </ul>
      </section>

      <section id="legal-bases">
        <h2>4. Legal bases (where applicable)</h2>
        <p>
          Depending on your location, we rely on one or more of: performance of a contract (providing the Service you
          request); legitimate interests (security, analytics, service improvement) balanced against your rights;
          consent (optional marketing, certain cookies); and legal obligation (records retention, regulatory requests).
          In India, we also align practices with the Digital Personal Data Protection Act, 2023 (DPDP Act), as
          applicable to our role as data fiduciary.
        </p>
      </section>

      <section id="share">
        <h2>5. How we share information</h2>
        <p>We do not sell your personal information. We may share information with:</p>
        <ul>
          <li>
            <strong>Service providers</strong> under contract: hosting, CDN, encoding, email/SMS, customer support,
            analytics, and payment processing—only as needed to perform services for us.
          </li>
          <li>
            <strong>Content partners and licensors</strong> when required to report viewership, territory compliance, or
            royalty calculations under license agreements (often in aggregated form).
          </li>
          <li>
            <strong>Affiliates</strong> within our corporate group for operations consistent with this policy.
          </li>
          <li>
            <strong>Legal and safety</strong> when we believe disclosure is required by law, to protect users or the
            public, or to detect abuse, piracy, or circumvention of technical protection measures.
          </li>
          <li>
            <strong>Business transfers</strong> in connection with merger, acquisition, or asset sale, subject to
            continued protection consistent with this policy.
          </li>
        </ul>
      </section>

      <section id="cookies">
        <h2>6. Cookies and similar technologies</h2>
        <p>
          We use essential cookies for sign-in and security, and may use analytics and preference cookies where
          permitted. You can control cookies through browser settings; disabling essential cookies may limit Service
          functionality. We do not respond to “Do Not Track” signals in a uniform way across all browsers.
        </p>
      </section>

      <section id="retention">
        <h2>7. Data retention</h2>
        <p>
          We retain personal information for as long as your account is active and as needed to provide the Service,
          resolve disputes, enforce agreements, and meet legal retention requirements (e.g. financial records).
          When no longer needed, we delete or de-identify data subject to backup and archival cycles.
        </p>
      </section>

      <section id="security">
        <h2>8. Security</h2>
        <p>
          We implement administrative, technical, and organizational measures appropriate to the risk, including
          encryption in transit, access controls, and monitoring. No method of transmission or storage is completely
          secure; you are responsible for safeguarding your credentials.
        </p>
      </section>

      <section id="children">
        <h2>9. Children’s privacy</h2>
        <p>
          The Service is not directed to children under 13 (or the minimum age in your jurisdiction). We do not
          knowingly collect personal information from children without verifiable parental consent where required.
          Parental controls and age-gated profiles may be offered for family accounts. Contact us if you believe we
          have collected a child’s data improperly.
        </p>
      </section>

      <section id="rights">
        <h2>10. Your rights and choices</h2>
        <p>Subject to applicable law, you may have the right to:</p>
        <ul>
          <li>Access, correct, or delete certain personal information.</li>
          <li>Withdraw consent where processing is consent-based.</li>
          <li>Object to or restrict certain processing, including direct marketing.</li>
          <li>Port data you provided in a structured, commonly used format where technically feasible.</li>
          <li>Lodge a complaint with a supervisory authority (e.g. Data Protection Board of India under the DPDP Act).</li>
        </ul>
        <p>
          Manage account details in <strong>Account</strong> settings or contact us. We may verify your identity before
          fulfilling requests.
        </p>
      </section>

      <section id="international">
        <h2>11. International transfers</h2>
        <p>
          We may process and store information in India and other countries where we or our providers operate. When we
          transfer data across borders, we use appropriate safeguards such as standard contractual clauses or equivalent
          mechanisms required by applicable law.
        </p>
      </section>

      <section id="changes">
        <h2>12. Changes to this policy</h2>
        <p>
          We may update this Privacy Policy from time to time. We will post the revised version with a new “Last
          updated” date and, for material changes, provide additional notice (e.g. email or in-app). Continued use after
          the effective date constitutes acceptance where permitted by law.
        </p>
      </section>

      <section id="contact">
        <h2>13. Contact us</h2>
        <p>
          Data protection inquiries: <a href="mailto:privacy@akhra.app" className="text-primary underline">privacy@akhra.app</a>
          <br />
          Grievance officer (India): grievance@akhra.app (response within timelines prescribed under applicable law).
        </p>
      </section>
    </LegalPageLayout>
  )
}
