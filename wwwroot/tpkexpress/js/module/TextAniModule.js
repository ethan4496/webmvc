export default function TextAniModule() {
    Splitting({
        target: "[data-spl]",
    });

    Splitting({
        target: "[data-spl-line]",
        by: "lines"
    });

    // SplitText inner

    const charSpls = document.querySelectorAll(".txt-spl .word");

    if (charSpls) {
        let text = "";
        charSpls.forEach(item => {
            text = item.innerHTML.trim();
            item.innerHTML = `<span class="chars">${text}</span>`;
        });
    }

}