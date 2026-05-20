import path from 'path'
import vuetify, { transformAssetUrls } from 'vite-plugin-vuetify'

const {
  DEV_SERVER_PORT,
  API_BASE_URL,
  AZURE_AD_CLIENT_ID,
  AZURE_AD_TENANT_ID,
  AZURE_AD_REDIRECT_URI,
  AZURE_AD_SCOPES,
  BRANDING_CORPORATION_SHORT_NAME,
  BRANDING_CORPORATION_LONG_NAME,
  BRANDING_CORPORATION_LOGO_URL,
  BRANDING_APPLICATION_SHORT_NAME,
  BRANDING_APPLICATION_LONG_NAME,
  BRANDING_APPLICATION_URL,
  GLOBALIZATION_DEFAULT_LOCALE,
} = process.env

export default defineNuxtConfig({
  devtools: { enabled: true },

  devServer: {
    port: Number(DEV_SERVER_PORT || 4000),
  },

  components: false,
  telemetry: false,
  ssr: false,
  spaLoadingTemplate: 'loading.html',

  hooks: {
    'pages:extend'(pages) {
      for (const page of pages) {
        if (!page.meta) page.meta = {}
        const meta: any = page.meta
        if (meta.protected === undefined) meta.protected = true
      }
    },
  },

  runtimeConfig: {
    public: {
      apiUrl: API_BASE_URL || '',
      legacyAppUrl: process.env.LEGACY_APP_URL || '',
      azureAd: {
        clientId: AZURE_AD_CLIENT_ID || '',
        tenantId: AZURE_AD_TENANT_ID || '',
        redirectUri: AZURE_AD_REDIRECT_URI || '',
        scopes: AZURE_AD_SCOPES || '',
      },
    },
  },

  vite: {
    define: {
      'process.platform': JSON.stringify('unknown'),
    },
    resolve: {
      alias: {
        path: 'path-browserify',
      },
    },
    vue: {
      template: {
        transformAssetUrls,
      },
    },
  },

  build: {
    transpile: ['vuetify'],
  },

  app: {
    baseURL: process.env.NUXT_APP_BASE_URL || '/',
    head: {
      title: BRANDING_APPLICATION_SHORT_NAME || 'BER',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      ],
    },
  },

  nitro: {
    // Cuando se compila para legacy (NUXT_OUTPUT_DIR definido), los archivos
    // van directamente a wwwroot/BERVueDist del proyecto MVC.
    output: {
      publicDir: process.env.NUXT_OUTPUT_DIR
        ? path.resolve(process.env.NUXT_OUTPUT_DIR)
        : path.resolve(__dirname, '.output/public'),
    },
    // Solo pre-renderizar la raíz — el enrutamiento lo maneja Vue Router en el cliente
    prerender: {
      crawlLinks: false,
      routes: ['/'],
    },
  },

  appConfig: {
    branding: {
      corporationShortName: BRANDING_CORPORATION_SHORT_NAME || 'Pemex',
      corporationLongName: BRANDING_CORPORATION_LONG_NAME || 'Petróleos Mexicanos',
      corporationLogoUrl: BRANDING_CORPORATION_LOGO_URL || '/img/logos/logo_pmx.png',
      applicationShortName: BRANDING_APPLICATION_SHORT_NAME || 'BER',
      applicationLongName:
        BRANDING_APPLICATION_LONG_NAME || 'Bóveda Electrónica de Recepción',
      applicationUrl: BRANDING_APPLICATION_URL || '',
    },
    globalization: {
      defaultLocale: GLOBALIZATION_DEFAULT_LOCALE || 'es',
    },
  },

  modules: [
    '@pinia/nuxt',
    (_options, nuxt) => {
      nuxt.hooks.hook('vite:extendConfig', (config) => {
        // @ts-ignore
        config.plugins.push(vuetify({ autoImport: true }))
      })
    },
  ],

  experimental: {
    viteEnvironmentApi: true,
  },

  compatibilityDate: '2024-07-10',
})
