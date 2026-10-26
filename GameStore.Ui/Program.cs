using Microsoft.AspNetCore.Builder;
using System;

var builder = WebApplication.CreateBuilder(args);

// Basic configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Configure HttpClient for API calls
builder.Services.AddHttpClient("GameApi", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5197/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

app.Run();
