﻿$(document).ready(function () {

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


    $(".editButton").click(function (event) {
        var id = ($(this).closest("tr").find("td").eq(11).text()).trim();
        if (id == 0 || id == '' || id == null) {
            var url = AppPath + 'ManuscriptLogin/JournalLogin';
        } else {
            var url = AppPath + 'ManuscriptLogin/JournalLogin?id=' + id + '&&jobtype=journal';
        }
        window.location.href = url;
    });

    $("#MSID").focusout(function () {
        $.get(AppPath + "ManuscriptLogin/GetManuscriptLoginedJobByMsid", { "msid": $("#MSID").val(), servicetype: $("#ServiceTypeID").val() }, function (data) {
            $("#ServiceTypeID").val($("#ServiceTypeID" + " option").filter(function () { return this.text == data.ServiceType }).val());
            $("#ddlJournalTitle").val($("#ddlJournalTitle" + " option").filter(function () { return this.text == data.JournalTitle }).val());
            $("#ArticleTitle").val(data.ArticleTitle);
            $("#SpecialInstruction").val(data.SpecialInstruction);
            var currentTime = new Date(parseInt(data.InitialSubmissionDate.substr(6)));
            var initialSubmissionDate = currentTime.getDate() + "/" + (currentTime.getMonth() + 1) + "/" + currentTime.getFullYear();
            $("#InitialSubmissionDate").val(initialSubmissionDate);
            $("#Associate").val(data.Associate);
            $.get(AppPath + "ManuscriptLogin/GetJournalLink", { "journalId": $("#ddlJournalTitle").val() }, function (link) {
                $('#JournalLink').html('<a href="' + link + '" target="_blank" style="text-decoration: underline;padding-left: 40px;word-break: break-all;color: blue;">' + link + '</a>');
            });
            if (data.ArticleTypeID != null) {
                $('#ddlArticleType').empty();
                $('#ddlArticleType').append($('<option>', {
                    value: data.ArticleTypeID,
                    text: data.ArticleTypeName
                }));
                $("#ddlArticleType").val($("#ddlArticleType" + " option").filter(function() { return this.text == data.ArticleTypeName }).val());
            }
            if (data.SectionID != null) {
                $('#ddlSectionType').append($('<option>', {
                    value: data.SectionID,
                    text: data.SectionName
                }));
            }
            $("#ddlSectionType").val($("#ddlSectionType" + " option").filter(function () { return this.text == data.SectionName }).val());
            GetTaskTypeStatus();
        });
        $.get(AppPath + "ManuscriptLogin/IsMsidAvaialable", { "msid": $("#MSID").val(), "serviceTypeStatusId": $("#ServiceTypeID").val() }, function (data1) {
            if (data1.toLocaleLowerCase() == "false") {
                $("#IsRevision").prop("disabled", false);
            } else {
                $("#IsRevision").prop("checked", false);
                $("#IsRevision").prop("disabled", true);
            }
        });
    });


    $("#btnReset").click(function () {
        var url = AppPath + 'ManuscriptLogin/JournalLogin';
        window.location.href = url;
    });
    

    $("#IsRevision").change(function () {
        if ($("#IsRevision").is(":checked")) {
            $("#btnLogin").val("Login");
            $("#ManuscriptFilePath").prop("readonly", false);
            $("#ManuscriptFilePath").click(function () {
                return true;
            });
            var msiddata = $("#MSID").val();
            var servicetype = $("#ServiceTypeID").val();
            if (servicetype == 6) {
                if ($(this).is(":checked")) {
                    $.get(AppPath + "ManuscriptLogin/CheckMSIDForRevision", { "msid": $("#MSID").val(), "serviceTypeStatusId": $("#ServiceTypeID").val() }, function (data1) {
                        if (data1 != "" || data1 != null) {
                            $("#MSID").val(msiddata + ".R" + data1);
                        } else {
                            $("#MSID").val();
                        }
                    });
                }
            } else {
                $.get(AppPath + "ManuscriptLogin/ValidateMsidIsOpen", { "msid": $("#MSID").val() }, function (data) {
                    if (data.toLocaleLowerCase() == "true") {
                        alert("You can not create revision for open manuscript.");
                        $("#IsRevision").prop("checked", false);
                        $("#IsRevision").prop("disabled", true);
                        return false;
                    }
                });
            }

        }
    });

    $('#InitialSubmissionDate').datepicker({ dateFormat: 'dd/mm/yy' });
    $('#ReceivedDate').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' });
    $("#ddlJournalTitle").change(function () {
        loadArticleTypeDDL();
        var selectedJournal = $("#ddlJournalTitle option:selected").val();
        if (selectedJournal == "") {
            $('#JournalLink').empty();
        } else {
            $.get(AppPath + "ManuscriptLogin/GetJournalLink", { "journalId": selectedJournal }, function (data) {
                $('#JournalLink').html('<a href="' + data + '" target="_blank" style="text-decoration: underline;padding-left: 40px;word-break: break-all;color: blue;">' + data + '</a>');
            });
        }
    });

    if ($("#CrestId").val() != 0 && $("#CrestId").val() != null) {
        $("#btnLogin").val("Update");
        var selectedJournal = $("#ddlJournalTitle option:selected").val();
        if (selectedJournal == "") {
            $('#JournalLink').empty();
        } else {
            $.get(AppPath + "ManuscriptLogin/GetJournalLink", { "journalId": selectedJournal }, function (data) {
                $('#JournalLink').html('<a href="' + data + '" target="_blank" style="text-decoration: underline;padding-left: 40px;word-break: break-all;color: blue;">' + data + '</a>');
            });
        }
    }

    if ($("#ArticleTitle").val() == '' && $("#CrestId").val() == 0) {
        $('#InitialSubmissionDate').val('');
    }

    $("#Associate").autocomplete({
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
                },
                error: function (response) {
                    //alert("Please,");
                },
                failure: function (response) {
                    //alert("Please,select journal title");
                }
            });
        },
        select: function (e, i) {
            $("#Associate").val(i.item.val);
        },
        minLength: 1
    });

    $("#ValidationSummaryClose").click(function () {
        $("#dvValidationSummary").css({ "display": "none" });
    });

    $("#btnExporttoExcel").click(function () {
        $("#myModal").modal();
        $('.datepicker1').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' }).datepicker("setDate", new Date());
    });
    $('.datepicker1').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' }).datepicker("setDate", new Date());
    $('#myModal').on('hidden.bs.modal', function () {
        $(this).find("input,textarea,select").val('').end();

    });

    $('.datepicker1').datepicker({ dateFormat: 'dd/mm/yy', maxDate: '0' }).datepicker("setDate", new Date());

    $("#btnLogin").click(function () {
        var serviceType = $("#ServiceTypeID option:selected").text();
        if (serviceType !== "Manuscript Screening") {
            if ($("#TaskID").val() == null || $("#TaskID").val() == '') {
                alert('Please, Select Task');
                return false;
            }
        }
        else {
            $("#TaskID").val('');
        }
        if (serviceType === "Manuscript Screening") {
            if ($("#ddlArticleType").val() == null || $("#ddlArticleType").val() == '') {
                alert('Please, select journal title and article type');
                return false;
            }
            if ($("#InitialSubmissionDate").val() == null || $("#InitialSubmissionDate").val() == '') {
                alert('Initial submission date can not be blank');
                return false;
            }
        }
    });

    GetTaskTypeStatus();

    $("#ServiceTypeID").change(function () {
        GetTaskTypeStatus();
    });
    $('#myModal').on('shown.bs.modal', function () {
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
            window.location.href = AppPath + "ManuscriptLogin/ManuscriptLoginExportResult?FromDate=" + $("#FromDate").val() + "&ToDate=" + $("#ToDate").val();
            $("#myModal").modal('hide');
        });

    });

});//ready function end


