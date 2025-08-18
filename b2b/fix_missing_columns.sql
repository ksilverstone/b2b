-- =====================================================
-- EKSİK KOLONLARI EKLEME SCRIPTİ - SADECE KOLON EKLEME
-- =====================================================

PRINT '=== Carts ve ProductRequests Tablolarına BuyerCompanyId Kolonu Ekleme ===';

-- 1. Carts tablosuna BuyerCompanyId kolonu ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Carts' AND COLUMN_NAME = 'BuyerCompanyId')
BEGIN
    ALTER TABLE Carts ADD BuyerCompanyId INT NULL;
    PRINT '✅ BuyerCompanyId kolonu Carts tablosuna eklendi.';
END
ELSE
BEGIN
    PRINT 'ℹ️ BuyerCompanyId kolonu Carts tablosunda zaten mevcut.';
END

-- 2. ProductRequests tablosuna BuyerCompanyId kolonu ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductRequests' AND COLUMN_NAME = 'BuyerCompanyId')
BEGIN
    ALTER TABLE ProductRequests ADD BuyerCompanyId INT NULL;
    PRINT '✅ BuyerCompanyId kolonu ProductRequests tablosuna eklendi.';
END
ELSE
BEGIN
    PRINT 'ℹ️ BuyerCompanyId kolonu ProductRequests tablosunda zaten mevcut.';
END

PRINT '';
PRINT '=== FOREIGN KEY CONSTRAINT''LERİ EKLEME ===';

-- 3. Carts tablosu için BuyerCompany foreign key constraint'i ekle
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Carts_Companies_BuyerCompanyId')
BEGIN
    ALTER TABLE Carts 
    ADD CONSTRAINT FK_Carts_Companies_BuyerCompanyId 
    FOREIGN KEY (BuyerCompanyId) REFERENCES Companies(Id);
    PRINT '✅ FK_Carts_Companies_BuyerCompanyId foreign key constraint eklendi.';
END
ELSE
BEGIN
    PRINT 'ℹ️ FK_Carts_Companies_BuyerCompanyId foreign key constraint zaten mevcut.';
END

-- 4. ProductRequests tablosu için BuyerCompany foreign key constraint'i ekle
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProductRequests_Companies_BuyerCompanyId')
BEGIN
    ALTER TABLE ProductRequests 
    ADD CONSTRAINT FK_ProductRequests_Companies_BuyerCompanyId 
    FOREIGN KEY (BuyerCompanyId) REFERENCES Companies(Id);
    PRINT '✅ FK_ProductRequests_Companies_BuyerCompanyId foreign key constraint eklendi.';
END
ELSE
BEGIN
    PRINT 'ℹ️ FK_ProductRequests_Companies_BuyerCompanyId foreign key constraint zaten mevcut.';
END

PRINT '';
PRINT '=== KOLONLARIN EKLENİP EKLENMEDİĞİNİ KONTROL ET ===';

-- 5. Carts tablosunda BuyerCompanyId kolonu var mı kontrol et
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Carts' AND COLUMN_NAME = 'BuyerCompanyId')
BEGIN
    PRINT '✅ Carts tablosunda BuyerCompanyId kolonu mevcut.';
END
ELSE
BEGIN
    PRINT '❌ Carts tablosunda BuyerCompanyId kolonu bulunamadı!';
END

-- 6. ProductRequests tablosunda BuyerCompanyId kolonu var mı kontrol et
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductRequests' AND COLUMN_NAME = 'BuyerCompanyId')
BEGIN
    PRINT '✅ ProductRequests tablosunda BuyerCompanyId kolonu mevcut.';
END
ELSE
BEGIN
    PRINT '❌ ProductRequests tablosunda BuyerCompanyId kolonu bulunamadı!';
END

PRINT '';
PRINT '=== İŞLEM TAMAMLANDI ===';
PRINT '✅ Tüm eksik kolonlar ve foreign key''ler eklendi.';
PRINT '🎯 Artık user@example.com ile giriş yapabilirsiniz!';
PRINT '';
PRINT '=== SONRAKI ADIM ===';
PRINT 'Şimdi ayrı bir script ile mevcut kayıtları güncelleyebilirsiniz.';
