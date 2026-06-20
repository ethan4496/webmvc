const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();


// Hiển thị cửa sổ chat
const chatWindow = document.getElementById("chatWindow");
if (chatWindow) {
    chatWindow.style.display = "block"

    // Đóng cửa sổ chat
    document.getElementById("closeChat").addEventListener("click", function (event) {
        // Kiểm tra xem sự kiện không phải đến từ nút backButton
        if (!event.target.closest("#backButton")) {
            // Kiểm tra xem cửa sổ chat hiện đang được hiển thị hay không
            const chatList = document.getElementById("chat-body");

            // Nếu cửa sổ đang hiển thị, ẩn nó; nếu không, hiển thị lại
            if (chatList.style.display === "none" || chatList.style.display === "") {
                chatList.style.display = "block";  // Hiển thị cửa sổ chat
                document.getElementById("chatWindow").style.height = "550px";
                document.getElementById("chatWindow").style.width = "810px";
                localStorage.setItem("closeChat", false);
            } else {
                chatList.style.display = "none";  // Ẩn cửa sổ chat
                document.getElementById("chatWindow").style.height = "unset";
                document.getElementById("chatWindow").style.width = "400px";
                localStorage.setItem("closeChat", true);
            }
        }
    });
    document.addEventListener("DOMContentLoaded", function () {
        const chatList = document.getElementById("chat-body");
        const chatWindow = document.getElementById("chatWindow");

        // Kiểm tra trạng thái đã lưu
        const isCloseChat = localStorage.getItem("closeChat") === "true";

        // Cập nhật trạng thái sidebar khi tải lại trang
        if (isCloseChat) {
            chatList.style.display = "none";
            chatWindow.style.height = "unset";
            chatWindow.style.width = "400px";

        } else {
            chatList.style.display = "block";
            chatWindow.style.height = "550px";
            chatWindow.style.width = "810px";
        }
    });
}
// Kết nối SignalR
async function start() {
    try {
        await connection.start();
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});
// Start the connection.
start();



// Nhận thông báo tin nhắn từ server
connection.on("ReceiveMessageNotification", (receiverId) => {
    document.getElementById("notificationDot").style.display = "inline-block";
    const closeChatBtn = document.getElementById("closeChat");
    const chatBox = document.getElementById("chat-body");

    if (
        closeChatBtn &&
        chatBox &&
        getComputedStyle(chatBox).display === "none"
    ) {
        closeChatBtn.click();
    }
});

// Nhận thông báo toast từ server
connection.on("ReceiveToastNotification", (message) => {
    showToast(message, 4, false);
    document.getElementById("notificationBadge").style.display = "inline-block";
});

connection.on("RemoteLogout", function () {
    showToast("Tài khoản đã bị thay đổi, vui lòng đăng nhập lại", 2, false);
    setTimeout(
        function () {
            window.location.href = "/Home/SignOff";
        },
        2000
    );
});
