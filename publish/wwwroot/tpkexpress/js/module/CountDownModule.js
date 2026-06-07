export default function CountDownModule() {
    const timeItems = document.querySelectorAll('.cdw')
    if (timeItems) {
        timeItems.forEach(items => {
            const daysEl = items.querySelector(".date");
            const hoursEl = items.querySelector(".hour");
            const minsEl = items.querySelector(".minute");
            const secondsEl = items.querySelector(".second");
            if (daysEl) {
                const dates = items.getAttribute("data-time");
                const newYears = dates;

                function countdown() {
                    const newYearsDate = new Date(newYears);
                    const currentDate = new Date();

                    const totalSeconds = (newYearsDate - currentDate) / 1000;

                    const days = Math.floor(totalSeconds / 3600 / 24);
                    const hours = Math.floor(totalSeconds / 3600) % 24;
                    const mins = Math.floor(totalSeconds / 60) % 60;
                    const seconds = Math.floor(totalSeconds) % 60;

                    daysEl.innerHTML = days;
                    hoursEl.innerHTML = formatTime(hours);
                    minsEl.innerHTML = formatTime(mins);
                    secondsEl.innerHTML = formatTime(seconds);

                    if (days < 0) {
                        daysEl.innerHTML = 0;
                        hoursEl.innerHTML = 0;
                        minsEl.innerHTML = 0;
                        secondsEl.innerHTML = 0;
                    }
                }

                function formatTime(time) {
                    return time < 10 ? `0${time}` : time;
                }

                // initial call
                countdown();

                setInterval(countdown, 1000);
            }
        })
    }


    function countDownGGS() {
        // Lấy thời gian hiện tại
        const timeDowns = document.querySelectorAll('.timeDown');
        if (timeDowns) {
            timeDowns.forEach(item => {
                const dataTime = item.getAttribute("data-time");
                const timeDownHtml = item.querySelector('.timeDownHtml')

                const endDate = new Date(dataTime).getTime();
                const currentDate = new Date().getTime();

                // Tính toán thời gian còn lại (tính bằng mili giây)
                const timeLeft = endDate - currentDate;

                // Tính toán giờ, phút, giây từ thời gian còn lại
                const days = Math.floor(timeLeft / (1000 * 60 * 60 * 24));
                const hours = Math.floor((timeLeft % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
                const seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);

                // Hiển thị kết quả trong phần tử có id tương ứng
                timeDownHtml.innerHTML = `${days}n ${hours}g ${minutes}p ${seconds}s`;

                // Nếu thời gian còn lại <= 0, hiển thị thông báo hoặc thực hiện hành động cụ thể
                if (timeLeft <= 0) {
                    timeDownHtml.innerHTML = "Hết thời gian!";
                    item.classList.add('disable')
                    // Thực hiện hành động sau khi hết thời gian (ví dụ: hiển thị thông báo)
                }
            })
        }

    }

    countDownGGS();
    var intervalId = setInterval(() => {
        countDownGGS();
    }, 1000);
}