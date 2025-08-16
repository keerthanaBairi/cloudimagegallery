using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// HttpClient with API base address from config
builder.Services.AddHttpClient("Api", client =>
{
    var apiUrl = builder.Configuration["ApiUrl"];
    if (string.IsNullOrWhiteSpace(apiUrl))
        throw new InvalidOperationException("ApiUrl not configured");
    client.BaseAddress = new Uri(apiUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

app.Run();
