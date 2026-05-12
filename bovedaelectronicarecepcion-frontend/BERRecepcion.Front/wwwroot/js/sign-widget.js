var variablesfirma = ({
    paqueteSeleccionado: [],
    documentos: [],
    headers: [{ text: "titulo", value: "titulo" }],
    datos: [],
    dialogRechazo: false,
    dialogFirma: false,
    dialogEstadoFirma: false,
    valid: false,
    motivo: "",
    firmantes: [],
    firmanteSeleccionado: null,
    visibleBotones: true,
    tab: null,
    btnfirma: false,
    btnrechazar: false,
    reglasFileInput: [
        (value) =>
            !value || value.size < 2000000 || "El archivo no debe pasar 2 MB!",
    ],
    showContrasenia: false,
    firmaCert: null,
    b64Cert: null,
    x509: null,
    firmaKey: null,
    keyPrivada: null,
    rfcFirma: null,
    passKey: "",
    validacion: false,
    paqueteId: null,
    componenteId: null,
    cargando: true,
    muestraFooter: true,
    muestraDocFooter: true,
    Pagina: 0,
    Cantidad: 0,
    Busqueda: "",
    loadprimero: 0,
    numPag: 0,
    totalpaquetes: 0,
    search: "",
    options: {},
    varbusqueda: 0,
    tabbusq: null,
    itemtabselect: 0,
    itemstab: [
        { index: 0, name: "Pendiente de firma" },
        { index: 1, name: "Firmado" },
        { index: 2, name: "Rechazado" },
        { index: 3, name: "Paquete cancelado" },
    ],

    EdoPaquete: "",
    validacionOCSP: false,
});

function validaFiel(e, t, n, r) {
    if (!this.validaEntradasCertificado(n, r)) {
        return;
    }
    if (!this.validaEntradasFirma(e, t, "...", r)) {
        return;
    }
    const i = new FileReader();
    i.onload = function (n) {
        const s = new Uint8Array(i.result);
        let o = "";
        for (let u = 0; u < s.byteLength; u++) {
            o += String.fromCharCode(s[u]);
        }
        const a = rstrtohex(o);
        const f = KJUR.asn1.ASN1Util.getPEMStringFromHex(a, "CERTIFICATE");
        const l = new X509();
        l.readCertPEM(f);
        const c = l.subjectPublicKeyRSA.n;
        if (typeof c === "undefined") {
            r(27);
            return;
        }
        const h = new FileReader();
        h.onload = function (e) {
            const n = new Uint8Array(h.result);
            let i = "";
            for (let s = 0; s < n.byteLength; s++) {
                i += String.fromCharCode(n[s]);
            }
            const o = rstrtohex(i);
            const u = KJUR.asn1.ASN1Util.getPEMStringFromHex(
                o,
                "ENCRYPTED PRIVATE KEY"
            );
            try {
                const a = KEYUTIL.getKey(u, t, "PKCS8PRV");
                t = null;
                const f = a.n;
                if (!c.equals(f)) {
                    r(28);
                    return;
                }
            } catch (e) {
                if (e.indexOf("malformed format: SEQUENCE(0).items != 2") !== -1) {
                    r(24);
                    return;
                } else if (e === "malformed plain PKCS8 private key(code:001)") {
                    r(25);
                    return;
                } else {
                    r(24);
                    return;
                }
            }
            r(0, l);
        };
        h.readAsArrayBuffer(e.files[0]);
    };
    i.readAsArrayBuffer(n.files[0]);
}

