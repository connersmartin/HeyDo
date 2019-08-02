//Firebase configuration
var firebaseConfig = {

};
// Initialize Firebase only once
if (firebase.apps.length===0) {
firebase.initializeApp(firebaseConfig);
}
//if already logged in go to dashboard
if (firebase.auth().currentUser !== null) {
    loginAPI();
}


$("#create").click(function () {
    var log = $("#logCreate").val();
    var pas = $("#passCreate").val();
    firebase.auth().createUserWithEmailAndPassword(log, pas).then(function () {
            loginAPI(true);
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
        loginAPI(false);
    }).catch(function (error) {
        var errorCode = error.code;
        var errorMessage = error.message;
        alert(errorCode + " - " + errorMessage);
    });
});

function loginAPI(newAdmin) {
    $.ajax({
        url: "/Home/SetAuth",
        type: 'POST',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Accept", "application/json");
            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.setRequestHeader("Authorization", "Bearer " + firebase.auth().currentUser.ra);
            xhr.setRequestHeader("Uid", firebase.auth().currentUser.uid);
            xhr.setRequestHeader("Token", firebase.auth().currentUser.ra);
        },
        error: function (ex) {
            alert("error? "+ex.status + " - " + ex.statusText);
        },
        success: function (data) {
            if (!newAdmin) {
                window.location.href = "../Home/Dashboard";
            } else {
                window.location.href = "../Home/AddAdmin";
            }
            
        }
    });
}

$("#logout").click(function () {
    $.ajax({
        url: "/Home/ClearCookies",
        type: 'POST'

    });
    firebase.auth().signOut().then(function () {
        alert("logged out");
        
        // Sign-out successful.
    }).catch(function (error) {
        alert("was not able to log out: " + error);
        // An error happened.
    });
    window.location.href = "../Home/Index";
});


