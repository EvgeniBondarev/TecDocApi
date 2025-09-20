function recoverOrder(id, start, end) {
    $.ajax({
        url: '/Orders/Recover',
        type: 'POST',
        data: { id: id, start: start, end: end },
        success: function (result) {
            // Перенаправить на страницу Index после успешного восстановления заказа
            window.location.href = '/Orders/Index';
        },
        error: function (xhr, status, error) {
            // Обработка ошибки, если таковая имеется
            console.error(xhr.responseText);
        }
    });
}