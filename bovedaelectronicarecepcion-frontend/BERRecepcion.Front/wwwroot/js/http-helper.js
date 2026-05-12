/**
 * Helper para manejo de respuestas HTTP en AJAX calls
 * Convierte del patrón antiguo { success: bool } a códigos HTTP apropiados
 * 
 * @version 1.0
 * @date 2026-01-20
 */

const HttpHelper = (function () {
    'use strict';

    /**
     * Configuración predeterminada para AJAX calls
     */
    const defaultConfig = {
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        cache: false
    };

    /**
     * Maneja respuestas exitosas de AJAX (HTTP 200-299)
     * @param {Object} data - Datos de respuesta
     * @param {Function} callback - Callback a ejecutar con los datos
     */
    function handleSuccess(data, callback) {
        if (typeof callback === 'function') {
            callback(data);
        }
    }

    /**
     * Maneja errores de AJAX (HTTP 400+)
     * @param {Object} xhr - Objeto XMLHttpRequest
     * @param {Function} errorCallback - Callback de error personalizado
     */
    function handleError(xhr, errorCallback) {
        let errorMessage = 'Ocurrió un error al procesar la solicitud.';
        let errorDetails = null;

        try {
            const response = xhr.responseJSON || JSON.parse(xhr.responseText || '{}');
            
            switch (xhr.status) {
                case 400: // Bad Request
                    errorMessage = response.message || 'Los datos enviados no son válidos.';
                    errorDetails = response.errors || null;
                    break;
                
                case 401: // Unauthorized
                    errorMessage = 'Su sesión ha expirado. Por favor inicie sesión nuevamente.';
                    setTimeout(() => {
                        window.location.href = '/Login';
                    }, 2000);
                    break;
                
                case 403: // Forbidden
                    errorMessage = response.message || 'No tiene permisos para realizar esta operación.';
                    break;
                
                case 404: // Not Found
                    errorMessage = response.message || 'El recurso solicitado no fue encontrado.';
                    break;
                
                case 409: // Conflict
                    errorMessage = response.message || 'El recurso ya existe o hay un conflicto.';
                    break;
                
                case 422: // Unprocessable Entity
                    errorMessage = response.message || 'La operación no puede procesarse debido a reglas de negocio.';
                    errorDetails = response.errors || null;
                    break;
                
                case 500: // Internal Server Error
                    errorMessage = 'Error interno del servidor. Por favor contacte al administrador.';
                    console.error('Server Error:', response);
                    break;
                
                case 503: // Service Unavailable
                    errorMessage = 'El servicio no está disponible temporalmente. Por favor intente más tarde.';
                    break;
                
                default:
                    errorMessage = response.message || `Error ${xhr.status}: ${xhr.statusText}`;
            }
        } catch (e) {
            console.error('Error parsing response:', e);
            errorMessage = `Error ${xhr.status}: No se pudo procesar la respuesta del servidor.`;
        }

        if (typeof errorCallback === 'function') {
            errorCallback(errorMessage, errorDetails, xhr.status);
        } else {
            // Mostrar alerta por defecto si no hay callback
            showError(errorMessage, errorDetails);
        }
    }

    /**
     * Muestra un mensaje de error (puede ser personalizado por la aplicación)
     * @param {String} message - Mensaje de error
     * @param {Object} details - Detalles adicionales del error
     */
    function showError(message, details) {
        // Implementación básica - debe ser personalizada según el framework UI usado
        if (typeof Swal !== 'undefined') {
            // SweetAlert2
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: message,
                footer: details ? `<small>${JSON.stringify(details)}</small>` : null
            });
        } else if (typeof toastr !== 'undefined') {
            // Toastr
            toastr.error(message);
        } else {
            // Fallback a alert
            alert(message);
        }
        
        if (details) {
            console.error('Error details:', details);
        }
    }

    /**
     * Realiza una petición AJAX con manejo de errores HTTP apropiado
     * @param {Object} options - Configuración de la petición
     * @param {String} options.url - URL del endpoint
     * @param {String} options.type - Método HTTP (GET, POST, PUT, DELETE)
     * @param {Object} options.data - Datos a enviar
     * @param {Function} options.success - Callback de éxito
     * @param {Function} options.error - Callback de error
     * @param {Boolean} options.showLoading - Mostrar indicador de carga
     */
    function ajax(options) {
        const config = $.extend({}, defaultConfig, options);
        
        // Mostrar loading si está habilitado
        if (config.showLoading !== false) {
            showLoading(true);
        }

        return $.ajax({
            url: config.url,
            type: config.type || 'GET',
            data: config.data,
            contentType: config.contentType,
            dataType: config.dataType,
            cache: config.cache,
            headers: config.headers || {},
            success: function (data, textStatus, xhr) {
                if (config.showLoading !== false) {
                    showLoading(false);
                }
                handleSuccess(data, config.success);
            },
            error: function (xhr, textStatus, errorThrown) {
                if (config.showLoading !== false) {
                    showLoading(false);
                }
                handleError(xhr, config.error);
            }
        });
    }

    /**
     * Muestra/oculta indicador de carga
     * @param {Boolean} show - true para mostrar, false para ocultar
     */
    function showLoading(show) {
        // Implementación básica - debe ser personalizada
        if (show) {
            if ($('#global-loading').length === 0) {
                $('body').append('<div id="global-loading" class="loading-overlay"><div class="spinner"></div></div>');
            }
            $('#global-loading').show();
        } else {
            $('#global-loading').hide();
        }
    }

    /**
     * Shortcuts para métodos HTTP comunes
     */
    function get(url, success, error) {
        return ajax({ url: url, type: 'GET', success: success, error: error });
    }

    function post(url, data, success, error) {
        return ajax({ url: url, type: 'POST', data: JSON.stringify(data), success: success, error: error });
    }

    function put(url, data, success, error) {
        return ajax({ url: url, type: 'PUT', data: JSON.stringify(data), success: success, error: error });
    }

    function del(url, success, error) {
        return ajax({ url: url, type: 'DELETE', success: success, error: error });
    }

    /**
     * Wrapper para mantener compatibilidad con código legacy que usa { success: bool }
     * NOTA: Este método está deprecated y debe eliminarse después de la migración completa
     * @deprecated Usar ajax() directamente con manejo de códigos HTTP
     */
    function legacyAjax(options) {
        console.warn('legacyAjax is deprecated. Please update to use HttpHelper.ajax() with proper HTTP codes.');
        
        return ajax({
            url: options.url,
            type: options.type || 'POST',
            data: options.data,
            success: function (data) {
                // Código legacy espera { success: bool, message: string, data: any }
                if (typeof options.success === 'function') {
                    options.success({ success: true, message: data.message || 'Operación exitosa', data: data });
                }
            },
            error: function (message, details, statusCode) {
                // Código legacy espera { success: bool, message: string }
                if (typeof options.error === 'function') {
                    options.error({ success: false, message: message });
                }
            }
        });
    }

    // API pública
    return {
        ajax: ajax,
        get: get,
        post: post,
        put: put,
        delete: del,
        legacyAjax: legacyAjax, // Deprecated
        showError: showError,
        showLoading: showLoading
    };
})();

// Hacer disponible globalmente
window.HttpHelper = HttpHelper;
