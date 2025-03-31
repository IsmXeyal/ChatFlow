"use strict";

document.addEventListener("DOMContentLoaded", function () {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.start().catch(err => console.error(err.toString()));

    $(".disabled").attr("disabled", "disabled");

    document.getElementById("btnEnter").addEventListener("click", function (event) {
        const clientName = $("#txtName").val();
        connection.invoke("GetClientName", clientName)
            .catch(err => console.error(err.toString()));

        // Enable input and buttons by removing the 'disabled' class
        $("#txtRoomName").removeClass("disabled").prop("disabled", false);
        $("#btnCreate").removeClass("disabled").prop("disabled", false);  
        $("#btnJoin").removeClass("disabled").prop("disabled", false);
        $(".rooms").removeClass("disabled").prop("disabled", false);

        $("#btnEnter").prop("disabled", true);

        event.preventDefault();
    });

    document.getElementById("btnLogout").addEventListener("click", function (event) {
        const clientName = $("#txtName").val();

        connection.invoke("DisconnectUser", clientName)
            .catch(function (err) {
                console.error('Error notifying server about logout:', err);
            })
            .finally(function () {
                connection.stop().then(function () {
                    window.location.href = "/Home/Login";  // Redirect to login page
                }).catch(function (err) {
                    console.error('Error stopping SignalR connection:', err);
                });
            });

        event.preventDefault();
    });

    connection.on("ReceiveClientName", function (clientName) {
        $("#clientSituation").html(`${clientName} is connected...`);
        $("#clientSituation").fadeIn(1000, () => {
            setTimeout(() => {
                $("#clientSituation").fadeOut(1000);
            }, 3000);
        });
    });

    connection.on("UserDisconnected", function (clientName) {
        $("#clientSituation").removeClass("alert-success")
            .addClass("alert-danger")
            .html(`${clientName} is disconnected...`);
        $("#clientSituation").fadeIn(1000, () => {
            setTimeout(() => {
                $("#clientSituation").fadeOut(1000);
            }, 3000);
        });

        // Remove the disconnected user from the client list UI
        $('#_clients .users').each(function () {
            if ($(this).text() === clientName) {
                $(this).remove();
            }
        });
    });

    connection.on("GetClients", function (clients) {
        $("#_clients").html(""); // Clear list before updating

        $.each(clients, function (index, client) {
            // Check if user already exists
            if ($("#_clients .users").filter((_, el) => $(el).text() === client.userName).length === 0) {
                const user = $(".users").first().clone();
                user.removeClass("active");
                user.html(client.userName);
                user.addClass("users");

                user.on("click", function () {
                    $(".users").removeClass("active");
                    $(this).addClass("active");
                });
                $("#_clients").append(user);
            }
        });
    });

    connection.on("ReceiveMessage", (message, clientName) => {
        // Get the current date and time
        const now = new Date();
        const sentDate = now.toLocaleString();

        const _message = $(".message").clone();
        _message.removeClass("message");
        _message.find("p").html(message);
        _message.find("h5")[0].innerHTML = clientName;
        _message.find("small").html(sentDate);
        if (clientName === $(".users.active").first().html()) {
            _message.addClass("sent-message");
        } else {
            _message.addClass("received-message");
        }
        $(".messages").append(_message);
    });

    document.getElementById("btnSend").addEventListener("click", function (event) {
        const clientName = $(".users.active").first().html();
        const message = $("#txtMessage").val();
        const currentUserName = $("#txtName").val(); // Get the current user's name

        // Get the current date and time
        const now = new Date();
        const sentDate = now.toLocaleString();

        if (!message || !clientName) return; // Prevent sending empty messages

        if (clientName === currentUserName) {
            alert("You cannot send a message to yourself!"); // Show an alert if the user is trying to message themselves
            return;
        }

        connection.invoke("SendMessageAsync", message, clientName)
            .catch(err => console.error(err.toString()));

        const _message = $(".message").clone();
        _message.removeClass("message");
        _message.find("p").html(message);
        _message.find("h5")[0].innerHTML = clientName;
        _message.find("h5")[1].innerHTML = "You";
        _message.find("small").html(sentDate);
        _message.addClass("sent-message");
        $(".messages").append(_message);

        event.preventDefault();
    });

    let _groupName = "";
    document.getElementById("btnSendToGroup").addEventListener("click", function (event) {
        const message = $("#txtMessage").val();
        const currentUserName = $("#txtName").val();

        if (!message || !_groupName) return;

        // Get the current date and time
        const now = new Date();
        const sentDate = now.toLocaleString();

        connection.invoke("SendMessageToGroupAsync", message, _groupName)
            .catch(err => console.error(err.toString()));

        const _message = $(".message").clone();
        _message.removeClass("message");
        _message.find("p").html(message);
        _message.find("h5")[0].innerHTML = _groupName;
        _message.find("h5")[1].innerHTML = "You";
        _message.find("small").html(sentDate);
        _message.addClass("sent-message");
        $(".messages").append(_message);

        event.preventDefault();
    });

    connection.on("ReceiveGroupMessage", (message, clientName, groupName) => {
        const currentUserName = $("#txtName").val();

        if (clientName === currentUserName) return;
        // Get the current date and time
        const now = new Date();
        const sentDate = now.toLocaleString();

        const _message = $(".message").clone();
        _message.removeClass("message");
        _message.find("p").html(message);
        _message.find("h5")[0].innerHTML = groupName;
        _message.find("h5")[1].innerHTML = clientName;
        _message.find("small").html(sentDate);

        if (clientName === currentUserName) {
            _message.addClass("sent-message");
        } else {
            _message.addClass("received-message");
        }
        $(".messages").append(_message);
    });

    document.getElementById("btnCreate").addEventListener("click", function (event) {
        connection.invoke("AddGroup", $("#txtRoomName").val())
            .catch(err => console.error(err.toString()));

        event.preventDefault();
    });

    connection.on("Groups", (groups) => {
        let options = "";
        $.each(groups, function (index, group) {
            // Check if the group already exists in the dropdown
            if ($(".rooms option[value='" + group.groupName + "']").length === 0) {
                options += `<option value="${group.groupName}">${group.groupName}</option>`;
            }
        });
        $(".rooms").append(options);
    });

    document.getElementById("btnJoin").addEventListener("click", function (event) {
        let groupNames = [];
        $(".rooms option:selected").map((i, e) => {
            groupNames.push(e.value);
        });

        connection.invoke("AddClientToGroup", groupNames)
            .catch(err => console.error(err.toString()));

        event.preventDefault();
    });

    document.querySelector(".rooms").addEventListener("change", function () {
        let groupName = this.value;
        _groupName = groupName;
        connection.invoke("GetClientInGroup", groupName)
            .catch(err => console.error(err.toString()));
    });
});