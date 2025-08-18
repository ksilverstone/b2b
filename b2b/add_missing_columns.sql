-- Cart tablosuna BuyerCompanyId kolonu ekle (eğer yoksa)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Carts' AND COLUMN_NAME = 'BuyerCompanyId')
BEGIN
    ALTER TABLE Carts ADD BuyerCompanyId INT NULL;
    PRINT 'BuyerCompanyId kolonu Carts tablosuna eklendi.';
END
ELSE
BEGIN
    PRINT 'BuyerCompanyId kolonu Carts tablosunda zaten mevcut.';
END

-- ProductRequests tablosuna BuyerCompanyId kolonu ekle (eğer yoksa)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductRequests' AND COLUMN_NAME = 'BuyerCompanyId')
BEGIN
    ALTER TABLE ProductRequests ADD BuyerCompanyId INT NULL;
    PRINT 'BuyerCompanyId kolonu ProductRequests tablosuna eklendi.';
END
ELSE
BEGIN
    PRINT 'BuyerCompanyId kolonu ProductRequests tablosunda zaten mevcut.';
END

-- Foreign key constraint'leri ekle
-- Cart tablosu için
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Carts_BuyerCompany')
BEGIN
    ALTER TABLE Carts 
    ADD CONSTRAINT FK_Carts_BuyerCompany 
    FOREIGN KEY (BuyerCompanyId) REFERENCES Companies(Id);
    PRINT 'FK_Carts_BuyerCompany foreign key constraint eklendi.';
END
ELSE
BEGIN
    PRINT 'FK_Carts_BuyerCompany foreign key constraint zaten mevcut.';
END

-- ProductRequests tablosu için
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProductRequests_BuyerCompany')
BEGIN
    ALTER TABLE ProductRequests 
    ADD CONSTRAINT FK_ProductRequests_BuyerCompany 
    FOREIGN KEY (BuyerCompanyId) REFERENCES Companies(Id);
    PRINT 'FK_ProductRequests_BuyerCompany foreign key constraint eklendi.';
END
ELSE
BEGIN
    PRINT 'FK_ProductRequests_BuyerCompany foreign key constraint zaten mevcut.';
END

-- Mevcut kayıtları güncelle (eğer gerekirse)
-- Cart tablosundaki mevcut kayıtlar için BuyerCompanyId'yi Customer'ın CompanyId'si olarak ayarla
UPDATE c 
SET c.BuyerCompanyId = cu.CompanyId
FROM Carts c
INNER JOIN Customers cu ON c.CustomerId = cu.Id
WHERE c.BuyerCompanyId IS NULL;

-- ProductRequests tablosundaki mevcut kayıtlar için BuyerCompanyId'yi Customer'ın CompanyId'si olarak ayarla
UPDATE pr 
SET pr.BuyerCompanyId = cu.CompanyId
FROM ProductRequests pr
INNER JOIN Customers cu ON pr.CustomerId = cu.Id
WHERE pr.BuyerCompanyId IS NULL;

PRINT 'Mevcut kayıtlar güncellendi.';
