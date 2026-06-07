export default function ShowHideModule() {
    const showhideBlocks = document.querySelectorAll(".show-hide-block");
    if (showhideBlocks) {
        showhideBlocks.forEach((item) => {
            const dataShow = item.getAttribute("data-init");
            const dataShowText = item.getAttribute("data-show");
            const dataHideText = item.getAttribute("data-hide");
            const dataBtn = item.querySelector('.show-hide-btn')
            const showhideContents = item.querySelector(".show-hide-content");
            dataBtn.querySelector('.text').innerHTML = dataShowText;
            var itemHeight = $(showhideContents).height();
            var lineHeight = parseInt($(showhideContents).css("line-height"), 10);
            var numberLine = Math.round(itemHeight / lineHeight);
            if (numberLine > dataShow) {
                showhideContents.style = `display:-webkit-box;-webkit-line-clamp:${dataShow};-webkit-box-orient: vertical;overflow: hidden;text-overflow: ellipsis; word-break: break-word;`;
            } else {
                $(dataBtn).hide();
            }
        });
    }
    const showhideToggles = document.querySelectorAll(".show-hide-btn");
    if (showhideToggles) {
        showhideToggles.forEach((item) => {
            item.addEventListener("click", () => {
                const dataShowText = item.parentElement.getAttribute(
                    "data-show"
                );
                const dataHideText = item.parentElement.getAttribute(
                    "data-hide"
                );
                const dataShow = item.parentElement.getAttribute(
                    "data-init"
                );
                if (
                    item.parentElement.querySelector(".show-hide-content").style
                    .overflow == "hidden"
                ) {
                    item.parentElement.querySelector(
                        ".show-hide-content"
                    ).style = `display:-webkit-box;-webkit-line-clamp: 0;-webkit-box-orient: vertical;overflow: auto;text-overflow: ellipsis; word-break: break-word;`;
                    item.querySelector(".text").innerHTML = dataHideText;
                    item.classList.add("hide")
                } else {
                    item.parentElement.querySelector(
                        ".show-hide-content"
                    ).style = `display:-webkit-box;-webkit-line-clamp:${dataShow};-webkit-box-orient: vertical;overflow: hidden;text-overflow: ellipsis; word-break: break-word;`;
                    item.querySelector(".text").innerHTML = dataShowText;
                    item.classList.remove("hide")
                }
            });
        });
    }
}