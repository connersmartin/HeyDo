// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Your web app's Firebase configuration
var firebaseConfig = {
};
// Initialize Firebase
firebase.initializeApp(firebaseConfig);



$("#sendsms").click(function () {
    $.ajax({
        url: "Message/SendText",
        success: function () { alert("SMS Sent!"); }
    });
});

$("#sendemail").click(function () {
    $.ajax({
        url: "Message/SendEmail",
        success: function () { alert("Email Sent!"); }
    });
});

$("#adddata").click(function () {
    $.ajax({
        url: "Data/AddData",
        data: {
            uid: firebase.auth().currentUser.uid,
            auth: firebase.auth().currentUser.ra,
            dataType:"User"
        },
        success: function () { alert("Data Added!"); }
    });
});

$("#getdata").click(function () {
    $.ajax({
        url: "Data/GetData",
        data: {
            uid: firebase.auth().currentUser.uid,
            auth: firebase.auth().currentUser.ra
        },
        success: function () { alert("Data Gotten!"); }
    });
});

$("#create").click(function () {
    var log = $("#logCreate").val();
    var pas = $("#passCreate").val();
    firebase.auth().createUserWithEmailAndPassword(log, pas).then(function () {
            loginAPI();
        },
        function(error) {
            var errorCode = error.code;
            var errorMessage = error.message;
            alert(errorCode + " - " + errorMessage);
        });
});

$("#createLog").click(function () {
    $(".Login").hide();
    $(".Create").show();
});

$("#reallyLog").click(function () {
    $(".Login").show();
    $(".Create").hide();
});

$("#login").click(function () {
    var log = $("#logIn").val();
    var pas = $("#passIn").val();
    firebase.auth().signInWithEmailAndPassword(log, pas).then(function (result) {
        loginAPI();
    }).catch(function (error) {
        var errorCode = error.code;
        var errorMessage = error.message;
        alert(errorCode + " - " + errorMessage);
    });
});


function loginAPI() {
    $.ajax({
        url: "/Home/Dashboard",
        type: 'GET',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Accept", "application/json");
            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.setRequestHeader("Uid", firebase.auth().currentUser.uid);
            xhr.setRequestHeader("Token", firebase.auth().currentUser.ra);
        },
        error: function (ex) {
            alert("error? "+ex.status + " - " + ex.statusText);
        },
        success: function (data) {
            $(".log").hide();
            $(".Dashboard").html(data);
        }
    });
}

$("#logout").click(function () {
    firebase.auth().signOut().then(function () {
        alert("logged out");
        // Sign-out successful.
    }).catch(function (error) {
        alert("was not able to log out: " + error);
        // An error happened.
    });
});