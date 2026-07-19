export default function HeaderModule() {
    const header = document.querySelector(".hd");
    const mobile = document.querySelector(".mobile");
    const mobileOverlay = document.querySelector(".mobile-overlay");
    const search = document.querySelector(".search-mona");
    const header2 = document.querySelector(".site-header");

    function HandleHeader() {
        if (header && mobile && mobileOverlay) {
            if (window.scrollY > 0) {
                header.classList.add("sticky");
                mobile.classList.add("sticky");
                mobileOverlay.classList.add("sticky");
            } else {
                header.classList.remove("sticky");
                mobile.classList.remove("sticky");
                mobileOverlay.classList.remove("sticky");
            }
        }
        if (!header2) return;
        if (window.scrollY > 0) {
            header2.classList.add("sticky");
        } else {
            header2.classList.remove("sticky");
        }
    }
    window.addEventListener("scroll", function () {
        HandleHeader();
    });
    $(document).ready(function () {
        HandleHeader();
    });
}