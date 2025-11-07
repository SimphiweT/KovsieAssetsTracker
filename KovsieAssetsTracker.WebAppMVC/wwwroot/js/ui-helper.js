var UIHelper = (function () {

    // ----------------- SWEET ALERT -----------------
    function showAlert(type, message, title = "Notification", timer = 2000) {
        if (!message || !type) return;

        const validIcons = ["success", "error", "warning", "info", "question"];
        const icon = validIcons.includes(type) ? type : "info";

        Swal.fire({
            icon: icon,
            title: title,
            text: message,
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: timer
        });
    }

    // Confirmation dialog (returns a promise)
    function confirmDialog(title, text, confirmText = "Yes", cancelText = "Cancel", type = "warning") {
        return Swal.fire({
            title: title,
            text: text,
            icon: type,
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: confirmText,
            cancelButtonText: cancelText
        });
    }

    // ----------------- LOTTIE -----------------
    const animations = {
        "Administrator": "/animations/admin.json",
        "Technician": "/animations/technician.json",
        "Success": "/animations/success.json",
        "Error": "/animations/error.json",
        "Guest": "/animations/guest.json",
        "Welcome": "/animations/welcome.json",
        "Contact": "/animations/admin.json"
    };

    function showAnimation(key, containerId = "lottie-role-animation", loop = true) {
        let path = animations[key] || animations["Guest"];
        let container = document.getElementById(containerId);
        if (!container) return;

        lottie.loadAnimation({
            container: container,
            renderer: "svg",
            loop: loop,
            autoplay: true,
            path: path
        });
    }

    return {
        showAlert,
        confirmDialog,
        showAnimation
    };
})();
