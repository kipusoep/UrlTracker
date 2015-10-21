var $tbSearch;
function pageLoad() {
    $("*[data-toggle='tooltip']").tooltip({ container: "body", html: true });

    $(".wrap input[type='hidden']").addClass("redirect-to").addClass("redirect-to-node");

    $.validator.addMethod("queryString", function (value) {
        return value == "" || /^([A-Za-z0-9\%\+\_\-\.\*]+(=[A-Za-z0-9\%\+\_\-\.\*]*)?(&[A-Za-z0-9\%\+\_\-\.\*]+(=[A-Za-z0-9\%\+\_\-\.\*]*)?)*)?$/.test(value);
    }, "Please enter a valid query string");

    // Initialize validation on the entire ASP.NET form.
    $("#form1").validate({
        onsubmit: false,
        errorClass: "help-inline",
        highlight: function (element, errorClass) {
            $(element).parents(".control-group").removeClass("success").addClass("error");
        },
        unhighlight: function (element, errorClass) {
            var $controlGroup = $(element).parents(".control-group");
            if ($controlGroup.hasClass("error"))
                $controlGroup.removeClass("error").addClass("success");
        },
        errorPlacement: function (error, element) {
            if ($(element).attr("type") == "hidden")
                error.insertAfter(element.parent());
            else
                error.insertAfter(element);
        }
    });
    var $oldUrl = $(".old-url");
    var $oldRegex = $(".old-regex");
    if ($oldUrl.length && $oldRegex.length) {
        $oldUrl.rules("add", {
            exactly_from_group: [1, ".old-url"],
            messages: {
                exactly_from_group: "Enter the old URL"
            }
        });
        $oldRegex.rules("add", {
            exactly_from_group: [1, ".old-url"],
            messages: {
                exactly_from_group: "or a Regex pattern"
            }
        });
    }
    else if ($oldUrl.length) {
        $oldUrl.rules("add", {
            required: true,
            messages: {
                required: "Enter the old URL"
            }
        });
    }
    else if ($oldRegex.length) {
        $oldRegex.rules("add", {
            required: true,
            messages: {
                required: "Enter a Regex pattern"
            }
        });
    }
    var $redirectToNode = $(".redirect-to-node");
    var $redirectToUrl = $(".redirect-to-url");
    if ($redirectToNode.length && $redirectToUrl.length) {
        $redirectToNode.rules("add", {
            exactly_from_group: [1, ".redirect-to"],
            messages: {
                exactly_from_group: "Select the redirect target"
            }
        });
        $redirectToUrl.rules("add", {
            exactly_from_group: [1, ".redirect-to"],
            messages: {
                exactly_from_group: "or fill in a redirect URL manually"
            }
        });
    }
    else if ($redirectToNode.length) {
        $redirectToNode.rules("add", {
            required: true,
            min: 0,
            messages: {
                required: "Select the redirect node",
                min: "Select the redirect node"
            }
        });
    }
    else if ($redirectToUrl.length) {
        $redirectToUrl.rules("add", {
            required: true,
            messages: {
                required: "Enter the redirect URL"
            }
        });
    }
    var $queryString = $(".query-string");
    if ($queryString.length) {
        $queryString.rules("add", {
            required: false
        });
        $queryString.rules("add", "queryString");
    }

    // Search for controls marked with the causesValidation flag 
    //  that are contained anywhere within elements marked as 
    //  validationGroups, and wire their click event up.
    $(".validationGroup .causesValidation").click(validateAndSubmit);
    // Select any input[type=text] elements within a validation group
    //  and attach keydown handlers to all of them.
    $(".validationGroup :text").keydown(function (evt) {
        // Only execute validation if the key pressed was enter.
        if (evt.keyCode == 13)
            validateAndSubmit(evt);
    });

    var $cbSelectAll = $("#cbSelectAll");
    if ($cbSelectAll.length) {
        $cbSelectAll.on("change", function () {
            $(".table input[type='checkbox']").prop("checked", $cbSelectAll.is(":checked"));
        });
    }

    $tbSearch = $("#tbSearch");
    if ($tbSearch.length) {
        var onchange = $tbSearch.attr("onchange");
        $tbSearch.removeAttr("onkeypress").focus();
        if (!$tbSearch.isBound("keydown", tbSearchOnKeyDown))
            $tbSearch.on("keydown", tbSearchOnKeyDown);
        if (!$tbSearch.isBound("keyup", tbSearchOnKeyUp))
            $tbSearch.on("keyup", tbSearchOnKeyUp);
    }

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandler);
}

function endRequestHandler(sender, args) {
    if (args.get_error() != undefined)
        alert(args.get_error());
}

var tbSearchOnChange, tbSearchTimeout;
function tbSearchOnKeyDown(e) {
    if (e.keyCode == 13)
        e.preventDefault();
    else {
        if (typeof tbSearchOnChange == "undefined") {
            tbSearchOnChange = $tbSearch.attr("onchange");
            $tbSearch.removeAttr("onchange");
        }
        if (typeof tbSearchTimeout != "undefined" && tbSearchTimeout)
            clearTimeout(tbSearchTimeout);
        tbSearchTimeout = setTimeout(function () {
            eval(tbSearchOnChange);
        }, 500);
    }
}

function tbSearchOnKeyUp(e) {
    if (e.keyCode == 27) { // Esc
        $tbSearch.val("").trigger("keydown");
    }
}

function validateAndSubmit(evt) {
    // Ascend from the button that triggered this click event 
    //  until we find a container element flagged with 
    //  .validationGroup and store a reference to that element.
    var $group = $(evt.currentTarget).parents(".validationGroup");

    var isValid = true;

    // Descending from that .validationGroup element, find any input
    //  elements within it, iterate over them, and run validation on 
    //  each of them.
    $group.find(":input").each(function (i, item) {
        if (!$(item).valid())
            isValid = false;
    });

    // If any fields failed validation, prevent the button"s click 
    //  event from triggering form submission.
    if (!isValid)
        evt.preventDefault();
}

function deleteSelectedClick() {
    var checkedCount = $(".table input[type='checkbox']:checked").length;
    if (checkedCount > 0)
        return confirm("Are you sure you want to delete " + checkedCount + " " + (checkedCount == 1 ? "entry" : "entries") + "?");
}

function clearNotFoundClick() {
    return confirm("Are you sure you want to delete all not found (404) entries?");
}

/*
 * Lets you say "exactly X inputs that match selector Y must be filled."
 *
 * The end result is that neither of these inputs:
 *
 *  <input class="productinfo" name="partnumber">
 *  <input class="productinfo" name="description">
 *
 *  ...will validate unless one of them is filled.
 *
 * partnumber:  {exactly_from_group: [1,".productinfo"]},
 * description: {exactly_from_group: [1,".productinfo"]}
 *
 */
jQuery.validator.addMethod("exactly_from_group", function (value, element, options) {
    var validator = this;
    var selector = options[1];
    var validOrNot = jQuery(selector, element.form).filter(function () {
        return validator.elementValue(this);
    }).length == options[0];

    if (!$(element).data('being_validated')) {
        var fields = jQuery(selector, element.form);
        fields.data('being_validated', true);
        fields.valid();
        fields.data('being_validated', false);
    }
    return validOrNot;
}, jQuery.format("Please fill exactly {0} of these fields."));

jQuery.fn.isBound = function (type, fn) {
    if (typeof this.data('events') == "undefined")
        return false;

    var data = this.data('events')[type];
    if (data === undefined || data.length === 0)
        return false;

    var bound = false;
    $.each(data, function (ix, el) {
        if (el.handler == fn)
            bound = true;
    });

    return bound;
};