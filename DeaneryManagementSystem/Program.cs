using DeaneryManagementSystem.Abstractions;
using DeaneryManagementSystem.Authentication;
using DeaneryManagementSystem.Data;
using DeaneryManagementSystem.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthenticationCore();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<DeaneryAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<DeaneryAuthenticationStateProvider>());

builder.Services.AddScoped<IGroupService, GroupService>()
                .AddScoped<ISubjectService, SubjectService>()
                .AddScoped<IAccountService, AccountService>()
                .AddScoped<IPasswordHasher, PasswordHasher>()
                .AddScoped<IPersonService, PersonService>()
                .AddScoped<IMarkService, MarkService>();

var connectionstring = builder.Configuration.GetConnectionString("DeanerySystem");
builder.Services.AddDbContext<DeaneryContext>(
    options => options.UseNpgsql(connectionstring),
    ServiceLifetime.Transient);

builder.Services.AddRadzenComponents();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<TooltipService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
