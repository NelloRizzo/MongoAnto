using MongoUtils;
using System.Runtime.CompilerServices;

async Task Upload(MongoConnection c, IEnumerable<string> files) {
    foreach (var item in files) {
        await c.Upload(item);
    }
}
async Task Download(MongoConnection c, string path, IEnumerable<string> files) {
    foreach (var item in files) {
        await c.Download(path, item);
    }
}

foreach (var item in args) {
    Console.WriteLine(item);
}

if (args.Length == 0) Environment.Exit(-1);

using var c = new MongoConnection();

if (args.Length == 0 || args[0] == "-h") {
    var fileName = System.AppDomain.CurrentDomain.FriendlyName;
    Console.WriteLine("Mongo File Uploader and Downloader");
    Console.WriteLine("**********************************");
    Console.WriteLine("Versione 1.0\n");
    Console.WriteLine("Utilizzo: ");
    Console.WriteLine($"{fileName} -h");
    Console.WriteLine("\tmostra questo help");
    Console.WriteLine($"{fileName} -u file1 file2 ... fileN");
    Console.WriteLine("\teffettua l'upload di tutti i files elencati");
    Console.WriteLine($"{fileName} [-p path] file1 file2 ... fileN");
    Console.WriteLine("\teffettua il download di tutti i files elencati");
    Console.WriteLine("\tl'opzione -p è seguita dalla cartella nella quale scaricare i file");
    Console.WriteLine("\tse non specificata i file vengono scaricati nella cartella corrente");
    Environment.Exit(0);
}
if (args[0] == "-u")
    await Upload(c, args.Skip(1));
else {
    if (args[0] == "-p") {
        var path = args[1];
        await Download(c, path, args.Skip(2));
    }
    else
        await Download(c, Path.GetFullPath("./"), args);
}

