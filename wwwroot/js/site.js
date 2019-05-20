// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Your web app's Firebase configuration
var firebaseConfig = {

};
// Initialize Firebase
firebase.initializeApp(firebaseConfig);

$("#setSession").click(function () {
    $.ajax({
        url: "SetSession",
        data: {
            uid: firebase.auth().currentUser.uid,
            auth: firebase.auth().currentUser.ra
        },
        success: function () { alert("Session Set!"); }
    });
});


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
        url: "AddData",
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
        url: "GetData",
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
        alert("something happened: " + errorCode + " : " + errorMessage);
    // ...
    });
    //window.location.href = 'Home/Dashboard';
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