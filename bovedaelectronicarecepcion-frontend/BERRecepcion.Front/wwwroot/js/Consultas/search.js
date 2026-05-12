
$(document).on('keyup', ".search", function (e) {
    if (e.keyCode === 13) {
        let text = $(this).val().trim();
        if (!itemSearchExists(text) && !isNull(text)) {
            $("#searchTerms").append(
                `<span class="badge badge-pill border border-primary mr-1 mb-2">
                    <span name="badgeTerms">${text}</span><i class="fas fa-times ml-2" name="deleteTerm" style="cursor: pointer;"></i>
                </span>`
            );
            try {
                updateSearch();
            }
            catch (error) {}
        }
        $(this).val('');
    }
});

$(document).on('click', 'i[name="deleteTerm"]', function (e) {
    e.preventDefault();
    $(this).parent().remove();
    try {
        updateSearch();
    }
    catch (error) {}
    e.stopImmediatePropagation();
});

let itemSearchExists = function (_item) {
    let exists = false;
    $.each($(document).find('span[name="badgeTerms"]'), function (index, item) {
        exists = $(item).text() === _item;
    });
    return exists;
}