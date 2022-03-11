function TesteCors() {
    var tokenJTW = "";
    var service = "https://localhost:44354/api/message/8e822783-2a02-410b-b297-c65222b484e1/ddf477bf-2c17-4314-90c8-08b8f9092bce";
    $("#resultado").html("-------Solicitando------");
    $.ajax({
        url: service,
        method: "GET",
        crossDomain: true,
        headers: {"Accept": "Application/json"},
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Bearer" + tokenJTW);
        },
        success: function (data, status, xhr) {
            $("#resultado").html(data);
        }

    });

}