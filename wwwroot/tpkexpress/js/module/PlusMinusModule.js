export default function PlusMinusModule() {
    var event = new Event("change");
    const count = document.querySelectorAll('.count')
    if (count) {
        count.forEach(item => {
            const input = item.querySelector('.count-input');
            const number = item.querySelector('.count-number');
            number.innerHTML = `${input.value < 10 && input.value > 0 ? "0" : " "}` + input.value;
        })
    }
    document.addEventListener('click', (e) => {
        const count = e.target.closest('.count');
        const countBtnPlus = e.target.closest('.count-btn.count-plus')
        const countBtnMinus = e.target.closest('.count-btn.count-minus');
        if (count) {
            const countNumber = count.querySelector('.count-number');
            const countInput = count.querySelector('.count-input');
            console.log(countNumber);
            if (countBtnPlus) {
                if (parseInt(countInput.value) < parseInt(countInput.max)) {
                    countInput.value++;
                    countNumber.innerHTML =
                        `${countInput.value < 10 && countInput.value > 0 ? "0" : " "}` + countInput.value;
                    countInput.dispatchEvent(event);
                    $(countInput).trigger("change");
                }
            }
            if (countBtnMinus) {
                if (parseInt(countInput.value) > parseInt(countInput.min)) {
                    countInput.value--;
                    countNumber.innerHTML =
                        `${countInput.value < 10 && countInput.value > 0 ? "0" : " "}` + countInput.value;
                    countInput.dispatchEvent(event);
                    $(countInput).trigger("change");
                }
            }

        }
    })

}