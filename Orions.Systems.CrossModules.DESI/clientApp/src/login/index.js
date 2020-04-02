window.Orions.LoginPage = {
    initialize: function (componentRef) {
		let logingFormEl = document.querySelector('.desi-login.form');
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