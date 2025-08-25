var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register database context
builder.Services.AddScoped<BTL.Web.Data.DatabaseContext>();

// Register repositories
builder.Services.AddScoped<BTL.Web.Repositories.ILoaiBanRepository, BTL.Web.Repositories.LoaiBanRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.IBanAnRepository, BTL.Web.Repositories.BanAnRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.IMonRepository, BTL.Web.Repositories.MonRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.ILayoutRepository, BTL.Web.Repositories.LayoutRepository>();

// Register services
builder.Services.AddScoped<BTL.Web.Services.ILoaiBanService, BTL.Web.Services.LoaiBanService>();
builder.Services.AddScoped<BTL.Web.Services.IMonService, BTL.Web.Services.MonService>();
builder.Services.AddScoped<BTL.Web.Services.IBanAnService, BTL.Web.Services.BanAnService>();
builder.Services.AddScoped<BTL.Web.Services.ILayoutService, BTL.Web.Services.LayoutService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
