export default function RangeModule() {

    const rangeInputs = document.querySelectorAll('.range-input input')

    let localMinVal = parseInt(rangeInputs[0]?.value)
    let localMaxVal = parseInt(rangeInputs[1]?.value)
    function RangeChange() {
        const progress = document.querySelector('.range-slider .progress')
        const rangePriceMin = document.querySelector('.range-price-min')
        const rangePriceMax = document.querySelector('.range-price-max')
        let priceGap = 200000;
        if (rangeInputs && progress) {
            let minVal = parseInt(rangeInputs[0].value)
            let maxVal = parseInt(rangeInputs[1].value)
            progress.style.left = (minVal / rangeInputs[0].max) * 100 + '%';
            progress.style.right = 100 - (maxVal / rangeInputs[1].max) * 100 + '%';
            rangePriceMin.innerHTML = formatCurrencyVND(minVal)
            rangePriceMax.innerHTML = formatCurrencyVND(maxVal)
            rangeInputs.forEach(item => {
                item.addEventListener('input', (e) => {
                    let minVal = parseInt(rangeInputs[0].value)
                    let maxVal = parseInt(rangeInputs[1].value)
                    if (maxVal - minVal < priceGap) {
                        if (e.target.className === 'range-min') {
                            rangeInputs[0].value = maxVal - priceGap;
                        } else {
                            rangeInputs[1].value = minVal + priceGap;
                        }
                    } else {
                        progress.style.left = (minVal / rangeInputs[0].max) * 100 + '%';
                        progress.style.right = 100 - (maxVal / rangeInputs[1].max) * 100 + '%';
                    }
                    rangePriceMin.innerHTML = formatCurrencyVND(minVal)
                    rangePriceMax.innerHTML = formatCurrencyVND(maxVal)
                })
            })
        }
    }
    RangeChange();
    function CheckRemoveAcitve() {
        const recheckItem = document.querySelectorAll('.fil .recheck-item');
        if (recheckItem) {
            recheckItem.forEach(item => {
                item.classList.remove('active')
                const input = item.querySelector('input')
                if (input.checked) {
                    input.checked = false
                }
            })
        }
    }
    const resetFil = document.querySelector('.resetFil');
    if (resetFil) {
        resetFil.addEventListener('click', (e) => {
            e.preventDefault();
            rangeInputs[0].value = localMinVal
            rangeInputs[1].value = localMaxVal
            RangeChange();
            CheckRemoveAcitve();
        })
    }

    function formatCurrencyVND(amount) {
        // Sử dụng toLocaleString để định dạng số và chuyển đổi thành chuỗi
        const formattedAmount = amount.toLocaleString('vi-VN', {
            style: 'currency',
            currency: 'VND',
            minimumFractionDigits: 0,
        });

        // Loại bỏ ký tự mặc định của tiền tệ (₫)
        const formattedAmountWithoutSymbol = formattedAmount.replace(/₫/g, '');

        // Thêm dấu chấm phân tách hàng nghìn vào chuỗi
        const formattedAmountWithDotSeparator = formattedAmountWithoutSymbol.replace(/\B(?=(\d{3})+(?!\d))/g, '.');

        // Thêm chữ "đ" vào cuối chuỗi
        return formattedAmountWithDotSeparator + 'đ';
    }

}