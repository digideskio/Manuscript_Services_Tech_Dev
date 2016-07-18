$(document).ready(function () {
    $("#btnFetchId").click(function () {
        var manuscriptsID = $("#manuscriptsID").val();
        $.ajax(
        {
            method: "GET",
            url: AppPath + 'AssociateDashboard/FetchJob',
            contentType: "application/json; charset=utf-8",
            success: function (returnValue) {
                debugger;
                if (returnValue.returnValue = "true") {
                    alert(returnValue.message);
                    if (returnValue.ManuscriptID = 0) {
                        alert('Manuscript not found.');
                        window.open('/Manuscript', "_blank");
                    }
                    else {
                        window.open('/Manuscript/HomePage/' + returnValue.ManuscriptID, "_blank");
                    }
                    window.location.reload(true);
                }
                else {
                    alert(returnValue.message);
                }
            },
            error: function (xhr, exception) {
            }
        });
    });

});