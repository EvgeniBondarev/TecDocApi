DROP TRIGGER IF EXISTS `trg_orders_after_insert`;
delimiter ;;
CREATE TRIGGER `trg_orders_after_insert` AFTER INSERT ON `Orders` FOR EACH ROW BEGIN
    DECLARE changed_by_user VARCHAR(255);
    
    SET changed_by_user = COALESCE(NEW.UpdatedBy, 'system');
    
    -- Записываем создание новой записи
    INSERT INTO orders_history (OrderId, ColumnName, ColumnDisplayName, OldValue, NewValue, ChangedBy)
    VALUES (NEW.Id, 'RecordCreated', 'Создание записи', NULL, 'Новая запись создана', changed_by_user);

END