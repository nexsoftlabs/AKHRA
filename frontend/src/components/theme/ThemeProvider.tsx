import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { applyTheme, getStoredTheme, resolveDark, setStoredTheme, type Theme } from '@/lib/theme'

type ThemeContextValue = {
  theme: Theme
  resolvedDark: boolean
  setTheme: (theme: Theme) => void
  toggleLightDark: () => void
}

const ThemeContext = createContext<ThemeContextValue | null>(null)

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setThemeState] = useState<Theme>(() => getStoredTheme())
  const [resolvedDark, setResolvedDark] = useState(() => resolveDark(getStoredTheme()))

  const setTheme = useCallback((next: Theme) => {
    setThemeState(next)
    setStoredTheme(next)
    applyTheme(next)
    setResolvedDark(resolveDark(next))
  }, [])

  const toggleLightDark = useCallback(() => {
    const next = resolvedDark ? 'light' : 'dark'
    setTheme(next)
  }, [resolvedDark, setTheme])

  useEffect(() => {
    if (theme !== 'system') return
    const media = window.matchMedia('(prefers-color-scheme: dark)')
    const onChange = () => {
      applyTheme('system')
      setResolvedDark(media.matches)
    }
    media.addEventListener('change', onChange)
    return () => media.removeEventListener('change', onChange)
  }, [theme])

  const value = useMemo(
    () => ({ theme, resolvedDark, setTheme, toggleLightDark }),
    [theme, resolvedDark, setTheme, toggleLightDark],
  )

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}

export function useTheme() {
  const ctx = useContext(ThemeContext)
  if (!ctx) {
    throw new Error('useTheme must be used within ThemeProvider')
  }
  return ctx
}
