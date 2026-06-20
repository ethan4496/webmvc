export default function LoadModule() {
    const load = document.querySelector('.load')
    if (load) {
        setTimeout(() => {
            $(load).fadeOut();
        }, 1200)
    }
}