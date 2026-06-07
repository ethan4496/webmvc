export default function SelectCusModule() {
    const slHeads = document.querySelectorAll(".sl-head");
    const slLists = document.querySelectorAll(".sl-list");
    var event = new Event("change");

    if (slHeads) {
        slHeads.forEach((item) => {
            item.addEventListener("click", () => {
                const slList = item.parentElement.querySelector(".sl-list");
                $(slList).toggle();
            });
        });
    }
    const slItems = document.querySelectorAll(".sl-item");
    if (slItems) {
        slItems.forEach((item) => {
            const name = item.getAttribute("data-name");
            item.innerHTML = name;
            item.addEventListener("click", () => {
                const slMain =
                    item.parentElement.parentElement.querySelector(".sl-main");
                const input =
                    item.parentElement.parentElement.querySelector(".select-input");
                const name = item.getAttribute("data-name");
                const slug = item.getAttribute("data-slug");
                const slList = item.parentElement;
                input.value = slug;
                slMain.innerHTML = name;
                input.dispatchEvent(event);
                slItems.forEach((item) => {
                    item.classList.remove("active");
                });
                item.classList.add("active");
                $(slList).hide();
            });
        });
    }
    if (slLists) {
        slLists.forEach((item) => {
            const slMain = item.parentElement.querySelector(".sl-main");
            const slItem = item.querySelectorAll(".sl-item");
            slItem.forEach((item) => {
                if (item.classList.contains("active")) {
                    const slug = item.getAttribute("data-slug");
                    const name = item.getAttribute("data-name");
                    const input =
                        item.parentElement.parentElement.querySelector(".select-input");
                    input.value = slug;
                    slMain.innerHTML = name;
                    input.dispatchEvent(event);
                }
            });
        });
    }

    const slCuss = document.querySelectorAll(".sl-cus");
    window.addEventListener("click", (e) => {
        slCuss.forEach((item) => {
            const notCuss = item.contains(e.target);
            if (!notCuss) {
                const slList = item.querySelector(".sl-list");
                $(slList).hide();
            }
        });
    });
}