-- =====================================================
-- MEVCUT KAYITLARI GÜNCELLEME SCRIPTİ
-- =====================================================

PRINT '=== MEVCUT KAYITLARI GÜNCELLEME ===';

-- 1. Carts tablosundaki mevcut kayıtlar için BuyerCompanyId'yi Customer'ın CompanyId'si olarak ayarla
PRINT 'Carts tablosundaki kayıtlar güncelleniyor...';

UPDATE c 
SET c.BuyerCompanyId = cu.CompanyId
FROM Carts c
INNER JOIN Customers cu ON c.CustomerId = cu.Id
WHERE c.BuyerCompanyId IS NULL;

DECLARE @CartsUpdated INT = @@ROWCOUNT;
PRINT '✅ Carts tablosunda ' + CAST(@CartsUpdated AS VARCHAR(10)) + ' kayıt güncellendi.';

-- 2. ProductRequests tablosundaki mevcut kayıtlar için BuyerCompanyId'yi Customer'ın CompanyId'si olarak ayarla
PRINT 'ProductRequests tablosundaki kayıtlar güncelleniyor...';

UPDATE pr 
SET pr.BuyerCompanyId = cu.CompanyId
FROM ProductRequests pr
INNER JOIN Customers cu ON pr.CustomerId = cu.Id
WHERE pr.BuyerCompanyId IS NULL;

DECLARE @ProductRequestsUpdated INT = @@ROWCOUNT;
PRINT '✅ ProductRequests tablosunda ' + CAST(@ProductRequestsUpdated AS VARCHAR(10)) + ' kayıt güncellendi.';

PRINT '';
PRINT '=== GÜNCELLENEN KAYIT SAYILARI ===';
PRINT '📊 Carts tablosunda güncellenen kayıt sayısı: ' + CAST(@CartsUpdated AS VARCHAR(10));
PRINT '📊 ProductRequests tablosunda güncellenen kayıt sayısı: ' + CAST(@ProductRequestsUpdated AS VARCHAR(10));

PRINT '';
PRINT '=== SON DURUM KONTROLÜ ===';

-- 3. Carts tablosu son durum
PRINT '🔍 Carts tablosu son durum:';
SELECT COUNT(*) AS TotalCarts, 
       COUNT(BuyerCompanyId) AS CartsWithBuyerCompanyId,
       COUNT(*) - COUNT(BuyerCompanyId) AS CartsWithoutBuyerCompanyId
FROM Carts;

-- 4. ProductRequests tablosu son durum
PRINT '🔍 ProductRequests tablosu son durum:';
SELECT COUNT(*) AS TotalProductRequests, 
       COUNT(BuyerCompanyId) AS RequestsWithBuyerCompanyId,
       COUNT(*) - COUNT(BuyerCompanyId) AS RequestsWithoutBuyerCompanyId
FROM ProductRequests;

PRINT '';
PRINT '=== İŞLEM TAMAMLANDI ===';
PRINT '✅ Tüm mevcut kayıtlar güncellendi.';
PRINT '🎯 Artık projeyi test edebilirsiniz!';
PRINT '🚀 user@example.com ile giriş yapmayı deneyin.';
