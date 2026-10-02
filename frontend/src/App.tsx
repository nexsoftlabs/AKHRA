import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { ThemeProvider } from '@/components/theme/ThemeProvider'
import { AppShell } from '@/components/layout/AppShell'
import { AdminLayout } from '@/components/layout/AdminLayout'
import { AuthLayout } from '@/components/layout/AuthLayout'
import { AdminDashboardPage } from '@/pages/admin/AdminDashboardPage'
import { AdminFinancePage } from '@/pages/admin/AdminFinancePage'
import { AdminLoginPage } from '@/pages/admin/AdminLoginPage'
import { AdminMovieEditPage } from '@/pages/admin/AdminMovieEditPage'
import { AdminMovieNewPage } from '@/pages/admin/AdminMovieNewPage'
import { AdminMoviesPage } from '@/pages/admin/AdminMoviesPage'
import { AccountPage } from '@/pages/AccountPage'
import { BrowsePage } from '@/pages/BrowsePage'
import { ConfirmEmailPage } from '@/pages/ConfirmEmailPage'
import { HomePage } from '@/pages/HomePage'
import { LibraryPage } from '@/pages/LibraryPage'
import { MovieDetailPage } from '@/pages/MovieDetailPage'
import { LoginPage } from '@/pages/LoginPage'
import { PrivacyPolicyPage } from '@/pages/PrivacyPolicyPage'
import { RegisterPage } from '@/pages/RegisterPage'
import { TermsOfServicePage } from '@/pages/TermsOfServicePage'
import { PlansPage } from '@/pages/PlansPage'
import { WatchPage } from '@/pages/WatchPage'

const queryClient = new QueryClient()

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/admin/login" element={<AdminLoginPage />} />
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<AdminDashboardPage />} />
            <Route path="movies" element={<AdminMoviesPage />} />
            <Route path="movies/new" element={<AdminMovieNewPage />} />
            <Route path="movies/:id" element={<AdminMovieEditPage />} />
            <Route path="finance" element={<AdminFinancePage />} />
          </Route>

          <Route element={<AppShell />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/browse" element={<BrowsePage />} />
            <Route path="/movies/:slug" element={<MovieDetailPage />} />
            <Route path="/account" element={<AccountPage />} />
            <Route path="/library" element={<LibraryPage />} />
            <Route path="/plans" element={<PlansPage />} />
            <Route path="/watch/:slug" element={<WatchPage />} />
            <Route path="/confirm-email" element={<ConfirmEmailPage />} />
            <Route path="/privacy" element={<PrivacyPolicyPage />} />
            <Route path="/terms" element={<TermsOfServicePage />} />
          </Route>
          <Route element={<AuthLayout />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
      </ThemeProvider>
    </QueryClientProvider>
  )
}
