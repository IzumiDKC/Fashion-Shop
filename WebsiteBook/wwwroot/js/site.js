$(document).ready(function () {
    $('.nav-item.dropdown').hover(function () {
        $(this).find('.dropdown-menu').stop(true, true).delay(100).fadeIn(300);
    }, function () {
        $(this).find('.dropdown-menu').stop(true, true).delay(100).fadeOut(300);
    });
});