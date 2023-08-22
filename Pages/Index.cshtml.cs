using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace NetDir.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly string _basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public Dictionary<string, List<string>> List;
        [BindProperty(SupportsGet = true)]
        public string? PathQuery { get; set; } = null;
        public List<string> PathList = Paths.PathsDir;
        public string PathBaseRelative { get; set; }
        public string? BreadCrumb { get; set; }
        public void OnGet()
        {
            LogClient();
            if (PathQuery != null)
            {
                if (PathList.Contains(PathQuery))
                {
                    var li = PathList.RemoveAll(x => x.StartsWith(PathQuery));
                    PathList.Add(PathQuery);
                    BreadCrumb = PathQuery;
                }
                else
                {
                    PathList.Add(PathQuery);
                    BreadCrumb = PathQuery;
                }
            }
            else
            {
                PathList.Clear();
                BreadCrumb = null;
                PathList.Add("/");
                ViewData["Title"] = "Root Folder";
            }

            ListDirectory(PathQuery);
        }
        public async Task<IActionResult> OnPostAsync(List<IFormFile> files,string path)
        {
            string filePath;
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    if (path==null)
                        filePath = Path.Combine(_basePath, file.FileName);
                    else
                        filePath = Path.Combine(_basePath, path, file.FileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    LogFileCreation(file,filePath);
                }
            }
            if (path == null)
                return RedirectToPage("Index");
            else
                return RedirectToPage("Index", new { PathQuery = path });
        }
        public IActionResult OnPostCreareFolder(string text, string? path)
        {
            string filePath;
            if (path == null)
                filePath = Path.Combine(_basePath, text);
            else
                filePath = Path.Combine(_basePath, path, text);

            LogFolderCreation(text, filePath);
            Directory.CreateDirectory(filePath);
            if (path == null)
                return RedirectToPage("Index");
            else
                return RedirectToPage("Index", new { PathQuery = path });
        }
        public void ListDirectory(string? pathArg)
        {
            List = new Dictionary<string, List<string>>
            {
                ["Folder"] = new List<string>(),
                ["FileText"] = new List<string>(),
                ["FileImg"] = new List<string>(),
                ["FileVideo"] = new List<string>()
            };
            string path = pathArg ?? _basePath;
            if (pathArg != null)
            {
                path = Path.Combine(_basePath, PathQuery);
                PathBaseRelative = Path.GetRelativePath(_basePath, path);
            }
            var directory = new DirectoryInfo(path);
            foreach (var dir in directory.GetDirectories())
            {
                List["Folder"].Add(dir.Name);
            }
            foreach (var file in directory.GetFiles())
            {
                if (IsForImg(file.Name))
                {
                    List["FileImg"].Add(file.Name);
                }
                else if (IsForVideo(file.Name))
                {
                    List["FileVideo"].Add(file.Name);
                }
                else
                {
                    List["FileText"].Add(file.Name);
                }

            }
        }
        private bool IsForImg(string input)
        {
            foreach (string extension in "gif jpg jpeg png svg apng avif webp".Split(' '))
            {
                if (input.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsForVideo(string input)
        {
            foreach (string extension in "mp4 obb webm".Split(' '))
            {
                if (input.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public string CleanPathName(string input)
        {
            string[] parts = input.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
        }
        private  void LogClient()
        {
            var client = HttpContext.Connection;
            var request=HttpContext.Request;
            var clientInfo = new StringBuilder()
                .AppendLine("Client Info")
                .AppendLine("*****************")
                .AppendLine($"Time: {DateTime.Now}")
                .AppendLine($"IP: {client.RemoteIpAddress}:{client.RemotePort}")
               .AppendLine($"Method: {request.Method}")
               .AppendLine($"Referrer: {request.Headers["Referer"]}")
               .AppendLine($"Protocol: {request.Protocol}")
               .AppendLine($"QueryString: {request.QueryString}")
               .AppendLine($"Folder Path: {Uri.UnescapeDataString(request.QueryString.ToString().Replace("?PathQuery=","\\"))}")
               .AppendLine($"User Agent: {request.Headers["User-Agent"]}")
               .AppendLine("*****************");
            _logger.LogInformation(clientInfo.ToString());
        }
        private void LogFileCreation(IFormFile file,string path)
        {
            var client = HttpContext.Connection;
            var fileInfo = new StringBuilder()
                .AppendLine("File Creation Info")
                .AppendLine("*****************")
                .AppendLine($"Time: {DateTime.Now}")
                .AppendLine($"From Ip: {client.RemoteIpAddress}:{client.RemotePort}")
                .AppendLine($"Name: {file.FileName}")
                .AppendLine($"Content Type: {file.ContentType}")
                .AppendLine($"Method: {HttpContext.Request.Method}")
                .AppendLine($"Length: {file.Length}")
                .AppendLine($"ContentDisposition: {file.ContentDisposition}")
                .AppendLine($"Folder Path: {path}")
                .AppendLine("*****************");
            _logger.LogInformation(fileInfo.ToString());
        }
        private void LogFolderCreation(string folderName,string path)
        {
            var client = HttpContext.Connection;
            var folderInfo = new StringBuilder()
                .AppendLine("Folder Creation Info")
                .AppendLine("*****************")
                .AppendLine($"Time: {DateTime.Now}")
                .AppendLine($"From Ip: {client.RemoteIpAddress}:{client.RemotePort}")
                .AppendLine($"Name: {folderName}")
                .AppendLine($"Method: {HttpContext.Request.Method}")
                .AppendLine($"Folder Path: {path}")
                .AppendLine("*****************");
            _logger.LogInformation(folderInfo.ToString());
        }
    }
}