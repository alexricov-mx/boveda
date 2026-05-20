export type Nullable<T> = T | null | undefined

export interface Pagination<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface SelectableOption<T = string> {
  value: T
  title: string
  subtitle?: string
  color?: string
  icon?: string
}

export interface DataResult<T> {
  data: T
  message?: string
  status: number
}

export interface PagedResult<T> {
  items: T[]
  totalRecords: number
  pageNumber: number
  pageSize: number
}
