import { useI18n } from '~/config/i18n'

export default defineNuxtPlugin(({ vueApp }) => {
  const { globalization } = useAppConfig() as any
  vueApp.use(useI18n(globalization?.defaultLocale || 'es'))
})