function firmaCadena(e, t, n, r) {
    if (!this.validaEntradasFirma(e, t, n, r)) {
        return;
    }
    const i = new FileReader();
    i.onload = function (e) {
        const s = new Uint8Array(i.result);
        let o = "";
        for (let u = 0; u < s.byteLength; u++) {
            o += String.fromCharCode(s[u]);
        }
        const a = rstrtohex(o);
        const f = KJUR.asn1.ASN1Util.getPEMStringFromHex(
            a,
            "ENCRYPTED PRIVATE KEY"
        );
        let l = "";
        try {
            const c = KEYUTIL.getKey(f, t, "PKCS8PRV");
            t = null;
            const h = c.signString(n, "sha1");
            l = hex2b64(h);
        } catch (e) {
            if (e.indexOf("malformed format: SEQUENCE(0).items != 2") !== -1) {
                r(24);
                return;
            } else if (e === "malformed plain PKCS8 private key(code:001)") {
                r(25);
                return;
            } else {
                r(24);
                return;
            }
        }
        r(0, l);
    };
    i.readAsArrayBuffer(e.files[0]);
}

function validaEntradasCertificado(e, t) {
    if (typeof t !== "function") {
        throw "Se requiere una función callback al invocar el método firmar()";
    }
    if (typeof e === "undefined" || e === null) {
        t(15);
        return false;
    }
    if (typeof e.files === "undefined") {
        t(16);
        return false;
    }
    if (e.files.length === 0) {
        t(26);
        return false;
    }
    return true;
}

function validaEntradasFirma(e, t, n, r) {
    if (typeof r !== "function") {
        throw "Se requiere una función callback al invocar el método firmar()";
    }
    if (typeof e === "undefined" || e === null) {
        r(11);
        return false;
    }
    if (typeof e.files === "undefined") {
        r(12);
        return false;
    }
    if (typeof t === "undefined" || t === null) {
        r(13);
        return false;
    }
    if (typeof n === "undefined") {
        r(14);
        return false;
    }
    if (e.files.length === 0) {
        r(21);
        return false;
    }
    if (t === "") {
        r(22);
        return false;
    }
    if (n === "") {
        r(23);
        return false;
    }
    return true;
}

function leeCertificado(e, t) {
    if (!this.validaEntradasCertificado(e, t)) {
        return;
    }
    const n = new FileReader();
    n.onload = function () {
        const r = new Uint8Array(n.result);
        let i = "";

        for (let s = 0; s < r.byteLength; s++) {
            i += String.fromCharCode(r[s]);
        }
        const o = rstrtohex(i);
        const u = KJUR.asn1.ASN1Util.getPEMStringFromHex(o, "CERTIFICATE");

        const a = new X509();
        a.readCertPEM(u);

        const f = a.getPublicKey().n;

        if (typeof f === "undefined") {
            t(27);
            return;
        }

        t(0, a);
    };
    n.readAsArrayBuffer(e.files[0]);
}

function obtenRfc(e) {
    const t = e.getSubjectString();
    const n = t.match(
        /\/uniqueIdentifier=[A-Z,\u00D1,\u00F1,&]{3,4}[0-9]{2}[0-1][0-9][0-3][0-9][A-Z,0-9]?[A-Z,0-9]?[0-9,A-Z]?[\s]{0,1}\//
    );

    if (n !== null && n.length === 1) {
        const r = n[0].match(
            /[A-Z,\u00D1,\u00F1,&]{3,4}[0-9]{2}[0-1][0-9][0-3][0-9][A-Z,0-9]?[A-Z,0-9]?[0-9,A-Z]?/
        );
        if (r.length > 0) {
            return r[0];
        } else {
            return "";
        }
    } else {
        return "";
    }
}

function obtenNumSerie(e) {
    const t = e.getSerialNumberHex();
    let n = 1;
    let r = "";
    while (n < t.length) {
        const i = t.charAt(n);
        r = r + i;
        n = n + 2;
    }
    return r;
}

function obtenDateInicial(e) {
    const t = e.getNotBefore();
    const n = parseInt(t.substring(0, 2)) + 2e3;
    const r = parseInt(t.substring(2, 4)) - 1;
    const i = parseInt(t.substring(4, 6));
    const s = new Date(n, r, i);
    return s;
}

