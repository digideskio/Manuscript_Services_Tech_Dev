$(document).ready(function () {

    jQuery.validator.addMethod(
        'date',
        function (value, element, params) {
            if (this.optional(element)) {
                return true;
            };
            var result = false;
            try {
                $.datepicker.parseDate('dd/mm/yy', value);
                result = true;
            } catch (err) {
                result = false;
            }
            return result;
        },
        ''
    );


    //book login script
    $('#ReceivedDateBook').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' });
    $("#btnBookReset").click(function () {
        var serviceType = $("#ServiceTypeID option:selected").text();
        if (serviceType == "Reviewer Suggestion") {
        }
        var url = AppPath + 'ManuscriptLogin/BookLogin';
        window.location.href = url;
    });
    $("#ddlBookTitle").change(function () {
        GetBookMasterInfo();
    });
    $(".editBookButton").click(function (event) {
        $('#IsNewEntry').prop('checked', false);
        //$("#btnBookLogin").val("Update");
        var id = ($(this).closest("tr").find("td").eq(0).text()).trim();
        if (id == 0 || id == '' || id == null) {
            var url = AppPath + 'ManuscriptLogin/BookLogin';
        } else {
            var url = AppPath + 'ManuscriptLogin/BookLogin?id=' + id + '&&jobtype=book';
        }

        window.location.href = url;

    });
    


    $('#IsNewEntry').click(function () {
        if ($(this).is(":checked")) {
            $("#btnBookLogin").val("Login");
        } else {
            if ($("#bookid").val() != 0) {
                $("#btnBookLogin").val("Update");
            }
        }
    });

    $('#IsNewEntry').change(function () {
        if ($(this).is(":checked")) {
            $("#btnBookLogin").val("Login");
        } else {
            if ($("#bookid").val() != 0) {
                $("#btnBookLogin").val("Update");
            }
        }
    });




    $("#BookAssociateName").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: AppPath + 'ManuscriptLogin/GetAssociateName',
                data: { searchText: request.term },
                dataType: "json",
                type: "GET",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.empname,
                            val: item.UserID
                        }
                    }))
                }
            });
        },
        select: function (e, i) {
            $("#BookAssociateName").val(i.item.val);
        },
        minLength: 1
    });
    //book login script
    
    if ($("#bookid").val() != 0 && $("#bookid").val() != null) {
        $("#btnBookLogin").val("Update");
        var selectedBook = $("#ddlBookTitle option:selected").val();
        if (selectedBook == "") {
            $('#txtFTPLink').empty();
            $('#txtGPUInformation').empty();
            $('#FTPLinkHyperlink').empty();
        } else {
            $.getJSON(AppPath + "ManuscriptLogin/GetBookLink", { "bookTitleId": selectedBook }, function (data) {
                if (data.FTPLink != "")
                    $('#txtFTPLink').html('<a id="FTPLinkHyperlink" href="' + data.FTPLink + '" target="_blank" style="text-decoration: underline;color: blue;">' + data.FTPLink + '</a>');
                $('#txtGPUInformation').html(data.GPUInformation);
            });
        }
    } else {
        $("#btnBookLogin").val("Login");
        $('#ReceivedDateBook').val('');
        $('#PageCount').val('');
        $('#txtFTPLink').empty();
        $('#txtGPUInformation').empty();
        $('#FTPLinkHyperlink').empty();
    }


    $("#ValidationSummaryClose").click(function () {
        $("#dvValidationSummary").css({ "display": "none" });
    });

    $('.datepicker1').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' }).datepicker("setDate", new Date());

    $("#btnBookExporttoExcel").click(function () {
        $("#myBookModal").modal();
        $('.datepicker1').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' }).datepicker("setDate", new Date());
    });
    $('.datepicker1').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' }).datepicker("setDate", new Date());
    $('#myBookModal').on('hidden.bs.modal', function () {
        $(this).find("input,textarea,select").val('').end();

    });

    $("#btnLogin").click(function () {
        var serviceType = $("#ServiceTypeID option:selected").text();
        if (serviceType !== "Manuscript Screening") {
            if ($("#TaskID").val() == null || $("#TaskID").val() == '') {
                alert('Please, Select Task');
                return false;
            }
        } else {
            $("#TaskID").val('');
        }
    });

    GetTaskTypeStatus();

    $("#ServiceTypeID").change(function () {
        GetTaskTypeStatus();
    });

    $('#myBookModal').on('shown.bs.modal', function () {
        $("#btnOk").click(function () {

            var FromDateValue = $.datepicker.parseDate("dd/mm/yy", $("#FromDate").val());
            var ToDateValue = $.datepicker.parseDate("dd/mm/yy", $("#ToDate").val());
            if (FromDateValue == null && ToDateValue == null) {
                alert('Please select Dates');
                return false;
            }
            if (FromDateValue == null) {
                alert('Please select From Date');
                return false;
            } else if (ToDateValue == null) {
                alert('Please select To Date');
                return false;
            }

            if (FromDateValue > ToDateValue) {
                alert("From date cannot be greater then To Date.");
                return false;
            }
            window.location.href = AppPath + "ManuscriptLogin/ManuscriptBookLoginExportResult?FromDate=" + $("#FromDate").val() + "&ToDate=" + $("#ToDate").val();
            $("#myBookModal").modal('hide');
        });

    });
});//ready function end

function GetTaskTypeStatus() {
    var serviceType = $("#ServiceTypeID option:selected").text();
    if (serviceType == "Manuscript Screening") {
        $("#TaskID").val('');
        $("#TaskID").prop("disabled", true);
        $("#StatusMasterTaskID").val('');
    } else if (serviceType == "Reviewer Suggestion") {
        $("#TaskID").prop("disabled", true);
        $("#TaskID").val($("#TaskID" + " option").filter(function () { return this.text == 'Not invited nor suggested through editorial system' }).val());
        $("#StatusMasterTaskID").val($("#TaskID" + " option").filter(function () { return this.text == 'Not invited nor suggested through editorial system' }).val());
    } else {
        $("#TaskID").prop("disabled", false);
        $("#StatusMasterTaskID").val('');
        $("#TaskID").val('');
    }
}

function GetBookMasterInfo() {
    var selectedBookTitle = $("#ddlBookTitle option:selected").val();
    if (selectedBookTitle == "") {
        $('#txtFTPPath').empty();
        $('#txtGPUInformation').empty();
        $('#FTPLinkHyperlink').empty();
    } else {
        $.getJSON(AppPath + "ManuscriptLogin/GetBookLink", { "bookTitleId": selectedBookTitle }, function (data) {
            if (data.FTPLink == null) {
                $('#txtFTPPath').empty();
                $('#FTPLinkHyperlink').empty();
            }
            else if (data.FTPLink != "")
                $('#txtFTPLink').html('<a id="FTPLinkHyperlink" href="' + data.FTPLink + '" target="_blank" style="text-decoration: underline;word-break: break-all;color: blue;">' + data.FTPLink + '</a>');
            $('#txtGPUInformation').html(data.GPUInformation);
        });
    }
}

