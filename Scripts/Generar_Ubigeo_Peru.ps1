# Genera Insert_Ubigeo_Peru_Completo.sql desde el archivo de ubigeo INEI (formato MySQL)
$ubigeoFile = Join-Path $PSScriptRoot "..\agent-tools\c4e4e039-6475-42e9-9874-1a3466a3864d.txt"
if (-not (Test-Path $ubigeoFile)) {
    Write-Error "Descargue primero el archivo de ubigeo desde https://raw.githubusercontent.com/ernestorivero/Ubigeo-Peru/master/sql/ubigeo_peru_inei_2016.sql y coloquelo como ubigeo_inei_2016.txt en Scripts"
    exit 1
}
$lines = Get-Content $ubigeoFile -Encoding UTF8
$outPath = Join-Path $PSScriptRoot "Insert_Ubigeo_Peru_Completo.sql"
function Esc-Sql { param($s) ($s -replace "'", "''").Trim() }
$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine("-- =============================================")
[void]$sb.AppendLine("-- IPRESS - UBIGEO PERÚ COMPLETO (INEI 2016)")
[void]$sb.AppendLine("-- Departamentos, Provincias y Distritos")
[void]$sb.AppendLine("-- Ejecutar sobre la BD IPRESS (tras crear las tablas IPRESS_Departamento, IPRESS_Provincia, IPRESS_Distrito)")
[void]$sb.AppendLine("-- =============================================")
[void]$sb.AppendLine("SET NOCOUNT ON;")
[void]$sb.AppendLine("")

# Departamentos
[void]$sb.AppendLine("-- DEPARTAMENTOS (25)")
[void]$sb.AppendLine("DELETE FROM IPRESS_Distrito;")
[void]$sb.AppendLine("DELETE FROM IPRESS_Provincia;")
[void]$sb.AppendLine("DELETE FROM IPRESS_Departamento;")
[void]$sb.AppendLine("")
$inDept = $false
foreach ($line in $lines) {
    if ($line -match "INSERT INTO.*ubigeo_peru_departments") { $inDept = $true; continue }
    if ($inDept -and $line -match "\('(\d{2})',\s*'([^']*)'\)") {
        $cod = $Matches[1]; $nom = Esc-Sql $Matches[2]
        [void]$sb.AppendLine("INSERT INTO IPRESS_Departamento (Codigo, Nombre) VALUES ('$cod', '$nom');")
    }
    if ($inDept -and $line -match "^\s*\);") { $inDept = $false; break }
}
[void]$sb.AppendLine("")

# Provincias
[void]$sb.AppendLine("-- PROVINCIAS (196)")
$inProv = $false
foreach ($line in $lines) {
    if ($line -match "INSERT INTO.*ubigeo_peru_provinces") { $inProv = $true; continue }
    if ($inProv -and $line -match "\('(\d{4})',\s*'([^']*)',\s*'(\d{2})'\)") {
        $cod = $Matches[1]; $nom = Esc-Sql $Matches[2]; $codDep = $Matches[3]
        [void]$sb.AppendLine("INSERT INTO IPRESS_Provincia (Codigo, Nombre, CodigoDepartamento) VALUES ('$cod', '$nom', '$codDep');")
    }
    if ($inProv -and $line -match "^\s*\);") { $inProv = $false; break }
}
[void]$sb.AppendLine("")

# Distritos
[void]$sb.AppendLine("-- DISTRITOS (~1877)")
$inDist = $false
$distCount = 0
foreach ($line in $lines) {
    if ($line -match "INSERT INTO.*ubigeo_peru_districts") { $inDist = $true; continue }
    if ($inDist -and $line -match "\('(\d{6})',\s*'([^']*)',\s*'(\d{4})',\s*'\d{2}'\)") {
        $ubigeo = $Matches[1]; $nom = Esc-Sql $Matches[2]; $codProv = $Matches[3]
        [void]$sb.AppendLine("INSERT INTO IPRESS_Distrito (Ubigeo, Nombre, CodigoProvincia) VALUES ('$ubigeo', '$nom', '$codProv');")
        $distCount++
    }
    if ($inDist -and $line -match "^\s*\);") { $inDist = $false; break }
}
[void]$sb.AppendLine("")
[void]$sb.AppendLine("PRINT 'Ubigeo Perú: 25 departamentos, 196 provincias, $distCount distritos.';")
$sb.ToString() | Set-Content $outPath -Encoding UTF8
Write-Host "Generado: $outPath"
