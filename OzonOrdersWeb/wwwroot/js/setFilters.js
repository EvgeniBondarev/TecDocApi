function setArticleFilter(article) {
    document.getElementById('articleFilter').value = article;
    articleFilter.focus();
    document.getElementById('submitFilterButton').click();
}

function setDeliveryCityFilter(deliveryCity) {
    document.getElementById('deliveryCityFilter').value = deliveryCity;
    deliveryCityFilter.focus();
    document.getElementById('submitFilterButton').click();
}

function setShipmentNumberFilter(shipmentNumber) {
    document.getElementById('shipmentNumberFilter').value = shipmentNumber
    shipmentNumberFilter.focus();
    document.getElementById('submitFilterButton').click();
}