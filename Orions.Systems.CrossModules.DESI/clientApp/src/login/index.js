window.Orions.LoginPage = {
    initialize: function (componentRef) {
        let logingFormEl = document.querySelector('.desi-login.form');
        let submitBtnEl = document.querySelector('button[type=submit]')

        let validateForm = function () {
            let is_valid = logingFormEl.checkValidity()

            if (is_valid) {
                submitBtnEl.removeAttribute('disabled')
            }
            else {
                submitBtnEl.disabled = 'disabled'
            }

            logingFormEl.classList.add('was-validated');
        }

        validateForm();

        logingFormEl.querySelectorAll('.form-control').forEach(input => {
            input.addEventListener(('input'), () => {
                validateForm()
            });
        });

        logingFormEl.addEventListener('submit', function (event) {
            if (logingFormEl.checkValidity() === false) {
                
            }
            else {
                componentRef.invokeMethodAsync("Login");
            }

            logingFormEl.classList.add('was-validated');
            event.preventDefault();
        }, false);
	}
}