export default function SwiperModule() {
  function functionSlider(element, customizeOption, typePagi) {
    const swiperSlider = document.querySelectorAll(element);
    if (swiperSlider) {
      swiperSlider.forEach((item) => {
        const swiper = item.querySelector(".swiper");
        const pagi = item.querySelector(".swiper-pagination");
        const next = item.querySelector(".swiper-next");
        const prev = item.querySelector(".swiper-prev");
        if (!typePagi) {
          typePagi = "bullets";
        }
        var slide = new Swiper(swiper, {
          watchSlidesProgress: true,
          pagination: {
            el: pagi,
            type: typePagi,
            clickable: true,
          },
          navigation: {
            nextEl: next,
            prevEl: prev,
          },
          fadeEffect: {
            crossFade: true,
          },
          ...customizeOption,
        });
      });
    }
  }

  functionSlider(".element", {
    speed: 1200,
    autoplay: {
      delaY: 5000,
    },
    // slidesPerGroup: 2,
    initialSlide: 0,
    centeredSlides: false,
    loop: false,
    spaceBetween: 24,
    effect: "slide",
    breakpoints: {
      0: {
        slidesPerView: 1.2,
        freeMode: true,
      },
      500: {
        slidesPerView: 2.2,
      },
      768: {
        slidesPerView: 3,
      },
      1200: {
        slidesPerView: 3,
      },
    },
  });

  functionSlider(".pro-slider", {
    speed: 1200,
    autoplay: false,
    // slidesPerGroup: 2,
    initialSlide: 0,
    centeredSlides: false,
    loop: false,
    spaceBetween: 0,
    effect: "slide",
    slidesPerView: "auto",
  });

  functionSlider(".ser-slider", {
    speed: 1200,
    autoplay: false,
    // slidesPerGroup: 2,
    initialSlide: 0,
    centeredSlides: false,
    loop: false,
    spaceBetween: 0,
    effect: "slide",
    slidesPerView: "auto",
  });
  functionSlider(".media-slider", {
    speed: 1200,
    autoplay: false,
    // slidesPerGroup: 2,
    initialSlide: 0,
    centeredSlides: false,
    loop: false,
    spaceBetween: 0,
    effect: "slide",
    slidesPerView: "auto",
  });

  functionSlider(".cusc-slider", {
    speed: 1200,
    autoplay: false,
    // slidesPerGroup: 2,
    initialSlide: 0,
    centeredSlides: false,
    loop: false,
    spaceBetween: 0,
    effect: "slide",
    slidesPerView: "auto",
  });
  functionSlider(".prose-slider", {
    speed: 1200,
    autoplay: false,
    // slidesPerGroup: 2,
    initialSlide: 0,
    centeredSlides: false,
    loop: false,
    spaceBetween: 0,
    effect: "slide",
    slidesPerView: "auto",
  });


  const bannerSliders = document.querySelectorAll(".banner-slider");
  if (bannerSliders) {
    bannerSliders.forEach((item) => {
      const swiperMain = item.querySelector(".banner-slider-main .swiper");
      const swiperThumb = item.querySelector(".banner-slider-thumb .swiper");
      var bannerSliderThumb = new Swiper(swiperThumb, {
        slidesPerView: "auto",
        spaceBetween: 0,
        loop: false,
        autoplay: false,
      });
      var bannerSliderMain = new Swiper(swiperMain, {
        slidesPerView: 1,
        spaceBetween: 0,
        // loopedSlides: 6,
        loop: false,
        autoplay: false,
        speed: 1000,
        effect: "coverflow",
        grabCursor: true,
        parallax: true,
        centeredSlides: true,
        coverflowEffect: {
          rotate: 0.05,
          depth: 0,
          stretch: 0,
          modifier: 1,
          slideShadows: !1,
        },
        on: {
          init: function (e) {
            let swiper = this;
            for (let i = 0; i < swiper.slides.length; i++) {
              $(swiper.slides[i])
                .find(".banner-img")
                .attr({
                  "data-swiper-parallax": 0.9 * swiper.width,
                  "data-swiper-paralalx-opacity": 0.1,
                });
            }
            let index = swiper.activeIndex;
          },
          resize: function () {
            this.update();
          },
        },
        thumbs: {
          swiper: bannerSliderThumb,
        },
      });
    });
  }
  // Slider Control
  // topsSliderMain.controller.control = topSliderThumb;
  // topSliderThumb.controller.control = topsSliderMain;
}
