DROP TRIGGER IF EXISTS trg_orders_after_update;
DELIMITER $$

CREATE TRIGGER trg_orders_after_update
    AFTER UPDATE ON Orders
    FOR EACH ROW
BEGIN
    DECLARE changed_by_user VARCHAR(255);
    DECLARE old_name_value VARCHAR(255);
    DECLARE new_name_value VARCHAR(255);
    DECLARE old_value_str VARCHAR(255);
    DECLARE new_value_str VARCHAR(255);
    
    -- Получаем пользователя, который внес изменения
    SET changed_by_user = COALESCE(NEW.UpdatedBy, 'system');
    
    -- Универсальная функция для безопасного сравнения значений
    -- ShipmentNumber
    IF NOT (OLD.ShipmentNumber <=> NEW.ShipmentNumber) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'ShipmentNumber', 'Номер отправления', 
                COALESCE(OLD.ShipmentNumber, 'NULL'), 
                COALESCE(NEW.ShipmentNumber, 'NULL'), 
                changed_by_user);
END IF;

-- ProcessingDate
IF NOT (OLD.ProcessingDate <=> NEW.ProcessingDate) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'ProcessingDate', 'Принят в обработку', 
                COALESCE(CAST(OLD.ProcessingDate AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.ProcessingDate AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- ShippingDate
    IF NOT (OLD.ShippingDate <=> NEW.ShippingDate) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'ShippingDate', 'Дата отгрузки', 
                COALESCE(CAST(OLD.ShippingDate AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.ShippingDate AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- Status
    IF NOT (OLD.Status <=> NEW.Status) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'Status', 'Статус Ozon', 
                COALESCE(OLD.Status, 'NULL'), 
                COALESCE(NEW.Status, 'NULL'), 
                changed_by_user);
END IF;
    
    -- AppStatusId
    IF NOT (OLD.AppStatusId <=> NEW.AppStatusId) THEN
        SET old_name_value = NULL;
        SET new_name_value = NULL;
        
        IF OLD.AppStatusId IS NOT NULL THEN
SELECT Name INTO old_name_value FROM AppStatuses WHERE Id = OLD.AppStatusId;
END IF;
        
        IF NEW.AppStatusId IS NOT NULL THEN
SELECT Name INTO new_name_value FROM AppStatuses WHERE Id = NEW.AppStatusId;
END IF;

INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
VALUES (NEW.Id, 'AppStatusId', 'Статус в отчете',
        COALESCE(old_name_value, 'NULL'),
        COALESCE(new_name_value, 'NULL'),
        changed_by_user);
END IF;
    
    -- ShipmentAmount (decimal)
    IF NOT (OLD.ShipmentAmount <=> NEW.ShipmentAmount) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'ShipmentAmount', 'Сумма отправления', 
                COALESCE(CAST(OLD.ShipmentAmount AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.ShipmentAmount AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- ProductName
    IF NOT (OLD.ProductName <=> NEW.ProductName) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'ProductName', 'Наименование', 
                COALESCE(OLD.ProductName, 'NULL'), 
                COALESCE(NEW.ProductName, 'NULL'), 
                changed_by_user);
END IF;
    
    -- ProductKey
    IF NOT (OLD.ProductKey <=> NEW.ProductKey) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'ProductKey', 'Ключ товара', 
                COALESCE(OLD.ProductKey, 'NULL'), 
                COALESCE(NEW.ProductKey, 'NULL'), 
                changed_by_user);
END IF;
    
    -- Article
    IF NOT (OLD.Article <=> NEW.Article) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'Article', 'Артикул', 
                COALESCE(OLD.Article, 'NULL'), 
                COALESCE(NEW.Article, 'NULL'), 
                changed_by_user);
END IF;
    
    -- ManufacturerId
    IF NOT (OLD.ManufacturerId <=> NEW.ManufacturerId) THEN
        SET old_name_value = NULL;
        SET new_name_value = NULL;
        
        IF OLD.ManufacturerId IS NOT NULL THEN
SELECT Name INTO old_name_value FROM Manufacturers WHERE Id = OLD.ManufacturerId;
END IF;
        
        IF NEW.ManufacturerId IS NOT NULL THEN
SELECT Name INTO new_name_value FROM Manufacturers WHERE Id = NEW.ManufacturerId;
END IF;

INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
VALUES (NEW.Id, 'ManufacturerId', 'Производитель',
        COALESCE(old_name_value, 'NULL'),
        COALESCE(new_name_value, 'NULL'),
        changed_by_user);
END IF;
    
    -- Price (decimal)
    IF NOT (OLD.Price <=> NEW.Price) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'Price', 'Цена', 
                COALESCE(CAST(OLD.Price AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.Price AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- Quantity (int)
    IF NOT (OLD.Quantity <=> NEW.Quantity) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'Quantity', 'Количество', 
                COALESCE(CAST(OLD.Quantity AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.Quantity AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- ShipmentWarehouseId
    IF NOT (OLD.ShipmentWarehouseId <=> NEW.ShipmentWarehouseId) THEN
        SET old_name_value = NULL;
        SET new_name_value = NULL;
        
        IF OLD.ShipmentWarehouseId IS NOT NULL THEN
SELECT Name INTO old_name_value FROM Warehouses WHERE Id = OLD.ShipmentWarehouseId;
END IF;
        
        IF NEW.ShipmentWarehouseId IS NOT NULL THEN
SELECT Name INTO new_name_value FROM Warehouses WHERE Id = NEW.ShipmentWarehouseId;
END IF;

INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
VALUES (NEW.Id, 'ShipmentWarehouseId', 'Склад отгрузки',
        COALESCE(old_name_value, 'NULL'),
        COALESCE(new_name_value, 'NULL'),
        changed_by_user);
END IF;
    
    -- SupplierId
    IF NOT (OLD.SupplierId <=> NEW.SupplierId) THEN
        SET old_name_value = NULL;
        SET new_name_value = NULL;
        
        IF OLD.SupplierId IS NOT NULL THEN
SELECT Name INTO old_name_value FROM Suppliers WHERE Id = OLD.SupplierId;
END IF;
        
        IF NEW.SupplierId IS NOT NULL THEN
SELECT Name INTO new_name_value FROM Suppliers WHERE Id = NEW.SupplierId;
END IF;

INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
VALUES (NEW.Id, 'SupplierId', 'Поставщик',
        COALESCE(old_name_value, 'NULL'),
        COALESCE(new_name_value, 'NULL'),
        changed_by_user);
END IF;
    
    -- OrderNumberToSupplier
    IF NOT (OLD.OrderNumberToSupplier <=> NEW.OrderNumberToSupplier) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'OrderNumberToSupplier', 'Номер заказа', 
                COALESCE(OLD.OrderNumberToSupplier, 'NULL'), 
                COALESCE(NEW.OrderNumberToSupplier, 'NULL'), 
                changed_by_user);
END IF;
    
    -- PurchasePrice (decimal)
    IF NOT (OLD.PurchasePrice <=> NEW.PurchasePrice) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'PurchasePrice', 'Цена закупки', 
                COALESCE(CAST(OLD.PurchasePrice AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.PurchasePrice AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- OriginalPurchasePrice (decimal)
    IF NOT (OLD.OriginalPurchasePrice <=> NEW.OriginalPurchasePrice) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'OriginalPurchasePrice', 'Цена закупки до перевода', 
                COALESCE(CAST(OLD.OriginalPurchasePrice AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.OriginalPurchasePrice AS CHAR), 'NULL'), 
                changed_by_user);
END IF;

    -- MinOzonCommission (decimal)
    IF NOT (OLD.MinOzonCommission <=> NEW.MinOzonCommission) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'MinOzonCommission', 'Минимальная комиссия ОЗОН', 
                COALESCE(CAST(OLD.MinOzonCommission AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.MinOzonCommission AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- MaxOzonCommission (decimal)
    IF NOT (OLD.MaxOzonCommission <=> NEW.MaxOzonCommission) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'MaxOzonCommission', 'Максимальная комиссия ОЗОН', 
                COALESCE(CAST(OLD.MaxOzonCommission AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.MaxOzonCommission AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- MinProfit (decimal)
    IF NOT (OLD.MinProfit <=> NEW.MinProfit) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'MinProfit', 'Минимальная прибыль', 
                COALESCE(CAST(OLD.MinProfit AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.MinProfit AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- MaxProfit (decimal)
    IF NOT (OLD.MaxProfit <=> NEW.MaxProfit) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'MaxProfit', 'Максимальная прибыль', 
                COALESCE(CAST(OLD.MaxProfit AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.MaxProfit AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- MinDiscount (decimal)
    IF NOT (OLD.MinDiscount <=> NEW.MinDiscount) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'MinDiscount', 'Минимальная скидка, %', 
                COALESCE(CAST(OLD.MinDiscount AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.MinDiscount AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- MaxDiscount (decimal)
    IF NOT (OLD.MaxDiscount <=> NEW.MaxDiscount) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'MaxDiscount', 'Максимальная скидка, %', 
                COALESCE(CAST(OLD.MaxDiscount AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.MaxDiscount AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- CostPrice (decimal)
    IF NOT (OLD.CostPrice <=> NEW.CostPrice) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'CostPrice', 'Себестоимость', 
                COALESCE(CAST(OLD.CostPrice AS CHAR), 'NULL'), 
                COALESCE(CAST(NEW.CostPrice AS CHAR), 'NULL'), 
                changed_by_user);
END IF;
    
    -- DeliveryCity
    IF NOT (OLD.DeliveryCity <=> NEW.DeliveryCity) THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'DeliveryCity', 'Город доставки', 
                COALESCE(OLD.DeliveryCity, 'NULL'), 
                COALESCE(NEW.DeliveryCity, 'NULL'), 
                changed_by_user);
END IF;
    
    -- IsVerified (boolean)
    IF OLD.IsVerified <> NEW.IsVerified THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'IsVerified', 'Статус обработки', 
                CAST(OLD.IsVerified AS CHAR), 
                CAST(NEW.IsVerified AS CHAR), 
                changed_by_user);
END IF;
    
    -- IsAccepted (boolean)
    IF OLD.IsAccepted <> NEW.IsAccepted THEN
        INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
        VALUES (NEW.Id, 'IsAccepted', 'Принят', 
                CAST(OLD.IsAccepted AS CHAR), 
                CAST(NEW.IsAccepted AS CHAR), 
                changed_by_user);
END IF;
    
    -- OzonClientId
    IF NOT (OLD.OzonClientId <=> NEW.OzonClientId) THEN
        SET old_name_value = NULL;
        SET new_name_value = NULL;
        
        IF OLD.OzonClientId IS NOT NULL THEN
SELECT Name INTO old_name_value FROM OzonClients WHERE Id = OLD.OzonClientId;
END IF;
        
        IF NEW.OzonClientId IS NOT NULL THEN
SELECT Name INTO new_name_value FROM OzonClients WHERE Id = NEW.OzonClientId;
END IF;

INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
VALUES (NEW.Id, 'OzonClientId', 'Ozon клиент',
        COALESCE(old_name_value, 'NULL'),
        COALESCE(new_name_value, 'NULL'),
        changed_by_user);
END IF;
    
END$$

DELIMITER ;