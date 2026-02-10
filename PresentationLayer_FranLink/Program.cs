using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSession(); // Add Session
builder.Services.AddHttpContextAccessor(); // Add HttpContextAccessor
builder.Services.AddScoped<BusinessLogicLayer_FranLink.Services.IInternalOrderService, BusinessLogicLayer_FranLink.Services.InternalOrderService>();
builder.Services.AddScoped<BusinessLogicLayer_FranLink.Services.IInventoryService, BusinessLogicLayer_FranLink.Services.InventoryService>();


builder.Services.AddDbContext<FranLinkContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Ensure static files are served

app.UseRouting();

app.UseSession(); // Enable Session middleware before Auth and Mapping

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.Run();
