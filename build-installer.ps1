param(
    [string]$PublishProfile = "AszAdmin-win-x64",
    [string]$CodeSignPfxPath = $env:ASZADMIN_SIGN_PFX_PATH,
    [string]$CodeSignPfxPassword = $env:ASZADMIN_SIGN_PFX_PASSWORD,
    [string]$CodeSignThumbprint = $env:ASZADMIN_SIGN_CERT_THUMBPRINT,
    [string]$TimeStampServer = $(if ($env:ASZADMIN_SIGN_TIMESTAMP_URL) { $env:ASZADMIN_SIGN_TIMESTAMP_URL } else { "http://timestamp.digicert.com" }),
    [switch]$RequireSigning
)

$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "WinFormsApp1\WinFormsApp1.csproj"
$publishDir = Join-Path $PSScriptRoot "WinFormsApp1\bin\Release\net8.0-windows\publish\win-x64"
$appExePath = Join-Path $publishDir "AszAdmin.exe"
$installerDir = Join-Path $PSScriptRoot "installer"
$installerScriptPath = Join-Path $PSScriptRoot "installer\AszAdmin1.iss"
$setupExePath = Join-Path $installerDir "AszAdmin-Setup.exe"
$setupPackageDir = Join-Path $installerDir "AszAdmin-Setup-Package"
$setupZipPath = Join-Path $installerDir "AszAdmin-Setup-Package.zip"

