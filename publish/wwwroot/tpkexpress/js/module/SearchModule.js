export default function SearchModule() {
    const hdSIcon = document.querySelector('.hd-icon-search');
    const hdSForm = document.querySelector('.re-search');
    let isOpen = false;
    if (hdSIcon) {
        hdSIcon.addEventListener('click', () => {
            isOpen = !isOpen;
            if (isOpen) {
                hdSForm.classList.add('open')
            } else {
                hdSForm.classList.remove('open')
            }
        })
        window.addEventListener('click', (e) => {
            if (!e.target.closest('.hd-icon-search') && !e.target.closest('.re-search')) {
                isOpen = false;
                hdSForm.classList.remove('open')
            }
        })
    }

}