import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import './index.css'
import './config/theme.css'
import './i18n'
import { ThemeProvider } from './context/ThemeContext'
import { AuthProvider } from './context/AuthContext'
import { PreferencesProvider } from './context/PreferencesContext'
import App from './App.tsx'

// A lazy route chunk can fail to load when the server restarted (dev) or a new build was
// deployed (prod) under an open tab — React caches the rejected import forever, so the
// error boundary's "Try again" can never recover it. Reload once to pick up fresh modules;
// the 30 s throttle prevents a reload loop when the server is actually down (the error
// boundary then shows the failure instead).
window.addEventListener('vite:preloadError', (event) => {
  const lastReload = Number(sessionStorage.getItem('preload-error-reloaded') ?? 0)
  if (Date.now() - lastReload < 30_000) return
  sessionStorage.setItem('preload-error-reloaded', String(Date.now()))
  event.preventDefault()
  window.location.reload()
})

// Server-state defaults tuned for an admin app on large data: a 30 s freshness window means
// navigating between screens (or refocusing the tab) reuses cached results instead of refiring
// every list/lookup query — mutations still see fresh data because handlers invalidate their
// query keys explicitly after each save.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          {/* Inside Auth (needs the signed-in user) and inside Theme (applies the theme it reads). */}
          <PreferencesProvider>
            <App />
          </PreferencesProvider>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  </StrictMode>,
)
