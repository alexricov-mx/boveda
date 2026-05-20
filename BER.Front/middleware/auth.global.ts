import { defineNuxtRouteMiddleware } from 'nuxt/app'
import BackendError from '~/features/kernel/composables/backend/backend-error'

export default defineNuxtRouteMiddleware(async (to) => {
  if (!process.client) return
  if (to.meta.protected === false) return

  try {
    const seguridadStore = useSeguridadStore()
    await seguridadStore.getUsuarioAutenticado()
  } catch (error: any) {
    const config = useRuntimeConfig()
    const status: number =
      error instanceof BackendError ? error.status : error?.response?.status || 0

    if (status === 401) {
      // Sin sesión de Azure AD → redirigir al login del MVC legacy
      if (process.client) {
        const legacyUrl = config.public.legacyAppUrl
        if (legacyUrl) {
          window.location.href = legacyUrl
          return abortNavigation()
        }
      }
    }

    console.error('[auth] Error inesperado en autenticación:', error)
  }
})