function obtenDateFinal(e) {
    const t = e.getNotAfter();
    const n = parseInt(t.substring(0, 2)) + 2e3;
    const r = parseInt(t.substring(2, 4)) - 1;
    const i = parseInt(t.substring(4, 6));
    const s = new Date(n, r, i, 23, 59, 59);
    return s;
}

function esVigente(e) {
    const t = this.obtenDateInicial(e);
    const n = this.obtenDateFinal(e);
    const r = new Date();
    if (r >= t && r <= n) {
        return true;
    } else {
        return false;
    }
}

function obtenMensajeError(e) {
    let t;
    if (typeof e === "undefined" || e === null) {
        return "Desconocido";
    }
    switch (e) {
        case 11:
            t = "No se ha pasado un valor para filePrivateKey para llave privada";
            break;
        case 12:
            t = "No se ha pasado un input del tipo file para llave privada";
            break;
        case 13:
            t = "No se ha pasado un valor para contraseña";
            break;
        case 14:
            t = "No se ha pasado un valor para la cadena a firmar";
            break;
        case 15:
            t = "No se ha pasado un valor para fileCertificado para Certificado";
            break;
        case 16:
            t = "No se ha pasado un input del tipo file para Certificado";
            break;
        case 21:
            t = "Selecione una clave privada";
            break;
        case 22:
            t = "Escriba la contraseña de la clave privada";
            break;
        case 23:
            t = "Introduza la información a firmar";
            break;
        case 24:
            t = "La clave privada no es válida";
            break;
        case 25:
            t = "La contraseña no es válida";
            break;
        case 26:
            t = "Seleccione un certificado";
            break;
        case 27:
            t = "El certificado no es válido";
            break;
        case 28:
            t = "El certificado no corresponde con la llave privada";
            break;
        case 29:
            t = "El RFC no es válido";
            break;
        case 30:
            t = "El certificado no esta vigente";
            break;
        case 31:
            t = "No es un archivo key válido";
            break;
        case 32:
            t = "El RFC del certificado no coincide con el RFC del firmante asignado";
            break;
        default:
            t = "Ocurrió una condición no válida";
            break;
    }
    if (
        document.characterSet.toUpperCase() === "ISO-8859-1" ||
        document.characterSet.toUpperCase() === "WINDOWS-1252"
    ) {
        const n = decodeURIComponent(escape(t));
        return n;
    } else {
        return t;
    }
}

let htmlText = `<div class="modal fade" style="display:none !important;" id="modalFirma" data-backdrop="static" data-keyboard="false" tabindex="-1" role="dialog" aria-labelledby="modalFirma" aria-hidden="true">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h3 class="modal-title text-muted">
                    <i class="fas fa-signature mr-2"></i>Información para la firma
                </h3>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <div class="row">
                    <div class="col-12">
                        <div class="card card-olive card-outline">
                            <div class="card-header">
                                <h5 class="card-title text-muted">
                                    <i class="fas fa-certificate mr-1"></i> Certificado (.cer)
                                </h5>
                            </div>
                            <div class="card-body box-profile">
                                <button type="button" class="btn btn-sm btn-outline-info" id="formFileCer">
                                    <i class="fas fa-upload mr-2"></i><span class="cerLabel" style="word-break: break-all;"> archivo.cer</span>
                                </button>
                                <input type="file" id="fileCert" accept=".cer" class="d-none">
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-12">
                        <div class="card card-olive card-outline">
                            <div class="card-header">
                                <h5 class="card-title text-muted">
                                    <i class="fas fa-key mr-1"></i> Clave privada (.key)
                                </h5>
                            </div>
                            <div class="card-body box-profile">
                                <button type="button" class="btn btn-sm btn-outline-info mr-1" id="formFileKey">
                                    <i class="fas fa-upload mr-2"></i> <span class="keyLabel" style="word-break: break-all;"> archivo.key</span>
                                </button>
                                <input type="file" id="fileKey" accept=".key" class="d-none">
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-12">
                        <div class="card card-olive card-outline">
                            <div class="card-header">
                                <h5 class="card-title text-muted">
                                    <i class="fas fa-user-lock mr-1"></i> Contraseña de clave privada
                                </h5>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-6 col-md-6 col-sm-12">
                                        <div class="input-group">
                                            <div class="input-group-prepend text-center">
                                                <span class="input-group-text"><i class="fas fa-lock"></i></span>
                                            </div>
                                            <input type="password" id="contrasena" name="pass" class="form-control form-control-sm" placeholder="Introduzca su contraseña" />
                                        </div>
                                    </div>
                                    <div class="col-lg-6 col-md-6 col-sm-12">
                                        <label id="cerRFC" class="text-muted mt-3 mt-md-1 mt-lg-1">

                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-sm btn-outline-danger float-right mr-1" data-dismiss="modal"  style="width:125px;">
                        <i class="fas fa-times mr-1"></i> Cancelar
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-success" id="btnFirmar" style="width:125px;">
                        <i class="fas fa-pencil-alt mr-1"></i>Firmar
                    </button>
                </div>
            </div>
        </div>
    </div>
</div>`;

