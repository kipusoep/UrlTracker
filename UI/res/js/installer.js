var serviceUrl = "/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.Installer.UrlTrackerInstallerService.asmx";
var $urlTrackerLoader, $loadingIcon;
$(function () {
    $urlTrackerLoader = $("#urlTrackerLoader");
    $loadingIcon = $("#circularG");
    if ($urlTrackerLoader.length) {
        // Step 0; request the ASMX, because the VirtualPathProvider throws a NullRef sometimes
        $.ajax(
            serviceUrl,
            {
                type: "GET",
                success: function (data, textStatus, jqXHR) {
                    // Step 1; install DB
                    $urlTrackerLoader.append($("<p />").text("Installing the database table."));
                    $.ajax(
                        serviceUrl + "/installtable",
                        {
                            accepts: "application/json",
                            contentType: "application/json",
                            dataType: "json",
                            type: "POST",
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert("Ajax error: " + $.parseJSON(jqXHR.responseText));
                            },
                            success: function (data, textStatus, jqXHR) {
                                data = data.d;
                                showInfoErrorOrSuccess("Installing the database table failed:", "Installing the database table skipped:", data, "Table already exists.", "Installed the database table.");

                                // Step 2; install dashboard
                                $urlTrackerLoader.append($("<p />").text("Installing the dashboard."));
                                $.ajax(
                                    serviceUrl + "/installdashboard",
                                    {
                                        accepts: "application/json",
                                        contentType: "application/json",
                                        dataType: "json",
                                        type: "POST",
                                        error: function (jqXHR, textStatus, errorThrown) {
                                            alert("Ajax error: " + jqXHR.responseText);
                                        },
                                        success: function (data, textStatus, jqXHR) {
                                            data = data.d;
                                            showInfoErrorOrSuccess("Installing the dashboard failed:", "Installing the dashboard skipped:", data, "Dashboard is already installed.", "Installed the dashboard.");

                                            // Step 3; check HttpModule
                                            $urlTrackerLoader.append($("<p />").text("Checking the Http Module."));
                                            $.ajax(
                                                serviceUrl + "/checkhttpmodule",
                                                {
                                                    accepts: "application/json",
                                                    contentType: "application/json",
                                                    dataType: "json",
                                                    type: "POST",
                                                    error: function (jqXHR, textStatus, errorThrown) {
                                                        alert("Ajax error: " + $.parseJSON(jqXHR.responseText));
                                                    },
                                                    success: function (data, textStatus, jqXHR) {
                                                        data = data.d;
                                                        showErrorOrSuccess("The Http Module isn't installed:", data, "The Http Module is functional.");

                                                        // Step 4; check if old version exists
                                                        $urlTrackerLoader.append($("<p />").text("Checking if a previous version exists."));
                                                        $.ajax(
                                                            serviceUrl + "/hasoldversioninstalled",
                                                            {
                                                                accepts: "application/json",
                                                                contentType: "application/json",
                                                                dataType: "json",
                                                                type: "POST",
                                                                error: function (jqXHR, textStatus, errorThrown) {
                                                                    alert("Ajax error: " + jqXHR.responseText);
                                                                },
                                                                success: function (data, textStatus, jqXHR) {
                                                                    data = data.d;
                                                                    showErrorOrSuccess("Checking for previous version failed:", data, "Previous version checked succesfully.");
                                                                    if (data.toLowerCase() == "true") {

                                                                        // Step 5; migrate data of old version
                                                                        $urlTrackerLoader.append($("<p />").text("Previous version found! Would you like to migrate the existing data?"));
                                                                        var yesButton = $("<a />").attr("href", "#").text("Yes please").addClass("btn").addClass("btn-primary");
                                                                        yesButton.click(function () {
                                                                            yesButton.remove();
                                                                            noButton.remove();
                                                                            showLoadingIcon();
                                                                            $urlTrackerLoader.append($("<p />").addClass("info").text("Yes please"));
                                                                            $urlTrackerLoader.append($("<p />").text("Migrating existing data."));
                                                                            $.ajax(
                                                                                serviceUrl + "/migratedata",
                                                                                {
                                                                                    accepts: "application/json",
                                                                                    contentType: "application/json",
                                                                                    dataType: "json",
                                                                                    type: "POST",
                                                                                    error: function (jqXHR, textStatus, errorThrown) {
                                                                                        alert("Ajax error: " + $.parseJSON(jqXHR.responseText));
                                                                                    },
                                                                                    success: function (data, textStatus, jqXHR) {
                                                                                        data = data.d;
                                                                                        showErrorOrSuccess("Migrating data failed:", data, "Migrating succeeded, entries migrated: " + data);
                                                                                        showFinishedMessage();
                                                                                    }
                                                                                }
                                                                            );
                                                                        });
                                                                        $urlTrackerLoader.append(yesButton);
                                                                        var noButton = $("<a />").attr("href", "#").text("No thanks").addClass("btn");
                                                                        noButton.click(function () {
                                                                            yesButton.remove();
                                                                            noButton.remove();
                                                                            $urlTrackerLoader.append($("<p />").addClass("info").text("No thanks"));
                                                                            showFinishedMessage();
                                                                        });
                                                                        $urlTrackerLoader.append(noButton);
                                                                        hideLoadingIcon();
                                                                    }
                                                                    else {
                                                                        $urlTrackerLoader.append($("<p />").addClass("info").text("Previous version not found."));
                                                                        showFinishedMessage();
                                                                    }
                                                                }
                                                            }
                                                        );
                                                    }
                                                }
                                            );
                                        }
                                    }
                                );
                            }
                        }
                    );
                }
            }
        );
    }
});

