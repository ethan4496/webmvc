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
        url: '/Campaign/GetTemplatePaging',
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
                
                html += `<tr class="campaign-${item.Id}">
                    <td class="text-center">${item.Id}</td>
                    <td class="text-center">${item.Name}</td>
                    <td class="text-center">${item.StatusBadge}</td>
                    <td class="text-center">${item.AccountName}</td>
                    <td class="text-center">${item.EmailSent}</td>
                    <td class="text-center">${item.ContactListName}</td>
                    <td class="text-center">${item.Subject}</td>
                    <td class="text-center">${item.SendAtStr}</td>
                    <td class="text-center">
                        <a class="btn btn-primary" href="/edit-campaign/${item.Id}">Chi tiết</a>
                        <button class="btn btn-danger" onclick="removeCampagin(${item.Id})">Xóa</button>
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
const removeCampagin = (id) => {
    const isConfirm = confirm(`Bạn có chắc chắn muốn xóa dòng này`);
    const parent = $('.campaign-' + id);
    if (isConfirm) {
        $.ajax({
            url: '/Campaign/Delete',
            type: 'DELETE',
            data: { Id: id },
            success: function (response) {
                showToast(response.Message, response.Type, true);
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