let progressModal = function (model) {
    let strModalStart = `<div class="modal fade" id="modalFirmaLoading" style="display:none !important;" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <div class="alert alert-default-warning w-100">
                    <h5><i class="icon fas fa-exclamation-triangle mr-1"></i>¡Atención! <small>Si cierras tu navegador, pierdes conexion de internet o apagas tu computadora durante el proceso de firmado, este quedará incompleto.</small></h5>
                </div>

            </div>
            <div class="modal-body">
                <div class="row">
                    <div class="col-lg-3 col-md-5 col-sm-12">
                        <h6>Progreso total de firmado: </h6>
                    </div>
                    <div class="col-lg-9 col-md-7 col-sm-12">
                        <div class="progress">
                            <div class="progress-bar bg-gradient-success progress-bar-striped" role="progressbar" id="total-progress-bar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
                            </div>
                        </div>
                    </div>
                </div>
                <div class="card mt-2">
                    <div class="card-header border-transparent">
                        <h3 class="card-title">Progreso de firmado</h3>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive" id="table-progress">`;

    let strModalEnd = `</div>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button class="btn btn-sm btn-outline-secondary" id="btn-close-progress-modal" data-dismiss="modal" disabled>Cerrar</button>
            </div>
        </div>
    </div>
</div>`;
    let progressTableStart = `<table class="table m-0" style="overflow-y: scroll;">
        <thead>
            <tr>
                <th>Descripción</th>
                <th>Progreso</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody>`;
    let progressTableEnd = `</tbody> </table>`;
    let strString = "";
    $.each(model, function (index, item) {
        strString += `<tr>
            <td>${item.Descripcion}</td>
            <td>
                <div class="progress" style="margin-top: 6px;">
                    <div class="progress-bar bg-gradient-success progress-bar-striped" role="progressbar" id="progress_bar_${item.ItemID}" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
                    </div>
                </div>
            </td>
            <td><span id="progress_status_${item.ItemID}" class="badge badge-warning">Pendiente</span></td>
        </tr>`;
    });
    let table = progressTableStart + strString + progressTableEnd;
    return strModalStart + table + strModalEnd;
}

var documentList = [];
var _position = 1;
var _urlFirmar;
var _urlCompletarFirma;
var listLength = 0;
var totalProgress = 0;
var errors = false;
var info = false;
var a;
var _reloadTableName;

//Función para validar si ya se tienen documentos seleccionados para firmar 
// => Implementar en el onclick del botón encargado de realizar dicha validacion EJ: onclick="validarInfo(event);"
var validarInfo = function (e) {
    e.preventDefault();
    return document.querySelectorAll('input[name="checkPDF"]:checked').length > 0 ? $(_modalSign).modal("show") : _infoAlert("Debe seleccionar al menos un documento para firmar.");
    e.stopImmediatePropagation();
}

let _modalSign = null;
let _modalProgress = null;
const template = document.createElement('template');
template.setAttribute("id", "template-modal");

