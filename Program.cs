using System.Diagnostics;
using System.Security.Cryptography;

const string DEMO_MD5 = "63b64090b620a76a177e131686955fe2";
const int DEMO_OFFSET = 0xdb5e8;
const string DEMO_SETUP = "EQ_PPC_E1_Demo_Setup.exe";
const string DEMO_FILENAME = "PocketEQDemoPPC2002.STRONGARM.cab";

static string ComputeMD5(FileStream stream)
{
    using var md5 = MD5.Create();
    var hash = md5.ComputeHash(stream);
    return BitConverter.ToString(hash).Replace("-", "").ToLower();
}

var setupFilepath = DEMO_SETUP;
if (args.Length > 0)
{
    setupFilepath = args[0];
}

if (!File.Exists(setupFilepath))
{
    var filePath = Process.GetCurrentProcess().MainModule?.FileName ?? "EQPPCDemoExtractor.exe";
    var program = Path.GetFileName(filePath);
    Console.WriteLine($"[ERROR] Setup file not found: {setupFilepath}");
    Console.WriteLine($"Usage: {program} [SETUP_FILE] [OUTPUT_DIRECTORY]");
    Console.WriteLine($"Example: {program} {DEMO_SETUP} Downloads");
    return;
}

var outputDir = Path.GetDirectoryName(setupFilepath);
if (args.Length > 1)
{
    outputDir = args[1];
    Directory.CreateDirectory(outputDir);
}

var fs = File.OpenRead(setupFilepath);
var md5sum = ComputeMD5(fs);
if (md5sum != DEMO_MD5)
{
    Console.WriteLine($"[ERROR] MD5 checksum failed, got {md5sum}, expected {DEMO_MD5}.");
    return;
}

var outputFilepath = Path.Join(outputDir, DEMO_FILENAME);
Console.WriteLine($"Extrating to {outputFilepath}");

fs.Position = DEMO_OFFSET;
var demoCab = Epsz.Entry.FromStream(fs);
using (FileStream ofs = File.Create(outputFilepath))
{
    ofs.Write(demoCab.Body);
}
File.SetCreationTime(outputFilepath, demoCab.TimeStamp.DateTime);
File.SetLastWriteTime(outputFilepath, demoCab.TimeStamp.DateTime);

Console.WriteLine("Done.");
