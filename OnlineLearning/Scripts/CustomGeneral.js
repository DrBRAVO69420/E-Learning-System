function displayError(message) {
    $("#errorMessage").html("<i class='glyphicon glyphicon-warning-sign smallPaddingLeftRight'></i> " + message);
    $('#errorMessage').css('display', 'inline-block');
    $('html, body').animate({
        //scrollTop: $("#errorMessage").offset().top
        scrollTo: $("#errorMessage").position().top
    });
    $('#errorMessage').delay(3000).fadeOut(300);
}

function displaySuccess(message) {
    $("#successMessage").html("<i class='glyphicon glyphicon-ok smallPaddingLeftRight'></i> " + message);
    $('#successMessage').css('display', 'inline-block');
    $('html, body').animate({
        //scrollTop: $("#successMessage").offset().top
        scrollTo: $("#successMessage").position().top

    });
    $('#successMessage').delay(2000).fadeOut(300);
}

function displayProcessSpin() {
    $('#progressSpin').css('display', 'inline-block');
    $('html, body').animate({
        //scrollTop: $("#progressSpin").offset().top
        scrollTo: $("#progressSpin").position().top
    });
}

function hideProcessSpin() {
    $('#progressSpin').fadeOut();
}

function adjustWindowSize(wnd) {
    var opts = wnd.options;
    //ipad pro
    if ($(window).width() == 1024) {
        opts.width = "68%";
    }
    //laptop
    else if ($(window).width() > 1024) {
        opts.width = "50%";
    }
    else {
        //mobile phone
        opts.width = "95%";
    }
    opts.height = "auto";

    wnd.setOptions(opts);
}

 // set grid's height to auto when data bound on the grid
function Grid_onDataBound(gridId) {
    var grid = "#" + gridId + " .k-grid-content";
    $(grid).attr("style", "height:auto");  
}

// display ModelState error alert for create and update record
function error_handler(e) {
    if (e.errors) {
        console.log(e.errors);
        var message = "Failed\n";
        $.each(e.errors, function (key, value) {
            if ('errors' in value) {
                $.each(value.errors, function () {
                    message += "\u2022 " + this + "\n";
                });
            }
        });
        var errorMsg = document.getElementById('errorMessage');
        errorMsg.innerText = message;
        $('#errorMessage').css('display', 'block');
        $('#errorMessage').delay(10000).fadeOut(300); 
    }
}

function ShowSuccessMessage(divId) {
    var messageDivId = "#" + divId;
    window.scrollTo(0, 0);
    $(messageDivId).css('display', 'block');
    $(messageDivId).delay(1500).fadeOut(300);
}

// after error displayed, refresh the grid
function delete_sync_handler(e) {
    this.read();
}

function onRequestEnd(e) {
    if (e.type == "create" && !e.response.Errors) {
        ShowSuccessMessage('RecordCreatedSuccess');
    }

    if (e.type == "update" && !e.response.Errors) {
        displaySuccess('Updated Successfully');
    }

    if (e.type == "destroy" && !e.response.Errors) {
        displaySuccess('Deleted Successfully!');
    }
    $('#errorMessage').delay(10000).fadeOut(300); 
}

//close the popup window when user click cancel button in popup window
function CloseWindow(windowId) {
    var id = "#" + windowId;
    var wnd = $(id).data("kendoWindow");
    wnd.close();
}

function iterate(object) {
    var html = '<ul style="padding:0;">';
    if (object !== null && object != undefined) {
        if (object.length > 0) {
            object.forEach(function (data) {
                html += '<li><i class="glyphicon glyphicon-triangle-right" style="font-size:8px"></i> ' + data + '</li>';
            });
        } else {
            return '-';
        }
    } else {
        html += '<li>-</li>';
    }
    html += '</ul>';
    return html;
}