const templateProgress = document.createElement('template');
template.setAttribute("id", "template-modal-progress");
class SignWidget extends HTMLElement {
    constructor() {
        super();
        this.shadowDOM = this.attachShadow({ mode: 'open' });
        _urlFirmar = this.getAttribute("data-urlFirmar");
        _urlCompletarFirma = this.getAttribute("data-urlCompletarFirma");
        _reloadTableName = this.getAttribute("data-reloadTableName");
    }
    connectedCallback() {
        template.innerHTML = htmlText;
        this.shadowDOM.appendChild(template.content.cloneNode(true));
        _modalSign = this.shadowDOM.querySelector("#modalFirma");

        $(_modalSign).on("hidden.bs.modal", function () {
            limpiarModalFirma();
        });

        this.shadowDOM.querySelector("#formFileCer").addEventListener("click", () => {
            document.getElementById('fileCert').click();
        });
        this.shadowDOM.querySelector("#formFileKey").addEventListener("click", () => {
            document.getElementById('fileKey').click();
        });
        this.shadowDOM.querySelector("#fileCert").addEventListener('change', (e) => {
            let fileSelected = document.getElementById('fileCert').files.length > 0 ? document.getElementById('fileCert').files[0].name : "Ningún archivo seleccionado";
            if (document.getElementById('fileCert').files.length > 0)
                ValidaOCSP(e, fileSelected);
            else {
                document.getElementById('cerRFC').innerHTML = '';
                document.getElementsByClassName('cerLabel')[0].textContent = fileSelected;
            }
        });
        this.shadowDOM.querySelector("#fileKey").addEventListener('change', (e) => {
            let fileSelected = document.getElementById('fileKey').files.length > 0 ? document.getElementById('fileKey').files[0].name : "Ningún archivo seleccionado";
            if (document.getElementById('fileKey').files.length > 0)
                ObtieneKey(e);
            document.getElementsByClassName('keyLabel')[0].textContent = fileSelected;
        });
        //ACCIÓN DE FIRMADO
        this.shadowDOM.querySelector("#btnFirmar").addEventListener("click", () => {
            var _validaArchivos = validaArchivosFirma();
            if (!_validaArchivos.isValid)
                return _infoAlert(_validaArchivos.message);
            var _data = document.querySelectorAll('input[name="checkPDF"]:checked');
            _data.forEach(function (item, index) {
                documentList.push(JSON.parse(item.getAttribute('data-model')));
            });

            variablesfirma.passKey = document.getElementById('contrasena').value;
            if (validaFirma()) {
                templateProgress.innerHTML = progressModal(documentList);
                this.shadowDOM.appendChild(templateProgress.content.cloneNode(true));
                _modalProgress = this.shadowDOM.querySelector("#modalFirmaLoading");
                firmar(documentList);
            }
        });
    }
}
window.customElements.define("sign-widget", SignWidget);

let firmar = async function (documentList) {
    listLength = documentList.length;
    document.getElementById('btnFirmar').disabled = true;
    $(_modalProgress).modal("show");
    await firmarDocumento(documentList[0], 0);
}

