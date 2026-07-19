function renderPagination(currentPage, totalPages) {
    let html = '';

    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${currentPage - 1}">
                Previous
            </a>
        </li>
    `;

    const start = Math.max(1, currentPage - 2);
    const end = Math.min(totalPages, currentPage + 2);

    if (start > 1) {
        html += `
            <li class="page-item">
                <a class="page-link" href="#" data-page="1">1</a>
            </li>
        `;

        if (start > 2) {
            html += `
                <li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>
            `;
        }
    }

    for (let i = start; i <= end; i++) {
        html += `
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">
                    ${i}
                </a>
            </li>
        `;
    }

    if (end < totalPages) {
        if (end < totalPages - 1) {
            html += `
                <li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>
            `;
        }

        html += `
            <li class="page-item">
                <a class="page-link" href="#" data-page="${totalPages}">
                    ${totalPages}
                </a>
            </li>
        `;
    }

    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${currentPage + 1}">
                Next
            </a>
        </li>
    `;

    $('.pagination').html(html);
}
function loadPage(page) {
    const fromDateInput = $('#fromDate').val();
    const toDateInput = $('#toDate').val();
    const fromDate = fromDateInput ? parseDate(fromDateInput) : null;
    const toDate = toDateInput ? parseDate(toDateInput) : null;
    const name = $('#name').val();

    const filters = {
        'PageIndex': page > 1 ? page : 1,
        'PageSize': 20,
        'FromDate': fromDate ? fromDate : '',
        'ToDate': toDate ? toDate : '',
    };
    const params = {};

    Object.keys(filters).forEach(key => {
        params[key] = filters[key]
    });

    $.ajax({
        url: '/Contact/GetContactListPaging',
        data: params,
        type: 'GET',
        beforeSend: function () {
            showLoading();
        },
        success: function (data) {
            renderPagination(data.CurrentPage, Math.ceil(data.TotalItems / data.PageSize))
            if (!data.items || data.items.length == 0) {
                $('#results').html(``);
                return;
            }
            let html = ``;
            data.items.forEach((item) => {
                html += `<tr class="contact-${item.Id}">
                    <td>${item.Id}</td>
                    <td>
                        ${item.Name}
                    </td>
                    <td>
                        ${item.CreatedStr}
                    </td>
                    <td>
                        <a class="btn btn-primary" href="/contact-detail/${item.Id}" target="_blank">Chi tiết</a>
                        <button class="btn btn-danger" onclick="removeList(${item.Id})" target="_blank">Xóa</button>
                    </td>
                </tr>`
            })
            $('#results tbody').html(html);
        },
        complete: function () {
            hideLoading();
        },
        error: function () {
            alert('Lỗi khi tải dữ liệu');
        }
    });
}
const removeList = (id) => {
    const isConfirm = confirm(`Bạn có chắc chắn muốn xóa dòng này`);
    const parent = $('.contact-' + id);
    if (isConfirm) {
        $.ajax({
            url: '/Contact/Delete',
            type: 'DELETE',
            data: { Id: id },
            success: function (response) {
                showToast(response.Message, response.Type, false);
                parent.fadeOut(300);
            },
            error: function (response) {
                showToast(response.responseJSON.Message, response.responseJSON.Type, false);
            },
            complete: function () {
                hideLoading();
            }
        });
    }
}
function deleteContact(id, contactListId) {
    const isConfirm = confirm(`Bạn có chắc chắn muốn xóa dòng này`);
    const parent = $('.contact-' + id);
    if (isConfirm) {
        $.ajax({
            url: '/Contact/DeleteContact',
            type: 'DELETE',
            data: { Id: id, ContactListId: contactListId },
            success: function (response) {
                showToast(response.Message, response.Type, false);
                parent.fadeOut(300);
            },
            error: function (response) {
                showToast(response.responseJSON.Message, response.responseJSON.Type, false);
            },
            complete: function () {
                hideLoading();
            }
        });
    }
}
function editContact(id) {
    var row = $('.contact-' + id);
    const firstName = row.find('td').eq(1).text().trim();
    const lastName = row.find('td').eq(2).text().trim();
    const email = row.find('td').eq(3).text().trim();
    $('#contact-id').val(id);
    $('#firstNameEdit').val(firstName);
    $('#lastNameEdit').val(lastName);
    $('#emailEdit').val(email);
    new bootstrap.Modal(document.getElementById('modal-edit')).show();
}
function submitEdit() {
    const id = $('#contact-id').val();
    const firstName = $('#firstNameEdit').val();
    const lastName = $('#lastNameEdit').val();
    const email = $('#emailEdit').val();
    const data = {
        id: parseInt(id),
        FirstName: firstName,
        LastName: lastName,
        Email: email,
    }
    $.ajax({
        url: '/Contact/EditContact',
        data: JSON.stringify(data),
        contentType: 'application/json',
        type: 'PUT',
        beforeSend: function () {
            showLoading();
        },
        success: function (data) {
            if (data.success) {
                showToast(data.success.Message, 'success', true);
                bootstrap.Modal.getInstance(document.getElementById('modal-edit')).hide();
            }
            hideLoading();
        },
        complete: function () {
            hideLoading();
        },
        error: function () {
            alert('Lỗi khi tải dữ liệu');
        }
    });
}

