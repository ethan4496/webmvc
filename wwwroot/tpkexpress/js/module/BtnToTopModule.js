export default function BtnToTopModule() {
    const btnToTop = document.querySelector(".backToTop");
   
    if (btnToTop) {
         window.addEventListener("scroll", () => {
            if (window.scrollY > 10) {
                btnToTop.classList.add("active");
            } else {
                btnToTop.classList.remove("active");
            }
        });
        btnToTop.addEventListener("click", function () {
            document.body.scrollTop = 0;
            document.documentElement.scrollTop = 0;
        });
    }
    $(document).ready(function () {
        if (btnToTop) {
            if (window.scrollY > 10) {
                btnToTop.classList.add("active");
            } else {
                btnToTop.classList.remove("active");
            }
        }

    });
}