let firmarDocumento = async function (_document, _position) {
    try {
        _showLoader();
        ///PETICION DE FIRMA PARA SUBIR DOCUMENTO Y OBTENER HASH
        let _respFirma = await fetch(_urlFirmar, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(_document)
        });
        let _firmar = await _respFirma.json();
        _modalProgress.style.zIndex = '9999';
        document.querySelector(".preloader").style.opacity = .25;
        document.getElementById('progress_bar_' + _document.ItemID).style.width = '50%';
        if ((!_isNull(_firmar.info) && (_firmar.info && _firmar.success)))
            info = true;
        if (!_firmar.success) {
            changeStatus(_document.ItemID, _firmar.success);
            if ((_position + 1) < listLength)
                firmarDocumento(documentList[_position], _position += 1);
            else {
                document.getElementById('btn-close-progress-modal').disabled = false;
                limpiarModalFirma();
                _hideLoader();
                return _infoAlert(_firmar.message);
            }
            errors = true;
        } else {
            let infoFirmado = obtenerInformacionFirma(_firmar.data);
            ///COMPLETAR FIRMA PARA CAMBIO DE ESTATUS

            let _completarFirma = await fetch(_urlCompletarFirma, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(infoFirmado)
            });
            let _completar = await _completarFirma.json();

            if (!isNull(_completar.info) && (_completar.info && !_completar.success) && info == false)
                errors = true;
            totalProgress = Math.round((_position + 1) * 100 / listLength);
            document.getElementById('total-progress-bar').style.width = totalProgress + '%';
            document.getElementById('progress_bar_' + _document.ItemID).style.width = '100%';
            changeStatus(_document.ItemID, _completar.success);
            if ((_position + 1) < listLength) {
                _position += 1;
                firmarDocumento(documentList[_position], _position);
            } else {
                eval(`${_reloadTableName}()`);
                $(_modalSign).modal("hide");
                document.getElementById('btn-close-progress-modal').disabled = false;
                _modalProgress.style.zIndex = '1050';
                document.querySelector(".preloader").style.opacity = 1;
                setTimeout(function () {
                    document.body.classList.add('modal-open');
                }, 1000);
                if (errors)
                    return _infoAlert(_completar.message);
                _hideLoader();
                return _successAlert(_completar.message);
            }
        }
    } catch (e) {
        document.getElementById('btn-close-progress-modal').disabled = false;
    }
}

let obtenerInformacionFirma = function (data) {
    const infoFirmado = {
        Certificado: {
            Nombre: variablesfirma.firmaCert.name,
            NoSerie: obtenNumSerie(variablesfirma.x509),
            FechaInicio: obtenDateInicial(variablesfirma.x509),
            FechaFin: obtenDateFinal(variablesfirma.x509),
            Codificacion: "Base64",
        },
    };
    const f = a.n;
    const c = variablesfirma.x509.getPublicKey().n;
    if (!c.equals(f))
        _infoAlert(obtenMensajeError(28));
    else {
        const sig = new KJUR.crypto.Signature({
            alg: "SHA512withRSA",
        });
        sig.init(a);
        sig.updateString(data.hashOriginal);
        const hSigVal = sig.sign();

        const l = hex2b64(hSigVal); // El documento Firmado
        data.hashFirma = l;

        //var documentoFirma = {
        //    PaqueteId: data.paqueteDocumento.paqueteId,
        //    DocumentoId: data.paqueteDocumento.documentoId,
        //    AlgoritmoDescripcion: "SHA512withRSA",
        //    AlgoritmoResult: data.paqueteDocumento.hash,
        //    InfoFirmado: JSON.stringify(infoFirmado).replace(
        //        '"',
        //        '"'
        //    ),
        //    CertificadoB64: variablesfirma.b64Cert,
        //};

        data.Certificado = variablesfirma.b64Cert;

        variablesfirma.false;
        //data.documentoFirma = documentoFirma;
    }
    return data;
}

var _showLoader = function () {
    document.getElementsByClassName('preloader')[0].removeAttribute('style');
}

var _hideLoader = function () {
    document.getElementsByClassName('preloader')[0].style.display = 'none';
}

var _infoAlert = function (message) {
    Swal.fire({
        title: 'Información.',
        html: message,
        icon: 'info',
        confirmButtonText: 'Aceptar'
    });
}

var _successAlert = function (message) {
    Swal.fire({
        title: 'Éxito',
        html: message,
        icon: 'success',
        confirmButtonText: 'Aceptar'
    });
}