function Resolve-CodeSigningCertificate
{
    param(
        [string]$PfxPath,
        [string]$PfxPassword,
        [string]$Thumbprint
    )

    if (-not [string]::IsNullOrWhiteSpace($PfxPath))
    {
        $candidatePath = if ([System.IO.Path]::IsPathRooted($PfxPath))
        {
            $PfxPath
        }
        else
        {
            Join-Path $PSScriptRoot $PfxPath
        }

        $resolvedPath = Resolve-Path -LiteralPath $candidatePath -ErrorAction SilentlyContinue
        if ($null -eq $resolvedPath)
        {
            throw "A megadott PFX fajl nem talalhato: $candidatePath"
        }

        $storageFlags =
            [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::DefaultKeySet -bor
            [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable -bor
            [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet

        return [System.Security.Cryptography.X509Certificates.X509Certificate2]::new(
            $resolvedPath.Path,
            $PfxPassword,
            $storageFlags)
    }

    if (-not [string]::IsNullOrWhiteSpace($Thumbprint))
    {
        $normalizedThumbprint = ($Thumbprint -replace "\s", "").ToUpperInvariant()
        $certificates = @(
            Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue
            Get-ChildItem Cert:\LocalMachine\My -CodeSigningCert -ErrorAction SilentlyContinue
        ) | Where-Object { $_.Thumbprint -eq $normalizedThumbprint }

        if ($certificates.Count -eq 0)
        {
            throw "Nem talalhato code signing cert a megadott thumbprinttel: $normalizedThumbprint"
        }

        return $certificates[0]
    }

    return $null
}

function Test-IsRsaCertificate
{
    param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate
    )

    return $Certificate.GetKeyAlgorithm() -eq "1.2.840.113549.1.1.1"
}

function Sign-Executable
{
    param(
        [string]$FilePath,
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate,
        [string]$TimestampUrl
    )

    if (-not (Test-Path -LiteralPath $FilePath))
    {
        throw "A signolando fajl nem talalhato: $FilePath"
    }

    Write-Host "Alairas: $FilePath"

    $signature = if ([string]::IsNullOrWhiteSpace($TimestampUrl))
    {
        Set-AuthenticodeSignature -LiteralPath $FilePath -Certificate $Certificate -HashAlgorithm SHA256
    }
    else
    {
        Set-AuthenticodeSignature -LiteralPath $FilePath -Certificate $Certificate -HashAlgorithm SHA256 -TimestampServer $TimestampUrl
    }

    if ($signature.Status -ne [System.Management.Automation.SignatureStatus]::Valid)
    {
        $statusMessage = if ([string]::IsNullOrWhiteSpace($signature.StatusMessage))
        {
            "Ismeretlen alairasi hiba."
        }
        else
        {
            $signature.StatusMessage
        }

        throw "Az alairas sikertelen vagy nem megbizhato: $FilePath`nStatus: $($signature.Status)`n$statusMessage"
    }
}

function New-InstallerPackage
{
    param(
        [string]$SourceDirectory,
        [string]$PackageDirectory,
        [string]$ZipPath
    )

    $packageFiles = Get-ChildItem -LiteralPath $SourceDirectory -File |
        Where-Object { $_.BaseName -like "AszAdmin-Setup*" }

    if ($packageFiles.Count -eq 0)
    {
        throw "Nem talalhatok telepito fajlok a csomagolashoz."
    }

    if (Test-Path -LiteralPath $PackageDirectory)
    {
        Remove-Item -LiteralPath $PackageDirectory -Recurse -Force
    }

    if (Test-Path -LiteralPath $ZipPath)
    {
        Remove-Item -LiteralPath $ZipPath -Force
    }

    New-Item -ItemType Directory -Path $PackageDirectory | Out-Null

    foreach ($file in $packageFiles)
    {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $PackageDirectory $file.Name)
    }

    $readmePath = Join-Path $PackageDirectory "README.txt"
    @(
        "Az installaciohoz a setup EXE es az osszes mellette levo BIN fajl egyutt szukseges.",
        "Masik gepre a teljes mappat vagy a ZIP csomagot masold at, ne csak az EXE-t."
    ) | Set-Content -LiteralPath $readmePath -Encoding ASCII

    Compress-Archive -Path (Join-Path $PackageDirectory '*') -DestinationPath $ZipPath -Force
}

$signingCertificate = Resolve-CodeSigningCertificate -PfxPath $CodeSignPfxPath -PfxPassword $CodeSignPfxPassword -Thumbprint $CodeSignThumbprint
$signingEnabled = $null -ne $signingCertificate

if ($signingEnabled)
{
    if (-not $signingCertificate.HasPrivateKey)
    {
        throw "A code signing cert nem tartalmaz privat kulcsot."
    }

    if (-not (Test-IsRsaCertificate -Certificate $signingCertificate))
    {
        throw "A code signing cert nem RSA alapu. Smart App Controlhoz RSA tanusitvany kell."
    }

    Write-Host "Code signing cert: $($signingCertificate.Subject)"
}
elseif ($RequireSigning)
{
    throw "A RequireSigning be van kapcsolva, de nincs megadva PFX fajl vagy cert thumbprint."
}
else
{
    Write-Warning "Nincs megadva code signing cert. A build alairas nelkul keszul el, es Smart App Control varhatoan blokkolni fogja."
}

Write-Host "Publish indul a '$PublishProfile' profillal..."
dotnet publish $projectPath "/p:PublishProfile=$PublishProfile"

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

if ($signingEnabled)
{
    Sign-Executable -FilePath $appExePath -Certificate $signingCertificate -TimestampUrl $TimeStampServer
}

$isccCandidates = @(@(
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
) | Where-Object { $_ -and (Test-Path $_) })

if ($isccCandidates.Count -eq 0)
{
    throw "Az Inno Setup 6 forditoja nem talalhato. Telepitsd az Inno Setupot, majd futtasd ujra ezt a scriptet."
}

$isccPath = $isccCandidates[0]

Write-Host "Telepito keszitese: $isccPath"
& $isccPath $installerScriptPath

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

if ($signingEnabled)
{
    Sign-Executable -FilePath $setupExePath -Certificate $signingCertificate -TimestampUrl $TimeStampServer
}

New-InstallerPackage -SourceDirectory $installerDir -PackageDirectory $setupPackageDir -ZipPath $setupZipPath

Write-Host "Kesz. A setup az installer mappaban jon letre."
Write-Host "Tovabbitashoz hasznald ezt a ZIP-et: $setupZipPath"
