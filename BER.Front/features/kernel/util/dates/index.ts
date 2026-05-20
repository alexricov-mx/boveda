import { format, parseISO, isValid } from 'date-fns'
import { es } from 'date-fns/locale'

export function formatDate(value: string | Date | null | undefined): string {
  if (!value) return ''
  const date = typeof value === 'string' ? parseISO(value) : value
  if (!isValid(date)) return ''
  return format(date, 'dd/MM/yyyy', { locale: es })
}

export function formatDateTime(value: string | Date | null | undefined): string {
  if (!value) return ''
  const date = typeof value === 'string' ? parseISO(value) : value
  if (!isValid(date)) return ''
  return format(date, 'dd/MM/yyyy HH:mm', { locale: es })
}

export function dateOnly(value: string | Date | null | undefined): string {
  return formatDate(value)
}

export function timeOnly(value: string | Date | null | undefined): string {
  if (!value) return ''
  const date = typeof value === 'string' ? parseISO(value) : value
  if (!isValid(date)) return ''
  return format(date, 'HH:mm', { locale: es })
}

export function toISODate(value: string | Date | null | undefined): string {
  if (!value) return ''
  const date = typeof value === 'string' ? parseISO(value) : value
  if (!isValid(date)) return ''
  return format(date, 'yyyy-MM-dd')
}
