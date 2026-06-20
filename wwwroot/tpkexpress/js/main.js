import MobileModule from "./module/MobileModule.js";
import AosModule from "./module/AosModule.js";
import BtnToTopModule from "./module/BtnToTopModule.js";
import SwiperModule from "./module/SwiperModule.js";
import CountUpModule from "./module/CountUpModule.js";
import Select2Module from "./module/Select2Module.js";
import GalleryModule from "./module/GalleryModule.js";
import TabModule from "./module/TabModule.js";
import HeaderModule from "./module/HeaderModule.js";
import SideModule from "./module/SideModule.js";
import CommonModule from "./module/CommonModule.js";
import ComponentModule from "./module/ComponentModule.js";
import SmoothScrollModule from "./module/SmoothScrollModule.js";


window.addEventListener("DOMContentLoaded", () => {
    AosModule();
    CommonModule();
    TabModule();
    Select2Module();
    CountUpModule();
    SwiperModule();
    MobileModule();
    BtnToTopModule();
    HeaderModule();
    GalleryModule();
    SideModule();
    ComponentModule();
    SmoothScrollModule();

	$(function () {
		$(".acc-info-btn").click(function () {
			$(".status-mobile").addClass("open");
			$(".overlay-status-mobile").show();
			return false;
		})
		$(".overlay-status-mobile").click(function () {
			$(".status-mobile").removeClass("open");
			$(this).hide();
		})
		$(".mobile-menu-btn").click(function () {
			$(this).addClass("active")
			$(".main-menu").addClass("open");
		})
		$(".main-menu").click(function () {
			$(".mobile-menu-btn").removeClass("active");
			$(this).removeClass("open");
		})
	})
});