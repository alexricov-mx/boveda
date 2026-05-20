export default interface UsuarioSesion {
  id: string
  nombre: string
  apellidos: string
  email: string
  roles: string[]
  grupos: string[]
}

export function nombreCompleto(usuario: UsuarioSesion): string {
  return `${usuario.nombre} ${usuario.apellidos}`.trim()
}
