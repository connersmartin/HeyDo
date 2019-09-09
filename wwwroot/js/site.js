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


