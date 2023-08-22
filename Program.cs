using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.Limits.MaxRequestBodySize = null; });
builder.Services.AddRazorPages();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

////app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)),
    RequestPath = new PathString("")
});
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
var host = Dns.GetHostEntry(Dns.GetHostName());
foreach (var item in host.AddressList)
{
    if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    {
        app.Urls.Add($"http://{item}:5000");
    }
}
app.Run();
