using IPRESS.ExcelParser;

// Ruta del archivo Excel como primer argumento. Escribe JSON a stdout y sale con 0; si falla, escribe a stderr y sale con 1.
if (args.Length == 0)
{
    Console.Error.WriteLine("Uso: IPRESS.ExcelHelper <ruta_archivo.xlsx>");
    return 1;
}

try
{
    var path = args[0].Trim();
    if (!File.Exists(path))
    {
        Console.Error.WriteLine("Archivo no encontrado: " + path);
        return 1;
    }

    var bytes = await File.ReadAllBytesAsync(path);
    var result = DiresaExcelReader.GetRawRows(bytes);
    var json = DiresaExcelReader.ToJson(result);
    Console.WriteLine(json);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error: " + ex.Message);
    return 1;
}
