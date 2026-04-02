using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;
var builder = WebApplication.CreateBuilder(args);

// Add database context and cache
if(builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
    builder.Services.AddDistributedMemoryCache();
}
else
{
    var sqlConnectionString =
        builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")
        ?? builder.Configuration["AZURE_SQL_CONNECTIONSTRING"];
    var redisConnectionString =
        builder.Configuration.GetConnectionString("AZURE_REDIS_CONNECTIONSTRING")
        ?? builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];

    if (string.IsNullOrWhiteSpace(sqlConnectionString))
    {
        throw new InvalidOperationException("Missing required connection string: AZURE_SQL_CONNECTIONSTRING");
    }

    if (string.IsNullOrWhiteSpace(redisConnectionString))
    {
        throw new InvalidOperationException("Missing required connection string: AZURE_REDIS_CONNECTIONSTRING");
    }

    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(sqlConnectionString));
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "SampleInstance";
    });
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add App Service logging
builder.Logging.AddAzureWebAppDiagnostics();

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
    pattern: "{controller=Todos}/{action=Index}/{id?}");

app.Run();
