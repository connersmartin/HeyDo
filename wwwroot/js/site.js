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

$("#create").click(function () {
    var log = $("#logCreate").val();
    var pas = $("#passCreate").val();
    var auth = firebase.auth().createUserWithEmailAndPassword(log, pas).catch(function (error) {
        // Handle Errors here.
        var errorCode = error.code;
        var errorMessage = error.message;
        // ...
    });
});

$("#login").click(function () {
    var log = $("#logIn").val();
    var pas = $("#passIn").val();
    firebase.auth().signInWithEmailAndPassword(log, pas).catch(function (error) {
        // Handle Errors here.
        var errorCode = error.code;
        var errorMessage = error.message;
    // ...
    });
    window.location.href = 'Home/Dashboard';
});

$("#logout").click(function () {
    firebase.auth().signOut().then(function () {
        alert("logged out");
        // Sign-out successful.
    }).catch(function (error) {
        alert("was not able to log out: " + error);
        // An error happened.
    });
});