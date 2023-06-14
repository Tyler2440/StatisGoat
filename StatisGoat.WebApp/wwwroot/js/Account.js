function updateAccount() {
    var password = d3.select("#new-password")["_groups"][0][0]["value"];
    var reEnterPassword = d3.select("#re-enter-password")["_groups"][0][0]["value"];

    console.log(password)
    console.log(reEnterPassword)

    if (password != reEnterPassword) {
        d3.select("#password-match").text("Passwords do not match!");
    }
    else {
        d3.select("#password-match").text("");
    }
}

document.getElementById("account").addEventListener("submit", function (event) {
    event.preventDefault(); 

    var email = document.getElementsByName("Email")[0].value; 
    var first = document.getElementsByName("First")[0].value; 
    var last = document.getElementsByName("Last")[0].value; 
    var id = document.getElementsByName("Id")[0].value;

    $("#spinner").show();
    $.ajax({
        url: "/Account/Updates",
        type: "POST",
        data: {
            email: email,
            first: first,
            last: last,
            id: id
        },
        success: function (data) {
            d3.select("#account-updated").text("Account has been Updated!").attr("style", "color: green !important;");
            $("#spinner").hide()
        },
        error: function () {
        },
        complete: function () {
            $("#spinner").hide()
        }
    });
});

document.getElementById("passwords").addEventListener("submit", function (event) {
    event.preventDefault();

    var email = document.getElementsByName("Email2")[0].value;
    var currpassword = document.getElementById("curr-password").value;
    var newpassword = document.getElementById("new-password").value;
    var reenterPassword = d3.select("#re-enter-password")["_groups"][0][0]["value"];
    if (newpassword.length < 6 && reenterPassword.length < 6) {
        d3.select("#password-match").text("New password must be at least 6 characters!").attr("style", "color: red !important;");
    }
    else if (newpassword == reenterPassword) {
        $("#pass-spinner").show();

        $.ajax({
            url: "/Account/UpdatePasswords",
            type: "POST",
            data: {
                email: email,
                currpassword: currpassword,
                newpassword: newpassword,
                reenterPassword: reenterPassword
            },
            success: function (data) {
                if (data.success) {
                    d3.select("#password-match").text("Passwords Updated!").attr("style", "color: green !important;");
                }
                else {
                    d3.select("#password-match").text("Current Password is wrong!").attr("style", "color: red !important;");
                }
                $("#pass-spinner").hide()
            },
            error: function () {
            },
            complete: function () {
                $("#pass-spinner").hide()
            }
        });
    }
    else if (newpassword != reenterPassword) {
        d3.select("#password-match").text("New passwords do not match!").attr("style", "color: red !important;");
    }
});