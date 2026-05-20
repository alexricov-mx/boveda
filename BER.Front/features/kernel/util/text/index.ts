export function formatCurrency(value: number | null | undefined, decimals = 2): string {
  if (value == null) return ''
  return new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN',
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value)
}

export function formatPercent(value: number | null | undefined, decimals = 2): string {
  if (value == null) return ''
  return `${value.toFixed(decimals)}%`
}

export function formatoMiles(value: number | null | undefined): string {
  if (value == null) return ''
  return new Intl.NumberFormat('es-MX').format(value)
}

export function fileSize(bytes: number): string {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`
}
