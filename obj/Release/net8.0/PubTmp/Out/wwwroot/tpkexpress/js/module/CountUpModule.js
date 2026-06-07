import { CountUp } from "../../assets/library/countUp/countUp.min.js";
export default function CountUpModule() {
    let num = document.querySelectorAll(".countNum");
    num.forEach((v) => {
        let n = parseInt(v.textContent);
        var countUp = new CountUp(v, n, {
            separator: ".",
            decimal: '.',
        });
        // let check = true;
        // window.addEventListener("scroll", () => {
        //     if (v.getBoundingClientRect().top < window.innerHeight && check) {
        //         countUp.start();
        //         check = false;
        //     }
        // });
        let $window = $(window);

        function scrollAddClass(el, className) {
            $(el).each(function () {
                let el = this;
                if (
                    $(el).offset().top <
                    $window.scrollTop() + ($window.height() / 10) * 8
                ) {
                    countUp.start();
                    $(el).addClass(className);
                }
            });
        }

        function bindImageAnimations() {
            scrollAddClass(".countNum", "is-inview");

            $window.on("scroll", function () {
                scrollAddClass(".countNum", "is-inview");
            });
        }
        bindImageAnimations();
    });

}