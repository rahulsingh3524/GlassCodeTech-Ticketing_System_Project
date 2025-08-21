using GlassCodeTech_Ticketing_System_Project.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register IHttpContextAccessor for CookieService
builder.Services.AddHttpContextAccessor();

// Register DatabaseHelper and CookieService
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<NotificationService>();


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
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
