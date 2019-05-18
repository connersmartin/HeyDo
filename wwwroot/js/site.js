// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$("#sendsms").click(function () {
    $.ajax({
        url: "Home/SendText",
        success: function () { alert("SMS Sent!"); }
    });
});

$("#sendemail").click(function () {
    $.ajax({
        url: "Home/SendEmail",
        success: function () { alert("Email Sent!"); }
    });
});

$("#adddata").click(function () {
    $.ajax({
        url: "Home/AddData",
        success: function () { alert("Data Added!"); }
    });
});

$("#login").click(function () {
    firebase.auth().createUserWithEmailAndPassword($("#log").val, $("#pass").val).catch(function (error) {
        // Handle Errors here.
        var errorCode = error.code;
        var errorMessage = error.message;
        // ...
    });
});

/*firebase.auth().createUserWithEmailAndPassword(email, password).catch(function (error) {
    // Handle Errors here.
    var errorCode = error.code;
    var errorMessage = error.message;
    // ...
});

firebase.auth().signInWithEmailAndPassword(email, password).catch(function (error) {
    // Handle Errors here.
    var errorCode = error.code;
    var errorMessage = error.message;
    // ...
});*/