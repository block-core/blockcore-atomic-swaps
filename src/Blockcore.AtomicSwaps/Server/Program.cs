using Blockcore.AtomicSwaps.Server;
using Blockcore.AtomicSwaps.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();
builder.Services.Configure<TelegramLoggingBotOptions>(options =>builder.Configuration.GetSection("TelegramLoggingBot").Bind(options));
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();

builder.Services.Configure<DataConfigOptions>(options => builder.Configuration.GetSection("DataConfig").Bind(options));
builder.Services.AddSingleton<IStorageService, StorageService>();

var app = builder.Build();

app.CreateDataLocation();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blazor API V1");
});

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
