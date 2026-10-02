import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { ensureCsrfToken } from '@/lib/api'

void ensureCsrfToken().catch(() => {
  /* API may be offline during first paint */
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
