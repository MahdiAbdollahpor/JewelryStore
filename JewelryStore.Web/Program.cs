using JewelryStore.Data.Context;
using JewelryStore.Infrastructure.Services;
using JewelryStore.Infrastructure.Services.Payment;
using JewelryStore.Services.Interfaces;
using JewelryStore.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddHttpClient<ZarinPalService>((client) =>
{
    // ??????? ???? HttpClient ???? ZarinPalService
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


builder.Services.AddAutoMapper(typeof(Program));



builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ISmsSender, SmsSender>();
builder.Services.AddScoped<IReportService, ReportService>();


// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
