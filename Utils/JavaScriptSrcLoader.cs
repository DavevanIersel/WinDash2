using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace WinDash2.Utils;

public static class JavaScriptSrcLoader
{
    public static async Task<string?> GetSrcFileContentsByResourceNameAsync(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            Debug.WriteLine($"Script source file not found: {resourceName}");
            return null;
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}