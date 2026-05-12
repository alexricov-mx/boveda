$(document).ready(function () {

    var laurl = $('#urlConsultaGRM').val();

    $("#buscar").click(function () {

        if (document.getElementById('txttoken').value.length >= 6) {

            var url = laurl;
            var eltoken = $("#txttoken").val();
            var npagina = $("#txtinipage").val();
            var numreg = $("#txtnummos").val();
            var data = {
                AdministratorToken: eltoken,
                pageNum:npagina,
                pageSize:numreg
            };

           //showLoader();

            $.post(url, data).done(function (data) {
                $("#resultadosGRM").html(data);
                hideLoader();
            })
        }

        else {
            // if (!isNull(data.success) && !data.success)
            // return errorAlert(data.message);
            // alert('El campo debe tener 10 digitos.');
            var message = 'El campo de "No. Administrador" debe tener almenos 6 digitos.'
            swal({
                title: "¡Algo no está bien!",
                text: message,
                type: "error",
                showCancelButton: false,
                confirmButtonClass: "btn-success",
                confirmButtonText: "Ok",
                closeOnConfirm: false
            });
        }
    });

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        var pageNum = $(this).data("page_num");
        ProfilesTable(pageNum);
        e.stopImmediatePropagation();
    });
});  


function ProfilesTable(pageNum = 1) {
    var eltoken = $("#txttoken").val();
    $.ajax({
        type: "POST",
        url: "AdmonGRM/GetGRM",
        data: { pageNum: pageNum, AdministratorToken: eltoken },
        beforeSend: function () {
            showLoader();
        },
        success: function (data) {
            if (!isNull(data.success) && !data.success)
                return errorAlert(data.message);
            $("#resultadosGRM").empty();
            $("#resultadosGRM").append(data);
        },
        complete: function () {
            hideLoader();
        }
    });
}
