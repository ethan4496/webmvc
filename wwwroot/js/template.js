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
    const status = $('#status').val();
    const filters = {
        'PageIndex': page > 1 ? page : 1,
        'PageSize': 20,
        'Status': $('#status').val(),
        'FromDate': fromDate ? fromDate : '',
        'ToDate': toDate ? toDate : '',
    };
    const params = {};

    Object.keys(filters).forEach(key => {
        params[key] = filters[key]
    });

    $.ajax({
        url: '/Template/GetTemplatePaging',
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
                html += `<div class="card card-email ${'email-' + item.Id}">
                    <div class="card-body">
                        <div class="email-header">
                            <h5 class="card-title">${item.Name}</h5>
                            <h6 class="card-subtitle mb-2 text-muted">${item.Subject}</h6>
                            <p class="card-text mail-content">
                                ${item.Body}
                            </p>
                        </div>
                        <div class="card-actions">
                            <span>
                                ID: ${item.Id}
                            </span>
                            <a class="btn email-edit" href="/edit-template/${item.Id}"><i class="bi bi-pencil"></i></a>
                            <button class="btn email-remove" onclick="removeTempalte(${item.Id})"><i class="bi bi-x-circle"></i></button>
                        </div>
                    </div>
                </div>`
            })
            $('#results').html(html);
        },
        complete: function () {
            hideLoading();
        },
        error: function () {
            alert('Lỗi khi tải dữ liệu');
        }
    });
}
const removeTempalte = (id) => {
    const isConfirm = confirm(`Bạn có chắc chắn muốn xóa dòng này`);
    const parent = $('.email-' + id);
    if (isConfirm) {
        $.ajax({
            url: '/Template/Delete',
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
function showLoading() {
    $('#loading').show();
}

function hideLoading() {
    $('#loading').hide();
}
$(document).ready(function () {
    loadPage(1);
})