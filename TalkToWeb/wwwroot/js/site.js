function TesteCors() {
    var tokenJTW = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im1pZ3VlbEB0ZXN0ZS5jb20uYnIiLCJzdWIiOiJkZGY0NzdiZi0yYzE3LTQzMTQtOTBjOC0wOGI4ZjkwOTJiY2UiLCJleHAiOjE2NDgwODYyNDN9.tX3e44_7IPM5K0ScTGYqdG7Vut0bUVlOO-mgxuzCu70";
    var service = "https://localhost:44354/api/message/8e822783-2a02-410b-b297-c65222b484e1/ddf477bf-2c17-4314-90c8-08b8f9092bce";
    $("#resultado").html("-------Solicitando------");
    $.ajax({
        url: service,
        method: "GET",
        crossDomain: true,
        headers: {"Accept": "application/json"},
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Bearer " + tokenJTW);
        },
        success: function (data, status, xhr) {
            $("#resultado").html(data);
            console.info(data);
        }

    });

}