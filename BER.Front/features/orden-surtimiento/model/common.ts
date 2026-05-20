export interface Pager {
  totalItems: number
  currentPage: number
  pageSize: number
  totalPages: number
  startPage: number
  endPage: number
}

export interface PagedResponse<T> {
  data: T[]
  message: string
  status: number
  pager: Pager
}

export interface DateHelperModel {
  order: number
  title: string
  description: string
  info: string
  date: string | null
}
