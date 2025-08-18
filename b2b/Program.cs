using b2b.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// Web uygulaması başlat
var builder = WebApplication.CreateBuilder(args);


// MVC ekle
builder.Services.AddControllersWithViews();

//Database Servisi
builder.Services.AddDbContext<B2BContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Oturum kapanma servisi
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/LockScreen";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

        // Gerekli tabloların varlığını kontrol et
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<B2BContext>();
        db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductImages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductImages](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ProductId] INT NOT NULL,
        [ImageUrl] NVARCHAR(500) NOT NULL,
        [IsPrimary] BIT NOT NULL DEFAULT(0),
        [DisplayOrder] INT NOT NULL DEFAULT(0)
    );
    ALTER TABLE [dbo].[ProductImages] ADD CONSTRAINT [FK_ProductImages_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE;
END");
    }
    catch { }
}

        // HTTP istek pipeline'ını yapılandır
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
            // HSTS varsayılan değeri 30 gün
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Bu satırı ekledim
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
