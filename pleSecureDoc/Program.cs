// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pleSecureDoc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<FirebaseService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<FaceRecognitionService>();

var app = builder.Build();

// ❌ Temporarily commented out to bypass network error
// The face group will not be created automatically.
/*
using (var scope = app.Services.CreateScope())
{
    var faceService = scope.ServiceProvider.GetRequiredService<FaceRecognitionService>();
    try
    {
        await faceService.CreatePersonGroupAsync();
        Console.WriteLine("✅ Person group created or already exists.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to create person group: {ex.Message}");
    }
}
*/

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
