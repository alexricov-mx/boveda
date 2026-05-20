import { computed, reactive, ref, type MaybeRef, unref } from 'vue'
import type { UnwrapNestedRefs } from '@vue/reactivity'
import axios, {
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
  type GenericAbortSignal,
  isAxiosError,
} from 'axios'
import BackendError from './backend-error'
import BackendRequestAbortedError from './backend-request-aborted-error'

export interface BackendRequest<R = any> {
  execute: <D = any>(config?: AxiosRequestConfig<D>) => Promise<R>
  abort: () => void
  data: R | undefined
  response: AxiosResponse<R> | undefined
  error?: any
  isExecuting: boolean
  isCompleted: boolean
  isFailed: boolean
  isAborted: boolean
}

export type ReactiveBackendRequest<R = any> = UnwrapNestedRefs<BackendRequest<R>>

export interface Backend {
  get: <R = any>(path: string, config?: AxiosRequestConfig) => Promise<R>
  post: <R = any, D = any>(path: string, data?: D, config?: AxiosRequestConfig<D>) => Promise<R>
  put: <R = any, D = any>(path: string, data?: D, config?: AxiosRequestConfig<D>) => Promise<R>
  patch: <R = any, D = any>(path: string, data?: D, config?: AxiosRequestConfig<D>) => Promise<R>
  delete: <R = any>(path: string, config?: AxiosRequestConfig) => Promise<R>
  execute: <R = any, D = any>(config: AxiosRequestConfig<D>) => Promise<R>
  createRequest: <R = any, D = any>(config?: AxiosRequestConfig<D>) => ReactiveBackendRequest<R>
}

const axiosInstances: Record<string, AxiosInstance> = {}

export default function useBackend(baseUrlOverride?: MaybeRef<string>): Backend {
  const config = useRuntimeConfig()
  const seguridadStore = useSeguridadStore()

  const axiosInstance = computed<AxiosInstance>(() => {
    if (!process.client) return axios.create()

    const baseURL = unref(baseUrlOverride) || config.public.apiUrl
    if (baseURL in axiosInstances) return axiosInstances[baseURL]

    const instance = axios.create({ baseURL, timeout: 30_000 })

    instance.interceptors.request.use(async (req) => {
      const token = await seguridadStore.getAccessToken()
      if (token) req.headers.Authorization = `Bearer ${token}`
      return req
    })

    instance.interceptors.response.use(
      undefined,
      (error) =>
        Promise.reject(
          error.name === 'CanceledError'
            ? new BackendRequestAbortedError()
            : isAxiosError(error)
              ? new BackendError(error)
              : error,
        ),
    )

    axiosInstances[baseURL] = instance
    return instance
  })

  function createRequest<R, D = any>(config?: AxiosRequestConfig<D>): ReactiveBackendRequest<R> {
    const data = ref<R>()
    const response = ref<AxiosResponse<R>>()
    const error = ref<any>()
    const isExecuting = ref(false)
    const isCompleted = ref(false)
    const isFailed = ref(false)
    const isAborted = ref(false)
    let abortController: AbortController | undefined

    async function execute(c?: AxiosRequestConfig): Promise<R> {
      if (isExecuting.value) throw new Error('La solicitud HTTP ya está en ejecución.')

      data.value = undefined
      error.value = undefined
      isExecuting.value = true
      isCompleted.value = false
      isFailed.value = false
      isAborted.value = false
      abortController = undefined

      let abortSignal: GenericAbortSignal | undefined

      try {
        if (c?.signal) {
          abortSignal = c.signal
        } else {
          abortController = new AbortController()
          abortSignal = abortController.signal
        }
        const requestConfig: AxiosRequestConfig = Object.assign({}, config || {}, c || {}, {
          signal: abortSignal,
        })
        response.value = await axiosInstance.value.request<R, AxiosResponse<R>, D>(requestConfig)
        data.value = response.value.data
        isCompleted.value = true
        return response.value.data as R
      } catch (e: any) {
        if (e instanceof BackendRequestAbortedError) {
          isAborted.value = true
        } else {
          error.value = e
          isFailed.value = true
        }
        throw e
      } finally {
        isExecuting.value = false
        abortController = undefined
      }
    }

    function abort() {
      abortController?.abort()
    }

    const request: any = { execute, abort }

    const props: Array<keyof BackendRequest<R>> = [
      'data',
      'response',
      'error',
      'isExecuting',
      'isCompleted',
      'isFailed',
      'isAborted',
    ]
    const refs: any = { data, response, error, isExecuting, isCompleted, isFailed, isAborted }
    for (const prop of props) {
      Object.defineProperty(request, prop, { get: () => refs[prop].value, enumerable: true })
    }

    return reactive<BackendRequest<R>>(request as BackendRequest<R>)
  }

  return {
    async get<R>(path: string, config?: AxiosRequestConfig): Promise<R> {
      const res = await axiosInstance.value.get<R>(path, config)
      return res.data
    },
    async post<R, D>(path: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
      const res = await axiosInstance.value.post<R, AxiosResponse<R>, D>(path, data, config)
      return res.data
    },
    async put<R, D>(path: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
      const res = await axiosInstance.value.put<R, AxiosResponse<R>, D>(path, data, config)
      return res.data
    },
    async patch<R, D>(path: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
      const res = await axiosInstance.value.patch<R, AxiosResponse<R>, D>(path, data, config)
      return res.data
    },
    async delete<R>(path: string, config?: AxiosRequestConfig): Promise<R> {
      const res = await axiosInstance.value.delete<R>(path, config)
      return res.data
    },
    async execute<R, D>(config: AxiosRequestConfig<D>): Promise<R> {
      const res = await axiosInstance.value.request<R>(config)
      return res.data
    },
    createRequest,
  }
}
