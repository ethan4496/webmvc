$(document).ready(function () {
    const swiper = new Swiper('#banner', {
        loop: true,
        pagination: {
            el: '.swiper-pagination',
        },
        scrollbar: {
            el: '.swiper-scrollbar',
        },
    });
    $('#find-transportation').on('click', function (e) {
        const status = {
            1: {
                text: "Đang xử lí",
                class: "badge-progress"
            },
            2: {
                text: "Nhập kho TQ",
                class: "badge-delivered",
            },
            3: {
                text: "Đã phát hàng",
                class: "badge-delivered",
            },
            4: {
                text: "Kiểm hóa",
                class: "badge-delivered",
            },
            5: {
                text: "Nhập kho HN",
                class: "badge-delivered",
            },
            6: {
                text: "Đã nhận hàng",
                class: "badge-completed",
            },
        }
        e.preventDefault();
        const barcode = $('#Barcode').val();
        if(!barcode) return;
        $.ajax({
            url: '/Transportation/GetTransportation',
            type: 'GET',
            data: { Barcode: barcode },
            success: function (response) {
                var data = response.Data;
                if(data){
                    var statusMatch = status?.[data?.Status]
                    $('.transportation-status .status-tag').text(statusMatch?.text);
                    $('.transportation-status .status-tag').addClass(statusMatch?.class);

                }
            },
            error: function (response) {
                
            }, complete: function () {
                
            }
        });
    })

    function escapeHtml(value) {
        return $('<div>').text(value ?? '').html();
    }

    function formatDate(value) {
        if (!value) return '';
        const date = new Date(value);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        return `${day}/${month}/${date.getFullYear()}`;
    }

    function renderPrimaryPost(post) {
        const $primary = $('.primary-post');
        if (!post) {
            $primary.hide();
            return;
        }
        $primary.show();
        $primary.find('.status-tag').text(post.Categories?.[0]?.Name ?? '');
        $primary.find('.image-article img').attr('src', post.Image ?? '');
        $primary.find('.time-read span').text(post.ReadingTimeMinutes ?? '');
        $primary.find('.article-created-at').text(formatDate(post.Created));
        $primary.find('h3').text(post.Title ?? '');
        $primary.find('p').text(post.Excerpt ?? '');
        $primary.find('.article-readmore a').attr('href', '/bai-viet/' + (post.Slug ?? ''));
    }

    function renderOtherPosts(primary, posts) {
        const others = (posts || []).filter(post => post.Id !== primary?.Id);
        $('.other-article .result-search').text(others.length + ' bài viết khác');

        const html = others.map(post => `
            <div class="article">
                <div class="image-article">
                    <img src="${escapeHtml(post.Image)}" alt="${escapeHtml(post.Title)}" />
                </div>
                <div class="article-info">
                    <div class="article-heading">
                        <div class="article-tag">
                            <div class="status-tag badge-progress">
                                ${escapeHtml(post.Categories?.[0]?.Name)}
                            </div>
                            <div class="d-flex align-items-center gap-1 time-read">
                                <img src="/tpkexpress/images/icon/Clock.svg" alt="">
                                <span>${escapeHtml(post.ReadingTimeMinutes)} phút đọc</span>
                            </div>
                            <div class="article-created-at">
                                ${formatDate(post.Created)}
                            </div>
                        </div>
                        <h3>${escapeHtml(post.Title)}</h3>
                        <p>${escapeHtml(post.Excerpt)}</p>
                    </div>
                    <div class="article-readmore">
                        <a href="/bai-viet/${escapeHtml(post.Slug)}" class="btn read-nore">
                            <span>Đọc thêm </span>
                            <img src="/tpkexpress/images/icon/ArrowRight.svg" alt="ArrowRight">
                        </a>
                    </div>
                </div>
            </div>
        `).join('');

        $('.other-article .list-articles').html(html);
    }

    if ($('#news-loading-style').length === 0) {
        $('<style id="news-loading-style">')
            .text(`
                .news-loading-overlay {
                    position: absolute;
                    inset: 0;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    background: rgba(255, 255, 255, 0.6);
                    z-index: 10;
                }
                .news-spinner {
                    width: 40px;
                    height: 40px;
                    border: 4px solid #e0e0e0;
                    border-top-color: #d0021b;
                    border-radius: 50%;
                    animation: news-spin 0.8s linear infinite;
                }
                .sectin-posts-wrapper.is-loading {
                    pointer-events: none;
                }
                @keyframes news-spin {
                    to { transform: rotate(360deg); }
                }
            `)
            .appendTo('head');
    }

    function setNewsLoading(isLoading) {
        const $wrapper = $('.sectin-posts-wrapper');
        $wrapper.toggleClass('is-loading', isLoading);
        if (isLoading && $wrapper.find('.news-loading-overlay').length === 0) {
            $wrapper.css('position', 'relative').append('<div class="news-loading-overlay"><div class="news-spinner"></div></div>');
        } else if (!isLoading) {
            $wrapper.find('.news-loading-overlay').remove();
        }
    }

    $('.post-categories .categories').on('click', 'li', function () {
        const $li = $(this);
        if ($li.hasClass('active')) return;

        const categoryId = $li.data('id');

        $li.siblings().removeClass('active');
        $li.addClass('active');

        setNewsLoading(true);
        $.ajax({
            url: '/api/tin-tuc',
            type: 'GET',
            data: categoryId ? { categoryId: categoryId } : {},
            success: function (response) {
                const data = response?.Data;
                if (!data) return;
                renderPrimaryPost(data.PrimaryPost);
                renderOtherPosts(data.PrimaryPost, data.Posts);
            },
            error: function () {
            },
            complete: function () {
                setNewsLoading(false);
            }
        });
    })
})