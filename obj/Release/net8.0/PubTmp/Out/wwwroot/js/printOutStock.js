function printOutOfStock(outOfStockId) {
    disableButton();
    $.ajax({
        url: '/OutOfStock/Print',
        data: { Id: outOfStockId },
        type: 'GET',
        success: function (response) {
            printInvoice(response.Data);
        },
        error: function (response) {
            showToast(response.responseJSON.Message, response.responseJSON.Type, false);
        },
        complete: function () {
            enableButton();
        }
    });
}
function printOutOfStockTemp(outOfStockId) {
    disableButton();
    $.ajax({
        url: '/OutOfStock/PrintTemp',
        data: { Id: outOfStockId },
        type: 'GET',
        success: function (response) {
            printInvoice(response.Data);
        },
        error: function (response) {
            showToast(response.responseJSON.Message, response.responseJSON.Type, false);
        },
        complete: function () {
            enableButton();
        }
    });
}

//function printInvoice(template) {
//    // Tạo một iframe ẩn để in nội dung
//    const iframe = document.createElement('iframe');
//    iframe.style.position = 'absolute';
//    iframe.style.width = '0';
//    iframe.style.height = '0';
//    iframe.style.border = 'none';

//    document.body.appendChild(iframe);

//    // Lấy tài liệu nội bộ của iframe
//    const iframeDocument = iframe.contentWindow.document;

//    // Đặt nội dung vào iframe
//    iframeDocument.open();
//    iframeDocument.write(template);
//    iframeDocument.close();

//    // In nội dung của iframe
//    iframe.contentWindow.print();
//}
function printInvoice(template) {
    const printWindow = window.open('', '_blank');

    if (!printWindow) {
        alert('Trình duyệt đã chặn popup. Hãy cho phép popup để in.');
        return;
    }

    printWindow.document.open();
    printWindow.document.write(template);
    printWindow.document.close();

    printWindow.onload = function () {

        const images = printWindow.document.images;
        let loaded = 0;

        function checkImagesLoaded() {
            loaded++;
            if (loaded === images.length) {
                startPrint();
            }
        }

        if (images.length === 0) {
            startPrint();
            return;
        }

        for (let img of images) {
            if (img.complete) {
                loaded++;
            } else {
                img.onload = checkImagesLoaded;
                img.onerror = checkImagesLoaded;
            }
        }

        if (loaded === images.length) {
            startPrint();
        }

        function startPrint() {
            printWindow.focus();

            // Bắt sự kiện kết thúc print (Chrome đôi khi không trigger)
            const mediaQueryList = printWindow.matchMedia('print');

            const closeWindow = () => {
                setTimeout(() => {
                    printWindow.close();
                }, 200);
            };

            if (mediaQueryList) {
                mediaQueryList.addEventListener('change', function (mql) {
                    if (!mql.matches) {
                        closeWindow();
                    }
                });
            }

            printWindow.onafterprint = closeWindow;

            // In
            printWindow.print();

            // fallback nếu browser không trigger event
            setTimeout(closeWindow, 1000);
        }
    };
}