function showErrorOrSuccess(errorPrefix, message, successText) {
    if (message.startsWith("error: ")) {
        $urlTrackerLoader.append($("<p />").addClass("error").css("font-weight", "bold").text(errorPrefix + " " + message.substring("error: ".length)));
    } else {
        $urlTrackerLoader.append($("<p />").addClass("success").css("font-weight", "bold").text(successText));
    }
}

function showInfoErrorOrSuccess(errorPrefix, infoPrefix, message, infoError, successText) {
    if (message.startsWith("error: ")) {
        var message = message.substring("error: ".length);
        var isInfo = message.startsWith(infoError);
        var stackTrace = "";
        if (message.indexOf(" | Stacktrace: ") > -1) {
            stackTrace = message.substring(message.indexOf(" | Stacktrace: ") + " | Stacktrace: ".length);
            message = message.substring(0, message.indexOf(" | Stacktrace: "));
        }
        $urlTrackerLoader.append($("<p />").addClass(isInfo ? "info" : "error").css("font-weight", "bold").html((isInfo ? infoPrefix : errorPrefix) + " " + message));
        if (stackTrace != "" && !isInfo)
            $urlTrackerLoader.append($("<p />").addClass(isInfo ? "info" : "error").html("Well, this is awkward... It seems an error has occurred :-(<br />Please <a href=\"http://our.umbraco.org/projects/developer-tools/301-url-tracker/version-2\" target=\"_blank\">post a topic at the forum</a>, including the message above, some information about your setup and the stacktrace below:<br /><br />Stacktrace: <br />" + stackTrace));
    } else {
        $urlTrackerLoader.append($("<p />").addClass("success").css("font-weight", "bold").html(successText));
    }
}

function hideLoadingIcon() {
    $loadingIcon.hide();
}

function showLoadingIcon() {
    $loadingIcon.show();
}

function removeLoadingIcon() {
    $loadingIcon.remove();
}

function showFinishedMessage() {
    $urlTrackerLoader.append($("<p />").css("font-weight", "bold").html("All done :-)<br />The Url Tracker interface can be found as a dashboard in the Content section.<br />Go to Content and look on the right for a tab called Url Tracker."));
    $urlTrackerLoader.append($("<p />").css("font-weight", "bold").text("Happy Url Tracking!"));
    removeLoadingIcon();
}

if (typeof String.prototype.startsWith != 'function') {
    // see below for better implementation!
    String.prototype.startsWith = function (str){
        return this.indexOf(str) == 0;
    };
}