var changeStatus = function (itemId, status) {
    let element = document.getElementById("progress_status_" + itemId)
    element.classList.remove('badge-warning');
    let _class = status ? "badge-success" : "badge-danger";
    let _text = status ? "Firmado" : "Error";
    element.classList.add(_class);
    element.textContent = _text;
    //ProgressBar Class
    if (!status) {
        let elementProgress = document.getElementById("progress_bar_" + itemId);
        elementProgress.classList.remove("bg-gradient-success");
        elementProgress.classList.add("bg-gradient-danger");
    }
}

function limpiarModalFirma() {
    document.getElementsByClassName('keyLabel')[0].textContent = 'archivo.key';
    document.getElementsByClassName('cerLabel')[0].textContent = 'archivo.cer';
    document.getElementById('cerRFC').innerHTML = '';
    document.getElementById('fileCert').value = null;
    document.getElementById('fileCert').value = null;
    document.getElementById('contrasena').value = null;
    document.getElementById('btnFirmar').disabled = false;
}

function _isNull(item) {
    return (item === ''
        || item === null
        || item == undefined
        || typeof item === 'undefined'
        || (Array.isArray(item) && item.length == 0));
}

var validaArchivosFirma = function () {
    if (_isNull(document.getElementById('fileCert').value))
        return { isValid: false, message: "Debe seleccionar su archivo de certificado (.cer)" };
    if (_isNull(document.getElementById('fileKey').value))
        return { isValid: false, message: "Debe seleccionar su archivo de llave privada (.key)" };
    var documentsToSign = document.querySelectorAll('input[name="checkPDF"]:checked');
    if (documentsToSign.length == 0)
        return { isValid: false, message: "Debe seleccionar al menos un documento para firmar." };
    if (_isNull(document.getElementById('contrasena').value))
        return { isValid: false, message: "Por favor ingrese la contraseña de su clave privada." };
    return { isValid: true };
}

function ObtieneRFC(event) {
    if (typeof event !== "undefined") {
        if (variablesfirma.firmaCert.type === "application/x-x509-ca-cert") {
            var reader = new FileReader();
            reader.onload = function (event) {
                try {
                    const r = new Uint8Array(reader.result);
                    let i = "";

                    for (let s = 0; s < r.byteLength; s++) {
                        i += String.fromCharCode(r[s]);
                    }
                    const o = rstrtohex(i);
                    const u = KJUR.asn1.ASN1Util.getPEMStringFromHex(
                        o,
                        "CERTIFICATE"
                    );

                    const a = new X509();
                    a.readCertPEM(u);

                    if (esVigente(a)) {
                        const rfc = obtenRfc(a);
                        if (rfc === sessionStorage.getItem('userRFC')) {
                            variablesfirma.b64Cert = btoa(
                                String.fromCharCode(...new Uint8Array(reader.result))
                            );
                            document.getElementById('cerRFC').innerHTML = `<i class="fas fa-user-check mr-2"></i> ${rfc}`;
                            variablesfirma.x509 = a;
                            variablesfirma.rfcFirma = rfc;
                            variablesfirma.validacion = true;
                        } else {
                            variablesfirma.validacion = false;
                            variablesfirma.firmaCert = null;
                            _infoAlert(obtenMensajeError(32));
                        }
                    } else {
                        variablesfirma.validacion = false;
                        variablesfirma.firmaCert = null;
                        _infoAlert(obtenMensajeError(30));
                    }
                } catch (error) {
                    variablesfirma.validacion = false;
                }
            };
            reader.readAsArrayBuffer(variablesfirma.firmaCert);
        } else {
            _infoAlert(obtenMensajeError(27));
            variablesfirma.firmaCert = null;
            variablesfirma.validacion = false;
        }
    } else {
        variablesfirma.rfcFirma = "";
        variablesfirma.validacion = false;
    }
}

