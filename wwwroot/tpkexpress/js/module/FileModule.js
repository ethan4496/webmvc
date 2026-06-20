export default function FileModule() {
    const Upload = document.querySelector(".upLoadFile");
    if (Upload) {
        const wrap = Upload.querySelector(".upLoadFileWrap");
        const btnUpload = Upload.querySelector(".upLoadBtn");
        const input = Upload.querySelector(".upLoadInput");
        const accHtml = Upload.querySelector('.acc-up-ava .inner')
        btnUpload.addEventListener("click", () => {
            input.click();
        });

        const dataTransfer = new DataTransfer();
        input?.addEventListener("change", (e) => {
            var files = e.target.files,
                file;
            for (var i = 0; i < files.length; i++) {
                file = files[i];
                dataTransfer.items.add(file);

                $(wrap).append(
                    `
            <div class="dt-images-item">
                                    <div class="img"> <img src="${URL.createObjectURL(file)}" alt=""><i class="fa-solid fa-xmark upLoadFileDelete"></i></div>
            </div>
            `);
                $(accHtml).html(`<img src="${URL.createObjectURL(file)}" alt="">`);
                console.log(URL.createObjectURL(file))
            }
            input.files = dataTransfer.files;


            var removeItem = function (fileEle) {
                const listClose = Upload.querySelectorAll(".upLoadFileDelete");
                listClose.forEach((ele, i) => {
                    ele.addEventListener("click", (e) => {
                        $(ele).parent().closest(".dt-images-item").remove();
                        const dataList = dataTransfer.items;
                        for (let j = dataList.length - 1; j >= 0; j--) {
                            if (i === j) {
                                if (dataList[i].kind === "file") {
                                    dataList.remove(i);
                                }
                            }
                        }
                        input.files = dataTransfer.files;
                        console.log(input.files)
                    });
                });
            };

            removeItem(file);
        });
    }
}