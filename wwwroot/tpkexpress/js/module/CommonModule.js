export default function CommonModule() {
  if (window.innerWidth > 1200) {
    const broll = document.querySelectorAll(".broll");
    if (broll) {
      broll.forEach((item) => {
        const brollItem = item.querySelectorAll(".brollItem");
        let sections = gsap.utils.toArray(brollItem);
        let percent = -100 * (sections.length - 9)
        if (item.classList.contains('third')) {
          percent = 100 * (sections.length - 9)
        } else {
          percent = -100 * (sections.length - 9)
        }
        gsap.to(sections, {
          xPercent: percent,
          ease: "none",
          scrollTrigger: {
            trigger: item,
            pin: false,
            start: "center bottom",
            markers: false,
            scrub: 1,
            // snap: {
            //     snapTo: 1 / (sections.length - 1),
            //     duration: { min: 1, max: 2 },
            //     delay: 0
            // },
            end: () => "+=" + item?.offsetWidth / 1,
          },
        });
      });
    }

    const wave = document.querySelectorAll(".wave");
    if (wave) {
      wave.forEach((item) => {
        const waveItem = item.querySelectorAll(".wave-item");
        let sections = gsap.utils.toArray(waveItem);

        gsap.to(sections, {
          xPercent: -100 * sections.length,
          ease: "none",
          scrollTrigger: {
            trigger: item,
            pin: false,
            start: "center bottom",
            markers: false,
            scrub: 1,
            // snap: {
            //     snapTo: 1 / (sections.length - 1),
            //     duration: { min: 1, max: 2 },
            //     delay: 0
            // },
            end: () => "+=" + item?.offsetWidth * 10,
          },
        });
      });
    }
    const act = document.querySelectorAll(".atc");
    if (act) {
      act.forEach((item) => {
        const markerItem = item.querySelectorAll(".atc-inner");
        let sections = gsap.utils.toArray(markerItem);
        let tl = gsap.timeline({
          scrollTrigger: {
            trigger: sections,
            scrub: true,
            pin: false,
            markers: false,
            start: "top center+=200",
            end: "bottom center",
          },
        });
        tl.from(sections, {
          ease: "none",
          scale: 1.15,
        }).to(sections, {
          ease: "none",
          scale: 1,
        });
      });
    }

    var timeline = new TimelineMax();
    timeline.staggerFrom(".region-dot-marker .inner", 0.4, { y: -100, opacity: 0 }, 0.05);
    timeline.reverse();
    const region = document.querySelector(".region");
    if (region) {
      gsap.to(region, {
        scrollTrigger: {
          pin: false,
          markers: false,
          trigger: ".region",
          scrub: true,
          start: "top center",
          end: "bottom center",
          onToggle: (self) => {
            if (self.isActive) {
              timeline.play();
            } else {
              timeline.reverse();
            }
          },
        },
      });
    }


    var timeline2 = new TimelineMax();
    timeline2.staggerFrom(".channel-box-item", 0.4, { y: -100, opacity: 0 }, 0.1);
    timeline2.reverse();
    const channel = document.querySelector(".channel");
    if (channel) {
      gsap.to(channel, {
        scrollTrigger: {
          pin: false,
          markers: false,
          trigger: ".channel",
          scrub: true,
          start: "top center",
          end: "bottom center",
          onToggle: (self) => {
            if (self.isActive) {
              timeline2.play();
            } else {
              timeline2.reverse();
            }
          },
        },
      });
    }


    var timeline3 = new TimelineMax();
    timeline3.staggerFrom(".cust-wrap", 1, { rotateY: 90, opacity: 0 }, 0.1);
    timeline3.reverse();
    const cust = document.querySelector(".cust");
    if (cust) {
      gsap.to(cust, {
        scrollTrigger: {
          pin: false,
          markers: false,
          trigger: ".cust",
          scrub: true,
          start: "top center",
          end: "bottom center",
          onToggle: (self) => {
            if (self.isActive) {
              timeline3.play();
            } else {
              timeline3.reverse();
            }
          },
        },
      });
    }
  }


  if (window.innerWidth > 750) {
    // Lấy đối tượng path trong SVG
    const path = document.querySelector(".region-line-svg path");
    const gpsProgress = document.querySelector(".gps-progress");
    if (path) {
      let svgOffsetTotal = 1319.9085693359375;

      gsap.to(path, {
        scrollTrigger: {
          pin: false,
          markers: false,
          trigger: ".region", // Sử dụng đối tượng gps làm trigger
          scrub: true, // Kích hoạt "scrubbing" để áp dụng animation dựa trên vị trí cuộn trang
          start: "top center", // Khi đối tượng path nằm ở vị trí trung tâm của viewport
          end: () => "+=200", // Khi đối tượng path đi qua đáy viewport
          onUpdate: (self) => {
            path.style = `stroke-dashoffset:${svgOffsetTotal - (self.progress * svgOffsetTotal)
              }px`;
          },
        },
      });
    }
  }

  var hasParallax = gsap.utils.toArray('.has-parallax');
    hasParallax.forEach(function (hParallax) {
      var bgImage = hParallax.querySelector("img");
      var bgVideo = hParallax.querySelector("video");
      var parallax = gsap.fromTo([bgImage, bgVideo], { y: '-20%', scale: 1.15 }, { y: '20%', scale: 1, duration: 1, ease: Linear.easeNone });
      var parallaxScene = ScrollTrigger.create({
        trigger: hParallax,
        start: "top 100%",
        end: () => `+=${hParallax.offsetHeight + window.innerHeight}`,
        animation: parallax,
        scrub: true
      });
    });
      // Clipped Image 
  gsap.utils.toArray('.clipImg').forEach((clipImg) => {
    const clipImgInner = clipImg.querySelector(".clipImgInner")
    function setClippedImageWrapperProperties() {
      gsap.set(clipImgInner, { height: window.innerHeight, });
      gsap.set(clipImg, { height: window.innerHeight });
    }

    // setClippedImageWrapperProperties();

    // window.addEventListener('resize', setClippedImageWrapperProperties);

    var clippedImageAnimation = gsap.to(clipImgInner, {
      clipPath: 'inset(0% 0% 0%)',
      scale: 1,
      duration: 1,
      ease: 'Linear.easeNone'
    });

    var clippedImageScene = ScrollTrigger.create({
      trigger: clipImg,
      start: function () {
        const startPin = window.innerHeight / 5;
        return `top bottom-=${startPin}`;
      },
      end: function () {
        const endPin = window.innerHeight / 5;
        return `bottom bottom-=${endPin}`;
      },
      animation: clippedImageAnimation,
      scrub: 1,
      pin: false,
      pinSpacing: false,
    });

  });

  var hasMaskFill = gsap.utils.toArray('.has-mask-fill');
  hasMaskFill.forEach(function (hMaskFill) {
    var spanFillMask = hMaskFill.querySelectorAll("span");
    gsap.to(spanFillMask, {
      scrollTrigger: {
        trigger: hMaskFill,
        start: "top 85%",
        end: () => `+=${hMaskFill.offsetHeight * 2}`,
        scrub: 1,
        // markers: true,
      },
      duration: 1,
      backgroundSize: "200% 100%",
      stagger: 0.5,
      ease: Linear.easeNone,
    });
  });


   // Clipped Image 
   gsap.utils.toArray('.clipWr').forEach((clippedImageWrapper) => {

    const clippedImagePin = clippedImageWrapper.querySelector(".clipPin");
    const clippedImage = clippedImageWrapper.querySelector(".clipImage");
    const clippedImageGradient = clippedImageWrapper.querySelector(".clipGradient");
    const clippedImageContent = clippedImageWrapper.querySelector(".clipContent");

    gsap.set(clippedImageContent, { paddingTop: (window.innerHeight / 2) + clippedImageContent.offsetHeight });

    gsap.set(clippedImageGradient, { backgroundColor: clippedImageWrapper.getAttribute("data-bgcolor") });

    function setClippedImageWrapperProperties() {
      gsap.set(clippedImageContent, { paddingTop: "" });
      gsap.set(clippedImageGradient, { height: window.innerHeight * 0.3 });
      gsap.set(clippedImage, { height: window.innerHeight, });
      gsap.set(clippedImageContent, { paddingTop: (window.innerHeight / 2) + clippedImageContent.offsetHeight });
      gsap.set(clippedImageWrapper, { height: window.innerHeight + clippedImageContent.offsetHeight });

    }
    setClippedImageWrapperProperties();
    window.addEventListener('resize', setClippedImageWrapperProperties);

    gsap.to(clippedImageGradient, {
      scrollTrigger: {
        trigger: clippedImagePin,
        start: function () {
          const startPin = 0;
          return "top +=" + startPin;
        },
        end: function () {
          const endPin = clippedImageContent.offsetHeight;
          return "+=" + endPin;
        },
        scrub: true,
      },
      opacity: 1,
      y: 1
    });

    var clippedImageAnimation = gsap.to(clippedImage, {
      clipPath: 'inset(0% 0% 0%)',
      scale: 1,
      duration: 1,
      ease: 'Linear.easeNone'
    });

    var clippedImageScene = ScrollTrigger.create({
      trigger: clippedImagePin,
      start: function () {
        const startPin = 0;
        return "top +=" + startPin;
      },
      end: function () {
        const endPin = clippedImageContent.offsetHeight;
        return "+=" + endPin;
      },
      animation: clippedImageAnimation,
      scrub: 1,
      pin: true,
      pinSpacing: false,
      // markers: true
    });

  });
}