function ValidaOCSP(event, fileName) {
    _showLoader();
    variablesfirma.firmaCert = event.target.files[0];
    if (typeof variablesfirma.firmaCert !== "undefined") {
        if (variablesfirma.firmaCert.type === "application/x-x509-ca-cert") {
            var formData = new FormData();
            formData.append("cerClientePath", variablesfirma.firmaCert);
            fetch('SignWidget/ValidaOCSP', {
                method: 'POST',
                body: formData
            }).then(resp => resp.json()).then(function (data) {
                _hideLoader();
                if (!_isNull(data.success) && data.success && data.isValid) {
                    document.getElementsByClassName('cerLabel')[0].textContent = fileName;
                    variablesfirma.validacionOCSP = data.isValid;
                    ObtieneRFC(event);
                }
                else {
                    document.getElementById('fileCert').value = null
                    document.getElementsByClassName('cerLabel')[0].textContent = 'archivo.cer';
                    _infoAlert(data.message);
                    variablesfirma.validacionOCSP = false
                }
            }).catch(function () {
                variablesfirma.validacionOCSP = false
                _hideLoader;
            });
        }
    }
}

function ObtieneKey(event) {
    if (typeof event !== "undefined" && event !== null) {
        variablesfirma.firmaKey = event.target.files[0];
        if (extensionValida(variablesfirma.firmaKey.name, "key")) {
            const h = new FileReader();
            h.onload = () => {
                try {
                    const n = new Uint8Array(h.result);
                    let i = "";

                    for (let s = 0; s < n.byteLength; s++) {
                        i += String.fromCharCode(n[s]);
                    }

                    const o = rstrtohex(i);
                    const u = KJUR.asn1.ASN1Util.getPEMStringFromHex(
                        o,
                        "ENCRYPTED PRIVATE KEY"
                    );

                    variablesfirma.keyPrivada = u;
                } catch (error) {
                    variablesfirma.validacion = false;
                    variablesfirma.firmaKey = null;
                }
            };
            h.readAsArrayBuffer(variablesfirma.firmaKey);
        } else {
            _infoAlert(FeaUtil.obtenMensajeError(31));
            variablesfirma.firmaKey = null;
            variablesfirma.validacion = false;
        }
    } else {
        //alert(FeaUtil.obtenMensajeError(31));
    }
}

function extensionValida(nombreArchivo, extValida) {
    let extension = nombreArchivo.replace(/^.*\./, "");

    if (extension == nombreArchivo) return false;
    else {
        extension = extension.toLowerCase();

        if (extension != extValida) return false;
        else return true;
    }
}

var validaFirma = function () {
    _showLoader();
    try {
        if (!variablesfirma.validacionOCSP) {
            variablesfirma.btnfirma = false;
            infoAlert("No es posible validar su certificado con el SAT o este se encuentra revocado. Intentar más tarde. ");
            _hideLoader();
            return false;
        }

        if (!variablesfirma.validacion) {
            variablesfirma.btnfirma = false;
            infoAlert("No se cumplen los requisitos minimos para realizar la firma.");
            _hideLoader();
            return false;
        }

        if (variablesfirma.rfcFirma == "") {
            variablesfirma.btnfirma = false;
            infoAlert(obtenMensajeError(29));
            _hideLoader();
            return false;
        }
        if (variablesfirma.passKey == "") {
            variablesfirma.btnfirma = false;
            infoAlert(obtenMensajeError(22));
            _hideLoader();
            return false;
        }
        a = KEYUTIL.getKey(
            variablesfirma.keyPrivada,
            variablesfirma.passKey,
            "PKCS8PRV"
        );
    } catch (e) {
        if (e.indexOf("malformed format: SEQUENCE(0).items != 2") !== -1) {
            variablesfirma.btnfirma = false; // habilitado
            errorAlert(obtenMensajeError(24));
        } else if (e === "malformed plain PKCS8 private key(code:001)") {
            variablesfirma.btnfirma = false; // habilitado            
            errorAlert(obtenMensajeError(25));
        } else {
            variablesfirma.btnfirma = false; // habilitado
            errorAlert(obtenMensajeError(24));
        }
        _hideLoader();
        document.getElementById('btnFirmar').disabled = false;
        $(_modalProgress).modal("hide");
        $(_modalSign).modal("show");
        return false;
    }
    return true;
}