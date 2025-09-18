var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register database context
builder.Services.AddScoped<BTL.Web.Data.DatabaseContext>();

// Register repositories
builder.Services.AddScoped<BTL.Web.Repositories.ILoaiBanRepository, BTL.Web.Repositories.LoaiBanRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.IBanAnRepository, BTL.Web.Repositories.BanAnRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.IMonRepository, BTL.Web.Repositories.MonRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.INguyenLieuRepository, BTL.Web.Repositories.NguyenLieuRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.INhaCungCapRepository, BTL.Web.Repositories.NhaCungCapRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.ILayoutRepository, BTL.Web.Repositories.LayoutRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.ILoaiNhanVienRepository, BTL.Web.Repositories.LoaiNhanVienRepository>();
builder.Services.AddScoped<BTL.Web.Repositories.INhanVienRepository, BTL.Web.Repositories.NhanVienRepository>();

// Register services
builder.Services.AddScoped<BTL.Web.Services.ILoaiBanService, BTL.Web.Services.LoaiBanService>();
builder.Services.AddScoped<BTL.Web.Services.IMonService, BTL.Web.Services.MonService>();
builder.Services.AddScoped<BTL.Web.Services.INguyenLieuService, BTL.Web.Services.NguyenLieuService>();
builder.Services.AddScoped<BTL.Web.Services.INhaCungCapService, BTL.Web.Services.NhaCungCapService>();
builder.Services.AddScoped<BTL.Web.Services.IBanAnService, BTL.Web.Services.BanAnService>();
builder.Services.AddScoped<BTL.Web.Services.ILayoutService, BTL.Web.Services.LayoutService>();
builder.Services.AddScoped<BTL.Web.Services.ILoaiNhanVienService, BTL.Web.Services.LoaiNhanVienService>();
builder.Services.AddScoped<BTL.Web.Services.INhanVienService, BTL.Web.Services.NhanVienService>();

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
