function clearForm() {
    var form = document.getElementById('filterForm');

    var elements = form.elements;

    for (var i = 0; i < elements.length; i++) {
        var element = elements[i];
        if (element.tagName === 'INPUT' || element.tagName === 'SELECT') {
            element.value = ''; // Сброс значения
        }
    }
    form.submit();
}
