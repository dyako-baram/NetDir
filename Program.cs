using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.Limits.MaxRequestBodySize = null; });
builder.Services.AddRazorPages();
var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!),
    RequestPath = new PathString("")
});

app.UseRouting();

app.MapRazorPages();

foreach (var item in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
{
    if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) 
        app.Urls.Add($"http://{item}:5000");
}
app.Run();
