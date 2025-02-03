// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/********* Stepper *********/
var stepper
$(document).ready(function () {

    if ($('.pop').length > 0) {
        $('.pop').on('click', function () {
            $('.imagepreview').attr('src', $(this).find('img').attr('src'));
            $('#imagemodal').modal('show');
        });
    }

    if ($('.bs-stepper').length > 0) {

        stepper = new Stepper($('.bs-stepper')[0], {
            linear: false,
            animation: false
        })

        // Add the links to the pages
        if ($('#stepNavigator').length > 0) {

            var items = $('.bs-stepper-pane').length;

            for (let i = 0; i < items; i++) {
                $('#stepNavigator').append('<li><a class="dropdown-item" onclick="stepper.to(' + (i + 1) + '); return false;" href="#">' + (i + 1) + '</a></li>')
            }
        }

        $('.bs-stepper')[0].addEventListener('shown.bs-stepper', function (event) {

            $("#stepNumber").html(event.detail.indexStep + 1)

            if (event.detail.indexStep == 0) {
                // Disable previous button
                $("#movePrevious").css("pointer-events", "none");
                $("#movePrevious").css("color", "gray");
            }
            else {
                // Enable previous button
                $("#movePrevious").css("pointer-events", "auto");
                $("#movePrevious").css("color", "");
            }

            if (event.detail.indexStep + 1 == $('.bs-stepper-pane').length) {
                // Disable next button
                $("#moveNext").css("pointer-events", "none");
                $("#moveNext").css("color", "gray");
            }
            else {
                // Enable steps  next button
                $("#moveNext").css("pointer-events", "auto");
                $("#moveNext").css("color", "");
            }

        })
    }
});
/********* End of stepper *********/

/********* Help selector *********/
const triggerTabList = document.querySelectorAll('#helpSelector button')
triggerTabList.forEach(triggerEl => {
    const tabTrigger = new bootstrap.Tab(triggerEl)

    triggerEl.addEventListener('click', event => {
        event.preventDefault()
        tabTrigger.show()
    })
})
/********* End of help selector *********/