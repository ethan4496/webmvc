export default function IsotopeModule() {
    var itemSelector = '.isotopeItem';

    const isotope = document.querySelector('.isotopeGrid');
    if (isotope) {
        var $grid = $('.isotopeGrid').isotope({
            itemSelector: itemSelector,
        });

        var currentFilter = '*';
        var filterAtribute = 'data-filter';

        function changeFilter(selector) {
            $grid.isotope({
                filter: selector,
            });
        }

        function goToPage(n) {
            var selector = itemSelector;
            selector += (currentFilter != '*') ? '[' + filterAtribute + '="' + currentFilter + '"]' : ''; 

            changeFilter(selector);
        }

        goToPage(1);

        //Adicionando Event de Click para as categorias
        $('.isotopeTab').click(function () {
            $('.active.isotopeTab').removeClass('active');
            $(this).addClass('active');
            var filter = $(this).attr(filterAtribute);
            currentFilter = filter;
            goToPage(1);
        });
    }
}
