/**
 * api.js — Cliente HTTP reutilizable basado en jQuery $.ajax
 *
 * Uso básico:
 *   apiGet("OrdenSurtimiento/GetOrdenSurtimiento", { pageNum: 1 }, function(data) { ... });
 *   apiPost("OrdenSurtimiento/Firmar", modelObj, function(data) { ... });
 *
 * Manejo de errores del backend (RestUtility lanza { success, statusCode, message }):
 *   - Si onError es null, muestra errorAlert automático con [statusCode] message.
 *   - Si onError se provee, recibe el objeto { statusCode, message } para control manual.
 */

/**
 * Núcleo: construye y ejecuta la petición $.ajax.
 * @param {string}   method     - Verbo HTTP: GET | POST | PUT | PATCH | DELETE
 * @param {string}   url        - Ruta relativa al baseUrl (ej. "Controller/Action")
 * @param {object}   data       - Parámetros (GET/DELETE) o body (POST/PUT/PATCH)
 * @param {function} onSuccess  - Callback(responseData) cuando la respuesta es 2xx
 * @param {function} onError    - Callback({ statusCode, message }) en error HTTP.
 *                                Si es null, muestra errorAlert automático.
 */
function apiRequest(method, url, data, onSuccess, onError) {
    var isBodyMethod = ["POST", "PUT", "PATCH"].indexOf(method.toUpperCase()) !== -1;

    var ajaxOptions = {
        type: method,
        url: baseUrl + url,
        success: function (response) {
            if (onSuccess) onSuccess(response);
        },
        error: function (xhr) {
            var error = _parseApiError(xhr);
            if (onError) {
                onError(error);
            } else {
                errorAlert("[" + error.statusCode + "] " + error.message);
            }
        }
    };

    if (isBodyMethod) {
        ajaxOptions.contentType = "application/json";
        ajaxOptions.data = JSON.stringify(data);
    } else {
        ajaxOptions.data = data || {};
    }

    $.ajax(ajaxOptions);
}

/**
 * GET — Obtener datos. Los parámetros se envían como query string.
 * @param {string}   url
 * @param {object}   params     - Query string params (ej. { pageNum: 1, search: "abc" })
 * @param {function} onSuccess  - Callback(responseData)
 * @param {function} [onError]  - Callback({ statusCode, message }). Opcional.
 */
function apiGet(url, params, onSuccess, onError) {
    apiRequest("GET", url, params, onSuccess, onError);
}

/**
 * POST — Crear recurso. El body se serializa como JSON.
 * @param {string}   url
 * @param {object}   body       - Objeto a enviar como JSON en el body
 * @param {function} onSuccess  - Callback(responseData)
 * @param {function} [onError]  - Opcional.
 */
function apiPost(url, body, onSuccess, onError) {
    apiRequest("POST", url, body, onSuccess, onError);
}

/**
 * PUT — Reemplazar recurso completo. El body se serializa como JSON.
 * @param {string}   url
 * @param {object}   body
 * @param {function} onSuccess
 * @param {function} [onError]
 */
function apiPut(url, body, onSuccess, onError) {
    apiRequest("PUT", url, body, onSuccess, onError);
}

/**
 * PATCH — Actualización parcial. El body se serializa como JSON.
 * @param {string}   url
 * @param {object}   body
 * @param {function} onSuccess
 * @param {function} [onError]
 */
function apiPatch(url, body, onSuccess, onError) {
    apiRequest("PATCH", url, body, onSuccess, onError);
}

/**
 * DELETE — Eliminar recurso. Los parámetros se envían como query string.
 * @param {string}   url
 * @param {object}   params
 * @param {function} onSuccess
 * @param {function} [onError]
 */
function apiDelete(url, params, onSuccess, onError) {
    apiRequest("DELETE", url, params, onSuccess, onError);
}

/**
 * Mensajes en español por código HTTP.
 * Se usa cuando el backend no devuelve un mensaje legible.
 */
var _httpMessages = {
    // 4xx — errores del cliente
    400: "Solicitud incorrecta. Verifica los datos enviados.",
    401: "No autenticado. Tu sesión puede haber expirado, vuelve a iniciar sesión.",
    403: "Acceso denegado. No tienes permisos para realizar esta acción.",
    404: "El recurso solicitado no existe.",
    405: "Método no permitido.",
    408: "Tiempo de espera agotado. El servidor tardó demasiado en responder.",
    409: "Conflicto. El recurso ya existe o hay un estado inconsistente.",
    410: "El recurso ya no está disponible.",
    413: "El contenido enviado es demasiado grande.",
    422: "Los datos enviados no son válidos o están incompletos.",
    429: "Demasiadas solicitudes. Espera un momento antes de intentar de nuevo.",
    // 5xx — errores del servidor
    500: "Error interno del servidor. Intenta más tarde.",
    502: "El servidor no está disponible temporalmente (Bad Gateway).",
    503: "Servicio no disponible. El servidor puede estar en mantenimiento.",
    504: "Tiempo de espera del servidor agotado (Gateway Timeout)."
};

/**
 * Extrae { statusCode, message } del objeto xhr de jQuery.
 * Prioridad del mensaje:
 *   1. json.message del backend (si existe y no es el código crudo tipo "Forbidden: Forbidden")
 *   2. Mensaje en español del mapa _httpMessages
 *   3. Mensaje genérico de fallback
 * @param {object} xhr - jqXHR de jQuery
 * @returns {{ statusCode: number, message: string }}
 */
function _parseApiError(xhr) {
    var json = xhr.responseJSON;
    var statusCode = (json && json.statusCode) ? json.statusCode : xhr.status;
    var backendMessage = (json && json.message) ? json.message : null;

    // Si el backend devuelve solo el código HTTP en texto (ej. "Forbidden: Forbidden"),
    // lo reemplazamos por el mensaje legible en español.
    var isCrudeCode = backendMessage && /^[A-Z][a-zA-Z]+:\s*[A-Z][a-zA-Z]+\s*$/.test(backendMessage.trim());
    var message = (!backendMessage || isCrudeCode)
        ? (_httpMessages[statusCode] || "Error desconocido. Intenta más tarde.")
        : backendMessage;

    return { statusCode: statusCode, message: message };
}