export default function LoadMoreModule() {
    $(document).ready(function() {
        const loadContainer = document.querySelectorAll(".load-container");
        if (loadContainer) {
            loadContainer.forEach((item) => {
                const loadNumberInit = parseInt(item.getAttribute("data-init"));
                const loadNumberSl = parseInt(item.getAttribute("data-sl"));
                const loadShow = item.getAttribute("data-show");
                const loadHide = item.getAttribute("data-hide");
                const loadItems = item.querySelectorAll(".load-item");
                const loadBtn = item.querySelector(".load-btn");
                loadBtn.querySelector('.text').innerHTML = loadShow
                loadItems.forEach((item) => {
                    item.classList.add("load-hidden");
                });
                if (loadItems.length <= loadNumberInit) {
                    $(loadBtn).hide();
                }
                $(loadItems).slice(0, loadNumberInit).show();
                $(loadItems).slice(0, loadNumberInit).removeClass("load-hidden");
                $(loadBtn).on("click", function(e) {
                    e.preventDefault();
                    const loadItemsAffter = item.querySelectorAll(
                        ".load-item.load-hidden"
                    );
                    if ($(item.querySelectorAll(".load-item.load-hidden")).length !== 0) {
                        loadBtn.classList.remove("load-hide");
                        $(loadItemsAffter).slice(0, loadNumberSl).slideDown();
                        $(loadItemsAffter)
                            .slice(0, loadNumberSl)
                            .removeClass("load-hidden");
                    }
                    if ($(item.querySelectorAll(".load-item.load-hidden")).length == 0) {
                        loadBtn.classList.add("load-hide");
                        loadBtn.querySelector(".text").innerHTML = loadHide;
                        if (loadItemsAffter.length == 0) {
                            $(loadItems).slice(loadNumberInit).slideUp();
                            loadItems.forEach((item) => {
                                item.classList.add("load-hidden");
                            });
                            $(loadItems).slice(0, loadNumberInit).removeClass("load-hidden");
                            loadBtn.querySelector(".text").innerHTML = loadShow;
                            loadBtn.classList.remove("load-hide");
                        }
                    }
                });
            });
        }
    });
}