function addMoreContact(id) {

    const firstName = $('#firstName').val();
    const lastName = $('#lastName').val();
    const email = $('#email').val();
    $('#formError').hide()
    if (!firstName || !lastName || !email) {
        $('#formError').html(`Vui lòng nhập đủ thông tin`)
        $('#formError').show()
        return;
    }
    const data = {
        Id: id,
        FirstName: firstName,
        LastName: lastName,
        Email: email,
    }
    $.ajax({
        url: '/Contact/AddContact',
        data: data,
        type: 'POST',
        beforeSend: function () {
            showLoading();
        },
        success: function (data) {
            if (data.success) {
                const contact = data.data
                const html = `<tr class="contact-${contact.Id}">
                    <td class="text-center">${contact.Id}</td>
                    <td class="text-center">${contact.FirstName}</td>
                    <td class="text-center">${contact.LastName}</td>
                    <td class="text-center">${contact.Email}</td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-danger" onclick="deleteContact(${contact.Id} , ${id})">
                            <i class="bi bi-trash"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-primary" onclick="editContact(${contact.Id})">
                            <i class="bi bi-pencil"></i>
                        </button>
                    </td>
                </tr>`
                $('#results tbody').prepend(html);
                $('#addContactForm')[0].reset();
                bootstrap.Modal.getInstance(document.getElementById('modal-add')).hide();
            }
            hideLoading();
        },
        complete: function () {
            hideLoading();
        },
        error: function () {
            alert('Lỗi khi tải dữ liệu');
        }
    });
}
function showLoading() {
    $('#loading').show();
}

function hideLoading() {
    $('#loading').hide();
}
const dropzone = document.getElementById('dropzone');
const fileInput = document.getElementById('fileInput');
const dropContent = document.getElementById('dropContent');
const fileInfo = document.getElementById('fileInfo');
const fileName = document.getElementById('fileName');
const fileSize = document.getElementById('fileSize');
const browseBtn = document.getElementById('browseBtn');
const removeBtn = document.getElementById('removeBtn');
const errorMsg = document.getElementById('errorMsg');

function formatSize(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
}

function showFile(file) {
    const validExt = /\.(xlsx|xls)$/i.test(file.name);
    if (!validExt) {
        errorMsg.textContent = 'Chỉ hỗ trợ file .xlsx hoặc .xls';
        errorMsg.style.display = 'block';
        return;
    }
    errorMsg.style.display = 'none';
    fileName.textContent = file.name;
    fileSize.textContent = formatSize(file.size);
    dropContent.style.display = 'none';
    fileInfo.style.display = 'flex';
}

if (dropzone) {
    browseBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        fileInput.click();
    });

    dropzone.addEventListener('click', function () {
        if (fileInfo.style.display !== 'flex') fileInput.click();
    });

    fileInput.addEventListener('change', function () {
        if (fileInput.files.length > 0) showFile(fileInput.files[0]);
    });

    dropzone.addEventListener('dragover', function (e) {
        e.preventDefault();
        dropzone.classList.add('dragover');
    });

    dropzone.addEventListener('dragleave', function () {
        dropzone.classList.remove('dragover');
    });

    dropzone.addEventListener('drop', function (e) {
        e.preventDefault();
        dropzone.classList.remove('dragover');
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            fileInput.files = files;
            showFile(files[0]);
        }
    });

    removeBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        fileInput.value = '';
        fileInfo.style.display = 'none';
        dropContent.style.display = 'block';
        errorMsg.style.display = 'none';
    });
}
$(document).ready(function () {
    $('#submitContactBtn').on('click', function () {
        var id = $(this).attr("data-id");
        addMoreContact(id);
    })
    $('#submitEditContact').on('click', function () {
        submitEdit();
    })
    $('#contact-search').on('click', function(){
        var email = $('#email-search').val().trim();
        var url = new URL(window.location.href);
        if (email) {
            url.searchParams.set('email', email);
        } else {
            url.searchParams.delete('email');
        }
        window.location.href = url.toString();
    })
})