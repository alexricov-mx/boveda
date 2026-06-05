$(function () {
    $('[data-toggle="tooltip"]').tooltip();
    //Verifica estado de tema oscuro
    darkMode(getCookie("dark-mode") == "true");
    //Cambio de estatus de tema oscuro
    $("#dark-mode-check").on("change", function (e) {
        if (getCookie("dark-mode") == null) {
            setCookie("dark-mode", true, 365);
            darkMode(true);
        } else if (getCookie("dark-mode") == "true") {
            darkMode(false);
            eraseCookie("dark-mode");
        }
        e.stopImmediatePropagation();
    });
    $("#info-rfc").remove();

    $.fn.materializeInputs = function (selectors) {

        // default param with backwards compatibility
        if (typeof (selectors) === 'undefined') selectors = "input, textarea, select";

        // attribute function
        function setInputValueAttr(element) {
            element.setAttribute('value', element.value);
        }

        // set value attribute at load
        this.find(selectors).each(function () {
            setInputValueAttr(this);
        });

        // on keyup and change
        this.on("keyup change", selectors, function () {
            setInputValueAttr(this);
        });
    };

    /** par
    * Material Inputs
    */
    $('body').materializeInputs();
});

$.ajaxSetup({
    beforeSend: function () {
        showLoader();
    },
    complete: function () {
        hideLoader();
    }
});
var baseUrl = $("base").attr("href");

var colors = {
    1: "primary",
    2: "secondary",
    3: "info",
    4: "success",
    5: "warning",
    6: "danger",
    7: "gray",
    8: "olive",
    9: "purple",
    10: "navy"
};

function random(min, max) {
    return Math.round(Math.random() * (max - min) + min);
}


//Cambio de tema => Oscuro, Claro
function darkMode(enable) {
    if (enable) {
        $('body').addClass('dark-mode');
        $('.control-sidebar').addClass('control-sidebar-dark');
        $('.control-sidebar').removeClass('control-sidebar-light');
        $("#dark-mode-check").prop("checked", true);
        $(".image-pdf").removeClass("pdf-red-color");
    } else {
        $("#dark-mode-check").prop("checked", false);
        $('body').removeClass('dark-mode');
        $('.control-sidebar').removeClass('control-sidebar-dark');
        $('.control-sidebar').addClass('control-sidebar-light');
        $(".image-pdf").addClass("pdf-red-color");
    }
}

function callModal(modalTitle, modalBody, buttonText, fuctionButtonAccept) {
    var _modal = $(".shared-modal");
    _modal.find(".modal-title").html(modalTitle);
    _modal.find(".modal-body").html(modalBody);
    var button = _modal.find(".button-confirm-accept");
    button.html(buttonText);
    $(button).unbind("click");
    $(button).on("click", fuctionButtonAccept);
    _modal.modal("show");
}

//Muestra loader
function showLoader() {
    $(".preloader").show();
}
//Oculta loader
function hideLoader() {
    $(".preloader").hide();
}
//Muestra alerta de error
function errorAlert(message) {
    Swal.fire({
        title: 'Error',
        html: message,
        icon: 'error',
        confirmButtonText: 'Aceptar'
    });
}
//Muestra alerta de información
function infoAlert(message) {
    Swal.fire({
        title: 'Información.',
        html: message,
        icon: 'info',
        confirmButtonText: 'Aceptar'
    });
}
//Muestra alerta de exito
function successAlert(message) {
    Swal.fire({
        title: 'Éxito',
        html: message,
        icon: 'success',
        confirmButtonText: 'Aceptar'
    });
}
//Muestra alerta de confirmación
function confirmAlert(title, message, callbackFunction, cancelFunction = null) {
    Swal.fire({
        title: title,
        html: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#691C32',
        cancelButtonColor: '#D9D9D9',
        confirmButtonText: 'Aceptar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed)
            return callbackFunction();
        if (cancelFunction != null)
            return cancelFunction();
    });
}
//Valida si un elemento es nulo (falta completar)
function isNull(item) {
    return (item === ''
        || item === null
        || item == undefined
        || typeof item === 'undefined'
        || (Array.isArray(item) && item.length == 0));
}
//Petición ajax simple
function ajaxRequest(url, type, onSuccessFunction, dataObj = {}) {
    $.ajax({
        type: type,
        url: baseUrl + url,
        data: dataObj,
        beforeSend: function () {
            showLoader();
        },
        success: onSuccessFunction,
        complete: function () {
            hideLoader();
        }
    });
}
//Inicializa un DataTable mediante su ID
function DataTableInitialize(tableId) {
    $("#" + tableId).DataTable({
        "destroy": true,
        "paging": false,
        "lengthChange": false,
        "searching": false,
        "ordering": false,
        "info": false,
        "autoWidth": false,
        "responsive": true,
    });
}
//Cookies para cambio de tema
function setCookie(key, value, expiry) {
    var expires = new Date();
    expires.setTime(expires.getTime() + (expiry * 24 * 60 * 60 * 1000));
    document.cookie = key + '=' + value + ';expires=' + expires.toUTCString();
}
//Cookies para cambio de tema
function getCookie(key) {
    var keyValue = document.cookie.match('(^|;) ?' + key + '=([^;]*)(;|$)');
    return keyValue ? keyValue[2] : null;
}
//Cookies para cambio de tema
function eraseCookie(key) {
    var keyValue = getCookie(key);
    setCookie(key, keyValue, '-1');
}
//Mantiene seleccionado el menú en el que se encuentra navegando actualmente el usuario
function setOpenMenu(menu, submenu) {
    $("#" + menu).addClass("menu-is-opening");
    $("#" + menu).addClass("menu-open");
    $("#" + submenu).addClass("active");
}
//Trim para remover espacios
function removeSpaces(string) {
    return string.split(' ').join('');
}
//Codificar elementos con tags (HTML, XML)
function encode(string) {
    $("div.encoding").text(string);
    var encoded = $("<div>").text(string).html();
    $("div.encoding").text(null);
    return encoded;
}
//Expresion regular para validar UUID (Guid, UNIQUEIDENTIFIER)
function validaUUID(uuid) {
    var expregUUID = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
    if (uuid.length == 36)
        if (expregUUID.test(uuid))
            return true;
    return false;
}
//Seleccionar contenido de elemetos tipo Input al hacer focus
$(document).on("focus", ".select-on-focus", function (e) {
    e.preventDefault();
    $(this).select();
    e.stopImmediatePropagation();
});