//if need to copy current page's url, pass in 'current' for textToClipboard parameter
function CopyToClipboard(textToClipboard) {
    var input = document.getElementById("toClipboard");
    if (textToClipboard == 'current') {
        textToClipboard = window.location.href.toString();
    } else {
        textToClipboard = window.location.protocol.toString() + "//" + window.location.host.toString() + textToClipboard;
    }

    var success = true;
    if (window.clipboardData) { // Internet Explorer
        window.clipboardData.setData("Text", textToClipboard);
    }
    else {
        // create a temporary element for the execCommand method
        var forExecElement = CreateElementForExecCommand(textToClipboard);

        /* Select the contents of the element
            (the execCommand for 'copy' method works on the selection) */
        SelectContent(forExecElement);

        var supported = true;

        // UniversalXPConnect privilege is required for clipboard access in Firefox
        try {
            if (window.netscape && netscape.security) {
                netscape.security.PrivilegeManager.enablePrivilege("UniversalXPConnect");
            }

            // Copy the selected content to the clipboard
            // Works in Firefox and in Safari before version 5
            success = document.execCommand("copy", false, null);
        }
        catch (e) {
            success = false;
        }

        // remove the temporary element
        document.body.removeChild(forExecElement);
    }

    if (success) {
        document.getElementById('linkCopiedModal').style.display = 'block';
        //alert ("The text is on the clipboard, try to paste it!");
    }
    else {
        alert("Your browser doesn't allow clipboard access!");
    }
}

function CreateElementForExecCommand(textToClipboard) {
    var forExecElement = document.createElement("div");
    // place outside the visible area
    forExecElement.style.position = "absolute";
    forExecElement.style.left = "-10000px";
    forExecElement.style.top = "-10000px";
    // write the necessary text into the element and append to the document
    forExecElement.textContent = textToClipboard;
    document.body.appendChild(forExecElement);
    // the contentEditable mode is necessary for the  execCommand method in Firefox
    forExecElement.contentEditable = true;

    return forExecElement;
}

function SelectContent(element) {
    // first create a range
    var rangeToSelect = document.createRange();
    rangeToSelect.selectNodeContents(element);

    // select the contents
    var selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(rangeToSelect);
}

function GetUserRole() {
    $.ajax({
        type: "GET",
        url: '/Account/GetLoginUserRole',
        success: function (response) {
            return response;
        }
    });
}

//used in table which date that need to be filtered
function dateFilter(element) {
    element.kendoDatePicker({
        format: "yyyy-MM-dd"
    });
}

//use in kendo grid table
function UserStatusFilter(e) {
    $.ajax({
        url: '/Account/GetUserStatusList',
        type: "GET",
        success: function (result) {
            e.kendoDropDownList({
                optionLabel: '- Select Status -',
                filter: "contains",
                dataSource: result
            });
        }
    });
}
//use in kendo grid table
function UserRoleFilter(e) {
    $.ajax({
        url: '/Account/GetUserRoleList',
        type: "GET",
        success: function (result) {
            e.kendoDropDownList({
                optionLabel: '- Select Role -',
                filter: "contains",
                dataSource: result
            });
        }
    });
}


//used in table which has class name
function ClassNameFilter(e) {
    $.ajax({
        url: '/Class/GetClassNameList',
        type: "GET",
        success: function (result) {
            e.kendoDropDownList({
                optionLabel: 'Please select...',
                dataTextField: 'Text',
                dataValueField: 'Value',
                filter: "contains",
                dataSource: result
            });
        }
    });
}
//used in table which has exam name
function ExamNameFilter(e) {
    $.ajax({
        url: '/Exam/TeacherGetExamNameList',
        type: "GET",
        success: function (result) {
            e.kendoDropDownList({
                optionLabel: 'Please select...',
                dataTextField: 'Text',
                dataValueField: 'Value',
                filter: "contains",
                dataSource: result
            });
        }
    });
}
//used in table which has student exam status (passed / failed)
function StudentExamStatusFilter(e) {
    $.ajax({
        url: '/Exam/GetStudentExamStatus',
        type: "GET",
        success: function (result) {
            e.kendoDropDownList({
                optionLabel: 'Please select...',
                filter: "contains",
                dataSource: result
            });
        }
    });
}
//used in exam history for student page, when reset button clicked, set dropdownlist value = "" and refresh the grid
function ResetFilter(dropdownId, gridId) {
    var dropdown = $('#' + dropdownId).data('kendoDropDownList');
    dropdown.value('');
    var gridid = '#' + gridId;
    var grid = $(gridid).data('kendoGrid');
    grid.dataSource.filter([]);
}







