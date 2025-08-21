// Bootstrap validation
(() => {
    'use strict';
    const forms = document.querySelectorAll('form');
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', e => {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
})();

// Password Strength Meter
const passwordInput = document.getElementById("password");
const strengthBar = document.getElementById("passwordStrength");
if (passwordInput) {
    passwordInput.addEventListener("input", () => {
        const val = passwordInput.value;
        let strength = 0;
        if (val.match(/[a-z]+/)) strength++;
        if (val.match(/[A-Z]+/)) strength++;
        if (val.match(/[0-9]+/)) strength++;
        if (val.match(/[$@#&!]+/)) strength++;   // ✅ works fine in .js file
        strengthBar.style.width = (strength * 25) + "%";
        strengthBar.className = "progress-bar bg-" + (["danger", "warning", "info", "success"][strength - 1] || "");
    });
}

// Bootstrap tooltip init
var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
tooltipTriggerList.map(el => new bootstrap.Tooltip(el));
