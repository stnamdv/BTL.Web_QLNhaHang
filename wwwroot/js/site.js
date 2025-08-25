// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-hide alerts after 5 seconds
$(document).ready(function () {
  setTimeout(function () {
    $(".alert").fadeOut("slow");
  }, 5000);
});

// Enable tooltips
$(document).ready(function () {
  $('[data-toggle="tooltip"]').tooltip();
});

// Enable popovers
$(document).ready(function () {
  $('[data-toggle="popover"]').popover();
});
