document.addEventListener("DOMContentLoaded", function () {
    const interactiveCards = document.querySelectorAll(".quick-access-card, .app-card, .app-form-card, .section-card");

    interactiveCards.forEach((card) => {
        card.addEventListener("mousemove", (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            const rotateX = ((y - centerY) / centerY) * -2;
            const rotateY = ((x - centerX) / centerX) * 2;

            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-2px)`;
        });

        card.addEventListener("mouseleave", () => {
            card.style.transform = "";
        });
    });
});