function GetTaskTypeStatus() {
    var serviceType = $("#ServiceTypeID option:selected").text();
    if (serviceType == "Manuscript Screening") {
        $("#TaskID").prop("disabled", true);
        $("#ArticleTypeAsterik").empty();
        $("#InitialSubmissionDateAsterik").empty();

        $("#ArticleTypeAsterik").append("<span>*</span>");
        $("#InitialSubmissionDateAsterik").append("<span>*</span>");
        $("#taskasterik").empty();
    } else {
        $("#TaskID").prop("disabled", false);
        $("#ArticleTypeAsterik").empty();
        $("#InitialSubmissionDateAsterik").empty();
        $("#taskasterik").empty();
        $("#taskasterik").append("<span>*</span>");

    }
}


function loadArticleTypeDDL() {
    var selectedJournal = $("#ddlJournalTitle option:selected").val();
    var articleDDL = $("#ddlArticleType");
    var sectionTypeDDL = $("#ddlSectionType");
    if (selectedJournal == 0 || selectedJournal == null || selectedJournal == "") {
        articleDDL.empty();
        articleDDL.append(new Option("Select-ArticleType", ""));
        sectionTypeDDL.empty();
        sectionTypeDDL.append(new Option("Select-Section", ""));
        return false;
    }
    $.ajax(
    {
        method: "GET",
        url: AppPath + "Manuscript/GetArticleType",
        contentType: "application/json; charset=utf-8",
        data: { journalMasterID: selectedJournal },
        success: function (data) {
            var drpCntnt = "";
            if (data != "") {
                var array = data.split('~');
                var po;
                if (array.length == 0) {
                    po = "No Journal available";
                } else {
                    articleDDL.empty();
                    articleDDL.append(new Option("Select-ArticleType", ""));
                    //articleDDL.append(new Option(" ", 0));
                    for (var i = 0; i < array.length - 1; i++) {
                        var temp = array[i].split('---');
                        articleDDL.append(new Option(temp[1], temp[0]));
                    }
                }
            } else {
                //alert("no Article");
                articleDDL.empty();
                articleDDL.append(new Option("Select-ArticleType", ""));
                $("#ddlArticleType option[value='']").attr('selected', true)
            }
        },
        error: function (xhr, exception) {
            alert("Error occured while fetching Article Type details.");
        }
    });

    $.ajax(
    {
        method: "GET",
        url: AppPath + "Manuscript/GetSectionType",
        contentType: "application/json; charset=utf-8",
        data: { journalMasterID: selectedJournal },
        success: function (data) {
            var drpCntnt = "";
            if (data != "") {
                var array = data.split('~');
                var po;
                if (array.length == 0) {
                    po = "No Journal available";
                } else {
                    sectionTypeDDL.empty();
                    sectionTypeDDL.append(new Option("Select-Section", ""));
                    for (var i = 0; i < array.length - 1; i++) {
                        var temp = array[i].split('---');
                        sectionTypeDDL.append(new Option(temp[1], temp[0]));
                    }
                }
            } else {
                //alert("no section");
                $("#ddlSectionType option[value='']").attr('selected', true)
                sectionTypeDDL.empty();
                sectionTypeDDL.append(new Option("Select-Section", ""));
            }
        },
        error: function (xhr, exception) {
            alert("Error occured while fetching Section details.");
        }
    });
};
