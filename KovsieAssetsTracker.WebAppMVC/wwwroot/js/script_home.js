document.addEventListener("DOMContentLoaded", function () {

    // --- Navbar toggle ---
    const menuBtn = document.getElementById("menu-btn");
    const navLinks = document.getElementById("nav-links");
    const menuBtnIcon = menuBtn?.querySelector("i");

    menuBtn?.addEventListener("click", () => {
        navLinks.classList.toggle("open");
        const isOpen = navLinks.classList.contains("open");
        menuBtnIcon.setAttribute("class", isOpen ? "ri-close-line" : "ri-menu-line");
    });

    navLinks?.addEventListener("click", () => {
        navLinks.classList.remove("open");
        menuBtnIcon.setAttribute("class", "ri-menu-line");
    });

    // --- Slides ---
    const slides = document.querySelectorAll(".slide");
    const trackers = document.querySelectorAll(".bottom_tracker .tracker");
    const prevBtn = document.querySelector(".chev_left");
    const nextBtn = document.querySelector(".chev_right");
    let currentIndex = 0;
    let slideInterval;

    function showSlide(index) {
        slides.forEach((slide, i) => {
            slide.classList.toggle("active", i === index);
            trackers[i].classList.toggle("active", i === index);
        });
    }

    function nextSlide() {
        currentIndex = (currentIndex + 1) % slides.length;
        showSlide(currentIndex);
    }

    function prevSlide() {
        currentIndex = (currentIndex - 1 + slides.length) % slides.length;
        showSlide(currentIndex);
    }

    function startAutoSlide() {
        slideInterval = setInterval(nextSlide, 5000);
    }

    function resetAutoSlide() {
        clearInterval(slideInterval);
        startAutoSlide();
    }

    nextBtn.addEventListener("click", () => { nextSlide(); resetAutoSlide(); });
    prevBtn.addEventListener("click", () => { prevSlide(); resetAutoSlide(); });

    // Initialize
    showSlide(currentIndex);
    startAutoSlide();

    // --- ScrollReveal animations ---
    const scrollRevealOption = {
        distance: "50px",
        duration: 1000
    };

    //ScrollReveal().reveal(".slides h1", { ...scrollRevealOption, origin: "left" });
    //ScrollReveal().reveal(".container_right .lottie_welcome_animation", { ...scrollRevealOption, origin: "right" });
    //ScrollReveal().reveal(".slides p", { ...scrollRevealOption, delay: 500 });
    //ScrollReveal().reveal(".content .chevrons", { ...scrollRevealOption, delay: 500 });
    //ScrollReveal().reveal(".content .bottom_tracker", { duration: 1000, interval: 500, delay: 500 });
    UIHelper.showAnimation("Guest", "lottie-about", true);
    UIHelper.showAnimation("Welcome", "lottie-welcome", true);
});
ScrollReveal().reveal('.download-card', {
    origin: 'bottom',
    distance: '50px',
    duration: 1000,
    delay: 200
});
document.addEventListener("DOMContentLoaded", () => {
    // Animate elements on scroll
    ScrollReveal().reveal("[data-sr='fade-up']", {
        distance: "50px",
        origin: "bottom",
        duration: 1000,
        interval: 200,
    });
});
ScrollReveal().reveal(".content h1, .content p, .chevrons, .bottom_tracker, .lottie_welcome_animation", {
    distance: "50px",
    origin: "bottom",
    duration: 1000,
    interval: 150,
});

