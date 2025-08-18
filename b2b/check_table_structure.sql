-- Cart ve ProductRequest tablolarının mevcut yapısını kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('Carts', 'ProductRequests')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- Tabloların foreign key ilişkilerini kontrol et
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS Table_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table_Name,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS Referenced_Column_Name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN ('Carts', 'ProductRequests');
