$(document).ready(function () {
    const swiper = new Swiper('#banner', {
        loop: true,
        pagination: {
            el: '.swiper-pagination',
        },
        scrollbar: {
            el: '.swiper-scrollbar',
        },
    });
    $('#find-transportation').on('click', function (e) {
        const status = {
            1: {
                text: "Đang xử lí",
                class: "badge-progress"
            },
            2: {
                text: "Nhập kho TQ",
                class: "badge-delivered",
            },
            3: {
                text: "Đã phát hàng",
                class: "badge-delivered",
            },
            4: {
                text: "Kiểm hóa",
                class: "badge-delivered",
            },
            5: {
                text: "Nhập kho HN",
                class: "badge-delivered",
            },
            6: {
                text: "Đã nhận hàng",
                class: "badge-completed",
            },
        }
        e.preventDefault();
        const barcode = $('#Barcode').val();
        if(!barcode) return;
        $.ajax({
            url: '/Transportation/GetTransportation',
            type: 'GET',
            data: { Barcode: barcode },
            success: function (response) {
                var data = response.Data;
                if(data){
                    var statusMatch = status?.[data?.Status]
                    $('.transportation-status .status-tag').text(statusMatch?.text);
                    $('.transportation-status .status-tag').addClass(statusMatch?.class);

                }
            },
            error: function (response) {
                
            }, complete: function () {
                
            }
        });
    })
})