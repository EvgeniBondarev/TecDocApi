// wwwroot/notification.js
"use strict";

// Создаём подключение
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// Подписываемся на событие от сервера
connection.on("PopupNotification", (message) => {
    // Простейшее всплывающее окно
    alert("Уведомление от сервера:\n\n" + message);

    // TODO: вместо alert() можно показать кастомное модальное окно
});

// Запускаем соединение
connection.start()
    .then(() => console.log("SignalR connected"))
    .catch(err => console.error(err.toString()));
