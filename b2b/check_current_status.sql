-- =============================================
-- B2B Portal Mevcut Durum Kontrol Sorgusu
-- =============================================

-- 1. Companies tablosu - Rol durumları
PRINT '=== COMPANIES TABLOSU - ROL DURUMLARI ==='
SELECT 
    Id,
    Name,
    IsSeller,
    IsBuyer,
    CASE 
        WHEN IsSeller = 1 AND IsBuyer = 1 THEN 'Hem Satıcı Hem Alıcı'
        WHEN IsSeller = 1 AND IsBuyer = 0 THEN 'Sadece Satıcı'
        WHEN IsSeller = 0 AND IsBuyer = 1 THEN 'Sadece Alıcı'
        ELSE 'Tanımsız'
    END AS RolDurumu
FROM Companies
ORDER BY Id;

PRINT ''

-- 2. Users tablosu - Kullanıcı bilgileri
PRINT '=== USERS TABLOSU - KULLANICI BİLGİLERİ ==='
SELECT 
    u.Id,
    u.Email,
    u.FullName,
    u.CompanyId,
    c.Name AS CompanyName,
    c.IsSeller,
    c.IsBuyer
FROM Users u
INNER JOIN Companies c ON u.CompanyId = c.Id
ORDER BY u.Id;

PRINT ''

-- 3. UserRoles tablosu - Kullanıcı rolleri
PRINT '=== USERROLES TABLOSU - KULLANICI ROLLERİ ==='
SELECT 
    ur.UserId,
    u.Email,
    ur.RoleId,
    r.RoleName,
    c.Name AS CompanyName
FROM UserRoles ur
INNER JOIN Users u ON ur.UserId = u.Id
INNER JOIN Roles r ON ur.RoleId = r.Id
INNER JOIN Companies c ON u.CompanyId = c.Id
ORDER BY ur.UserId;

PRINT ''

-- 4. CustomerOrders tablosu - Sipariş durumları
PRINT '=== CUSTOMERORDERS TABLOSU - SİPARİŞ DURUMLARI ==='
SELECT 
    Id,
    OrderNumber,
    BuyerCompanyId,
    SellerCompanyId,
    Status,
    TotalAmount,
    OrderDate,
    (SELECT Name FROM Companies WHERE Id = BuyerCompanyId) AS BuyerCompany,
    (SELECT Name FROM Companies WHERE Id = SellerCompanyId) AS SellerCompany
FROM CustomerOrders
ORDER BY Id;

PRINT ''

-- 5. Products tablosu - Ürün bilgileri
PRINT '=== PRODUCTS TABLOSU - ÜRÜN BİLGİLERİ ==='
SELECT 
    Id,
    Name,
    SellerCompanyId,
    (SELECT Name FROM Companies WHERE Id = SellerCompanyId) AS SellerCompany,
    Stock,
    MinStock,
    Category,
    Brand
FROM Products
ORDER BY Id;

PRINT ''

-- 6. Customers tablosu - Müşteri bilgileri
PRINT '=== CUSTOMERS TABLOSU - MÜŞTERİ BİLGİLERİ ==='
SELECT 
    Id,
    Name,
    CompanyId,
    (SELECT Name FROM Companies WHERE Id = CompanyId) AS CompanyName,
    Balance,
    Status
FROM Customers
ORDER BY Id;

PRINT ''

-- 7. Özet bilgi
PRINT '=== ÖZET BİLGİ ==='
SELECT 
    'Toplam Şirket' AS Bilgi,
    COUNT(*) AS Sayı
FROM Companies
UNION ALL
SELECT 
    'Toplam Kullanıcı',
    COUNT(*)
FROM Users
UNION ALL
SELECT 
    'Toplam Sipariş',
    COUNT(*)
FROM CustomerOrders
UNION ALL
SELECT 
    'Toplam Ürün',
    COUNT(*)
FROM Products
UNION ALL
SELECT 
    'Toplam Müşteri',
    COUNT(*)
FROM Customers;

PRINT ''
PRINT '=== SORUN TESPİTİ ==='
PRINT 'Eğer seller@example.com ve seller2@example.com satıcı olmalıysa:'
PRINT 'Companies tablosunda IsSeller = 1 olmalı'
PRINT 'Şu anda hepsi IsSeller = 0 olduğu için hepsi alıcı dashboard görüyor'
PRINT ''
PRINT 'ÇÖZÜM:'
PRINT 'UPDATE Companies SET IsSeller = 1 WHERE Name IN (''seller'', ''seller2'');'
