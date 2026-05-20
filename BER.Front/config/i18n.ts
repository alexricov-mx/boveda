import { createI18n } from 'vue-i18n'
import kernelEs from '~/features/kernel/translations/es'
import kernelEn from '~/features/kernel/translations/en'
import seguridadEs from '~/features/seguridad/translations/es'
import seguridadEn from '~/features/seguridad/translations/en'

export function useI18n(defaultLocale: string) {
  return createI18n({
    legacy: false,
    globalInjection: true,
    locale: defaultLocale || 'es',
    fallbackLocale: 'es',
    messages: {
      es: {
        kernel: kernelEs,
        seguridad: seguridadEs,
      },
      en: {
        kernel: kernelEn,
        seguridad: seguridadEn,
      },
    },
  })
}
