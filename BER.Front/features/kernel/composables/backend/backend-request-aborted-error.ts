export default class BackendRequestAbortedError extends Error {
  constructor() {
    super('La solicitud al servidor fue cancelada.')
  }
}
