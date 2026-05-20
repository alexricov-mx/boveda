import type { AxiosError } from 'axios'
import { StatusCodes } from 'http-status-codes'

export default class BackendError extends Error {
  constructor(source: AxiosError) {
    super((source.response?.data as any)?.title || source.message)

    let data: any = source.response?.data
    if (typeof data === 'string') {
      try {
        data = JSON.parse(data)
      } catch {
        // response no es JSON válido
      }
    }

    this.status = source.response?.status || 0
    this.type = data?.type || ''
    this.title = data?.title || source.message
    this.detail = data?.detail
    this.code = data?.code || source.code
    this.data = data
    this.traceId = data?.traceId || ''
    this.errors = data?.errors
    this.source = source
  }

  public readonly status: number
  public readonly type: string
  public readonly title: string
  public readonly detail?: string
  public readonly code?: string
  public readonly traceId: string
  public readonly data?: any
  public readonly errors?: any[]
  public readonly source: AxiosError

  get isBadRequest() {
    return this.status === StatusCodes.BAD_REQUEST
  }
  get isUnauthorized() {
    return this.status === StatusCodes.UNAUTHORIZED
  }
  get isForbidden() {
    return this.status === StatusCodes.FORBIDDEN
  }
  get isNotFound() {
    return this.status === StatusCodes.NOT_FOUND
  }
  get isConflict() {
    return this.status === StatusCodes.CONFLICT
  }
  get isUnprocessableEntity() {
    return this.status === StatusCodes.UNPROCESSABLE_ENTITY
  }
  get isServiceUnavailable() {
    return this.status === StatusCodes.SERVICE_UNAVAILABLE
  }
}
