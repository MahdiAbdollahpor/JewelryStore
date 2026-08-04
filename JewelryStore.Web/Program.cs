using JewelryStore.Data.Context;
using JewelryStore.Infrastructure.Services;
using JewelryStore.Infrastructure.Services.Payment;
using JewelryStore.Services.Interfaces;
using JewelryStore.Services.Services;
using JewelryStore.Web.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1️⃣ ثبت DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ ثبت AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 3️⃣ ثبت سرویس پیامک
builder.Services.AddScoped<ISmsSender, SmsSender>();

// 4️⃣ ثبت سرویس پرداخت
builder.Services.AddHttpClient<ZarinPalService>((client) =>
{
    var isSandbox = builder.Configuration.GetValue<bool>("ZarinPal:IsSandbox", true);
    var baseUrl = isSandbox
        ? "https://sandbox.zarinpal.com/"
        : "https://api.zarinpal.com/";

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IPaymentService>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var client = httpClientFactory.CreateClient(nameof(ZarinPalService));

    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<ZarinPalService>>();

    var merchantId = configuration["ZarinPal:MerchantId"] ?? "35ab132c-e623-49d3-896a-3442b1c6561c";
    var isSandbox = configuration.GetValue<bool>("ZarinPal:IsSandbox", true);

    return new ZarinPalService(client, logger, merchantId, isSandbox);
});

// 5️⃣ ثبت سرویس‌های اصلی برنامه
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>(); // ✅ این خط را اضافه کنید
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "JewelryStoreAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ✅ ثبت Middleware بررسی نقش
app.UseMiddleware<RoleMiddleware>();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}"
);

app.Run();
