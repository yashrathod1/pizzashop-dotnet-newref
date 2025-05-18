$(document).ready(function () {
  const $passfield = $('#password');
  $('#togglePassword').on('click', function () {
    const type = $passfield.attr('type') === 'password' ? 'text' : 'password';
    $passfield.attr('type', type);
    $(this).toggleClass('fa-eye-slash');
  });

  $("#forgotPasswordLink").on("click", function (e) {
    e.preventDefault();
    var email = $("#email").val();
    var url = '/Auth/ForgotPassword?email=' + encodeURIComponent(email);
    window.location.href = url;
  });
});
