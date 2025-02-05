document.addEventListener("DOMContentLoaded", () => {
    const leftBrace = document.querySelector(".left-brace");
    const rightBrace = document.querySelector(".right-brace");
    const futureText = document.querySelector(".future-text");
    const line = document.querySelector(".line");
    const content = document.querySelector(".content");
    const actions = document.querySelector(".actions");

    // Създаване на timeline за анимации
    const tl = gsap.timeline();

    // 1. Появяване на линията
    tl.to(line, { duration: 1.5, width: "100%", opacity: 1, ease: "power2.out" })

        // 2. Появяване на скобите {} с мигане
        .to(leftBrace, { duration: 0.5, opacity: 1, ease: "power2.out" }, "+=0.5")
        .to(rightBrace, { duration: 0.5, opacity: 1, ease: "power2.out" }, "<")
        .to(leftBrace, { duration: 0.3, opacity: 0.5, repeat: 3, yoyo: true }, "-=0.3")
        .to(rightBrace, { duration: 0.3, opacity: 0.5, repeat: 3, yoyo: true }, "<")

        // 3. Появяване на текста между скобите
        .to(futureText, { duration: 1, opacity: 1, ease: "power2.out" }, "+=0.5")

        // 4. Появяване на текста под линията
        .to(content, { duration: 1, opacity: 1, ease: "power2.out" }, "+=0.5")

        // 5. Появяване на бутона
        .to(actions, { duration: 1, opacity: 1, ease: "power2.out" }, "+=0.5");
});
