import { defineStore } from 'pinia'
import { useAppConfig } from 'nuxt/app'

export const useBrandingStore = defineStore('branding', () => {
  const appConfig = useAppConfig()
  const branding: any = appConfig.branding

  const {
    corporationShortName,
    corporationLongName,
    corporationLogoUrl,
    applicationShortName,
    applicationLongName,
    applicationUrl,
  } = branding

  return {
    corporationShortName,
    corporationLongName,
    corporationLogoUrl,
    applicationShortName,
    applicationLongName,
    applicationUrl,
  }
})
