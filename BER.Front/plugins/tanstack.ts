import { VueQueryPlugin } from '@tanstack/vue-query'
import BackendError from '~/features/kernel/composables/backend/backend-error'

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.use(VueQueryPlugin, {
    queryClientConfig: {
      defaultOptions: {
        queries: {
          refetchOnWindowFocus: false,
          throwOnError: true,
          retry(failureCount: number, error: Error) {
            if (
              error instanceof BackendError &&
              (error.isUnauthorized ||
                error.isForbidden ||
                error.isBadRequest ||
                error.isConflict)
            )
              return false
            return failureCount < 1
          },
        },
        mutations: {
          throwOnError: true,
          retry(failureCount: number, error: Error) {
            if (
              error instanceof BackendError &&
              (error.isUnauthorized ||
                error.isForbidden ||
                error.isBadRequest ||
                error.isConflict)
            )
              return false
            return failureCount < 1
          },
        },
      },
    },
  })
})
