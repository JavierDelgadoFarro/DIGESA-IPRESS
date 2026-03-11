using System.Diagnostics;
using System.Text.Json;

namespace IPRESS.API.Services;

/// <summary>
/// Ejecuta el proceso auxiliar IPRESS.ExcelHelper para leer el Excel en un proceso separado.
/// Si el helper falla o se cierra, la API sigue en ejecución.
/// </summary>
public static class ExcelHelperRunner
{
    public class HelperOutput
    {
        public bool Ok { get; set; }
        public List<RowOutput> Rows { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class RowOutput
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Departamento { get; set; } = "";
        public string Provincia { get; set; } = "";
        public string Distrito { get; set; } = "";
        public string Ubigeo { get; set; } = "";
    }

    /// <summary>Escribe los bytes en un archivo temporal, ejecuta el helper y devuelve el resultado parseado. Devuelve null si el proceso falla.</summary>
    public static async Task<HelperOutput?> RunAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        string? tempPath = null;
        try
        {
            tempPath = Path.Combine(Path.GetTempPath(), "IPRESS_Excel_" + Guid.NewGuid().ToString("N") + ".xlsx");
            await File.WriteAllBytesAsync(tempPath, fileBytes, cancellationToken);

            // Buscar ExcelHelper en varias rutas (F5/VS puede usar otro directorio que dotnet run)
            var candidateDirs = new List<string>
            {
                AppContext.BaseDirectory,
                Path.GetDirectoryName(Environment.ProcessPath) ?? "",
                Directory.GetCurrentDirectory()
            }.Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();

            string? helperDir = null;
            string? helperDll = null;
            string? helperExe = null;
            foreach (var baseDir in candidateDirs)
            {
                var dir = Path.Combine(baseDir, "ExcelHelper");
                var dll = Path.Combine(dir, "IPRESS.ExcelHelper.dll");
                var exe = Path.Combine(dir, "IPRESS.ExcelHelper.exe");
                if (File.Exists(exe) || File.Exists(dll))
                {
                    helperDir = dir;
                    helperDll = dll;
                    helperExe = exe;
                    break;
                }
            }

            if (helperDir == null || (!File.Exists(helperExe!) && !File.Exists(helperDll!)))
                return null;

            string fileName;
            string arguments;
            if (File.Exists(helperExe))
            {
                fileName = helperExe;
                arguments = "\"" + tempPath + "\"";
            }
            else
            {
                fileName = "dotnet";
                arguments = "\"" + helperDll + "\" \"" + tempPath + "\"";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = helperDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return null;

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            var exitTask = process.WaitForExitAsync(cancellationToken);

            await exitTask;
            var stdout = await stdoutTask;
            await stderrTask;

            if (process.ExitCode != 0)
                return null;

            var output = JsonSerializer.Deserialize<HelperOutput>(stdout);
            return output;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (tempPath != null && File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { /* ignorar */ }
            }
        }
    }
}
