import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type UsuarioSesion from '~/features/seguridad/model/usuario-sesion'

export const useSeguridadStore = defineStore('seguridad', () => {
  const config = useRuntimeConfig()

  const usuarioSesion = ref<UsuarioSesion | null>(null)
  const accessToken = ref<string | null>(null)

  const sesionIniciada = computed(() => !!usuarioSesion.value)
  const sesionNoIniciada = computed(() => !sesionIniciada.value)
  const usuarioAutenticado = computed(() => usuarioSesion.value)

  /**
   * Pide el access token al MVC legacy vía el endpoint /BerFront/Token.
   *
   * Modos de operación:
   *  - App Vue embebida en MVC (mismo origen): la cookie de sesión se envía
   *    automáticamente, sin CORS ni problemas de cookies de terceros.
   *  - App Vue standalone en dev (http://localhost:4000): petición cross-origin
   *    con credentials. Requiere que el MVC tenga CORS habilitado para ese origen
   *    y que el usuario haya iniciado sesión en el MVC primero.
   */
  async function getTokenFromMvc(): Promise<{ accessToken: string; name: string; email: string } | null> {
    const legacyUrl = (config.public.legacyAppUrl || '').replace(/\/$/, '')
    if (!legacyUrl) return null

    try {
      const response = await fetch(`${legacyUrl}/BerFront/Token`, {
        credentials: 'include',
        headers: { Accept: 'application/json' },
      })
      if (!response.ok) return null
      return response.json()
    } catch {
      return null
    }
  }

  async function getUsuarioAutenticado(): Promise<UsuarioSesion> {
    if (usuarioSesion.value) return usuarioSesion.value

    const mvcData = await getTokenFromMvc()

    if (mvcData?.accessToken) {
      accessToken.value = mvcData.accessToken
      const parts = (mvcData.name || '').trim().split(' ')
      usuarioSesion.value = {
        id: '',
        nombre: parts[0] || '',
        apellidos: parts.slice(1).join(' '),
        email: mvcData.email || '',
        roles: [],
        grupos: [],
      }
      return usuarioSesion.value
    }

    // Sin sesión MVC activa → redirigir al login del legacy
    throw { response: { status: 401 } }
  }

  async function getAccessToken(): Promise<string | null> {
    if (accessToken.value) return accessToken.value

    // Token no disponible — intentar refrescarlo desde el MVC
    const mvcData = await getTokenFromMvc()
    if (mvcData?.accessToken) {
      accessToken.value = mvcData.accessToken
      return accessToken.value
    }
    return null
  }

  function redirectToLegacyLogin(): void {
    const legacyUrl = config.public.legacyAppUrl
    if (legacyUrl) window.location.href = legacyUrl
  }

  async function logout(): Promise<void> {
    accessToken.value = null
    usuarioSesion.value = null
    redirectToLegacyLogin()
  }

  return {
    usuarioAutenticado,
    sesionIniciada,
    sesionNoIniciada,
    getUsuarioAutenticado,
    getAccessToken,
    logout,
  }
})