$(document).on("change", "input.input-uuid", function (e) {
    e.preventDefault();
    if (!validaUUID($(this).val().trim())) {
        $(this).addClass("is-invalid");
        $(this).removeClass("is-valid");
    } else {
        $(this).removeClass("is-invalid");
        $(this).addClass("is-valid");
    }
    e.stopImmediatePropagation();
});
//Funciones para generar mask de formato numérico
function formatAmountNoDecimals(number) {
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(number)) {
        number = number.replace(rgx, '$1' + ',' + '$2');
    }
    return number;
}
function formatAmount(number) {

    // remove all the characters except the numeric values
    number = number.replace(/[^0-9]/g, '');

    // set the default value
    if (number.length == 0) number = "0.00";
    else if (number.length == 1) number = "0.0" + number;
    else if (number.length == 2) number = "0." + number;
    else number = number.substring(0, number.length - 2) + '.' + number.substring(number.length - 2, number.length);

    // set the precision
    number = new Number(number);
    number = number.toFixed(2);    // only works with the "."

    // change the splitter to ","
    //number = number.replace(/\./g, ',');

    // format the amount
    x = number.split(',');
    x1 = x[0];
    x2 = x.length > 1 ? ',' + x[1] : '';

    return formatAmountNoDecimals(x1) + x2;
}

function newGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function showFile(base64URL, showInModal = false) {
    if (!showInModal) {
        var binary = atob(base64URL.replace(/\s/g, ''));
        var len = binary.length;
        var buffer = new ArrayBuffer(len);
        var view = new Uint8Array(buffer);
        for (var i = 0; i < len; i++) {
            view[i] = binary.charCodeAt(i);
        }

        // create the blob object with content-type "application/pdf"               
        var blob = new Blob([view], { type: "application/pdf" });
        var fileURL = URL.createObjectURL(blob);
        window.open(fileURL);
    }
    else {
        $("#pdfDocument").prop("src", base64URL);
        $("#ModalDocumento").modal("show");
    }
}
$(document).on("keypress", ".numberonly", function (e) {
    var charCode = (e.which) ? e.which : e.keyCode
    if (String.fromCharCode(charCode).match(/[^0-9]/g))
        return false;
});

toastr.options = { "positionClass": "toast-bottom-right" };
const toast = {
    error: (message) => { return toastr.error(message); },
    success: (message) => { return toastr.success(message); },
}

/**
 * Renders an empty-state callout into a container.
 * @param {string|jQuery} target
 * @param {object}        [opts]
 * @param {string}        [opts.message="No se han encontrado registros."]
 * @param {string}        [opts.title]        - Optional bold heading
 * @param {string}        [opts.type="info"]  - "info" | "success" | "warning" | "danger"
 * @param {string}        [opts.icon="fas fa-inbox"] - FontAwesome class
 */
function emptyState(target, opts) {
    var cfg = $.extend({
        message: "No se han encontrado registros.",
        title: null,
        type: "info",
        icon: "fas fa-inbox"
    }, opts);

    var $callout = $('<div>').addClass("callout callout-" + cfg.type).css({ textAlign: "center", padding: "1.5rem 2.5rem", border: "none", borderLeft: "none", boxShadow: "none" });

    if (cfg.icon)
        $callout.append($('<i>').addClass(cfg.icon).css({ fontSize: "3rem", display: "block", marginBottom: "0.75rem" }));

    if (cfg.title)
        $callout.append($('<strong>').text(cfg.title).css({ display: "block", marginBottom: "0.25rem" }));

    $callout.append($('<p>').text(cfg.message));

    $(target).html($callout);
}