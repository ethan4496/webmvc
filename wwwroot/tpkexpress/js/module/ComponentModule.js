export default function ComponentModule() {
  // Funtion Copppy
  const btnCoppy = document.querySelectorAll(".center-control-btn.coppy-link");
  if (btnCoppy) {
    btnCoppy.forEach((item) => {
      item.addEventListener("click", (e) => {
        btnCoppy.forEach((item2) => {
          item2.querySelector(".text").innerHTML =
            item.getAttribute("data-csc");
          item2.classList.remove("active");
        });
        e.preventDefault();
        const value = item.href;
        var $temp = $("<input>");
        $("body").append($temp);
        $temp.val(value).select();
        document.execCommand("copy");
        $temp.remove();
        item.querySelector(".text").innerHTML = item.getAttribute("data-dsc");
        item.classList.add("active");
      });
    });
  }
  // Funtion Download pdf
  const btnDownload = document.querySelectorAll(".docs-wrap");
  if (btnDownload) {
    btnDownload.forEach((item) => {
      item.addEventListener("click", (e) => {
        e.preventDefault();
        var link = document.createElement("a");
        link.href = item.getAttribute("data-src");
        link.download = "file.pdf";
        link.dispatchEvent(new MouseEvent("click"));
      });
    });
  }

  // check pass;
  const fPass = document.querySelectorAll(".f-pass");
  if (fPass) {
    fPass.forEach((item) => {
      item.addEventListener("click", (e) => {
        const input = item.querySelector(".re-input");
        if (e.target.closest(".f-lock")) {
          if (input.type == "text") {
            input.type = "password";
            item.classList.remove("active");
          } else {
            input.type = "text";
            item.classList.add("active");
          }
        }
      });
    });
  }

  // Js get height item

  function getHeight() {
    const getHeightItems = document.querySelectorAll(".getHeight");
    if (getHeightItems) {
      getHeightItems.forEach((item) => {
        item.style = `--height:${item.getBoundingClientRect().height}px`;
      });
    }
  }
  getHeight();

  // Js get width item
  function getWidth() {
    const getWidthItems = document.querySelectorAll(".getWidth");
    if (getWidthItems) {
      getWidthItems.forEach((item) => {
        item.style = `--width:${item.getBoundingClientRect().width}px`;
      });
    }
  }
  getWidth();
  window.addEventListener("resize", () => {
    getHeight();
    getWidth();
  });

  function cscrollAddClass(el, className) {
    $(el).each(function () {
      let el = this;
      let itemTop = $(el).offset().top;
      if (
        $(el).hasClass("custom-fadeInUpBig") ||
        $(el).hasClass("custom-bounceInUp")
      ) {
        itemTop = $(el).offset().top - 2000;
      }
      if (
        $(el).hasClass("custom-fadeInDownBig") ||
        $(el).hasClass("custom-bounceInDown")
      ) {
        itemTop = $(el).offset().top + 2000;
      }

      if ($(el).hasClass("custom-fadeInUp")) {
        itemTop = $(el).offset().top - 20;
      }

      if ($(el).hasClass("custom-fadeInDown")) {
        itemTop = $(el).offset().top + 20;
      }

      if ($(el).hasClass("custom-backInUp")) {
        itemTop = $(el).offset().top - 1200;
      }

      if ($(el).hasClass("custom-backInDown")) {
        itemTop = $(el).offset().top + 1200;
      }
      if (itemTop < $(window).scrollTop() + ($(window).height() / 10) * 8) {
        $(el).addClass(className);
      }
    });
  }

  function cbindImageAnimations() {
    cscrollAddClass(".scr-item", "active");
    $(window).on("scroll", function () {
      cscrollAddClass(".scr-item", "active");
    });
  }
  cbindImageAnimations();

  // AUTO MOVE
  const cirs = document.querySelectorAll(".cir");
  var timer = Array;

  function randomXY(cir, x, y) {
    if (!x) {
      x = 10;
    }
    if (!y) {
      y = 15;
    }
    if (cir.classList.contains("cir-2")) {
      const translateX = Math.floor(Math.random() * x);
      const translateY = Math.floor(Math.random() * y);
      cir.style.transform = `translate(${translateX + "%" + "," + translateY + "%"
        })`;
    } else {
      // const translateX = Math.floor(Math.random() * 10);
      const translateY = Math.floor(Math.random() * y);
      // cir.style.transform = `translate(${translateX + "%" + "," + translateY + "%"})`;
      cir.style.transform = `translate(${0 + "%" + "," + -translateY + "%"})`;
    }
  }
  for (let i = 0; i < cirs.length; i++) {
    let time = (i % 3) * 200 + 1500;
    const x = parseInt(cirs[i].getAttribute("data-x"));
    const y = parseInt(cirs[i].getAttribute("data-y"));
    timer[i] = setInterval(() => {
      randomXY(cirs[i], x, y);
    }, time);
  }

}
