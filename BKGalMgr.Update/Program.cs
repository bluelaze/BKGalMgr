using System.Diagnostics;
using System.IO.Compression;
using DotMake.CommandLine;

namespace BKGalMgr.Update;

[CliCommand(Description = "BKGalMgr update program")]
class Options
{
    [CliOption(Required = true, Description = "要解压的zip文件(Zip file that need to decompress)")]
    public string ZipFileName { get; set; }

    [CliOption(Required = true, Description = "解压到的文件夹(Where decompress to)")]
    public string DecompressFolder { get; set; }

    [CliOption(Required = false, Description = "需要复制的文件(Files that need to copy)")]
    public List<string> CopyFiles { get; set; }

    [CliOption(Required = false, Description = "重启程序(Exe name that restart)")]
    public string RestartExeName { get; set; }
}

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("等待主程序关闭...(Waiting for main program stop)");
        Task.Delay(3000).Wait();

        var result = Cli.Parse<Options>(args);
        if (result == null)
        {
            Console.WriteLine("参数解析失败...(Args parse fail)\n");
            Cli.Run<Options>("-?");
            Console.ReadKey();
            return;
        }
        else if (result.Errors.Any())
        {
            Console.WriteLine("参数错误...(Args error)\n");
            foreach (var error in result.Errors)
                Console.WriteLine(error);

            Console.WriteLine("");
            Cli.Run<Options>("-?");

            Console.ReadKey();
            return;
        }

        var options = result.Bind<Options>();
        try
        {
            Console.WriteLine("解压文件...(Decompress file)");
            ZipFile.ExtractToDirectory(options.ZipFileName, options.DecompressFolder, true);

            if (options.CopyFiles?.Any() == true)
            {
                Console.WriteLine("复制文件...(Copy files)");
                foreach (var file in options.CopyFiles)
                {
                    File.Copy(file, Path.Combine(options.DecompressFolder, Path.GetFileName(file)), true);
                }
            }

            if (!string.IsNullOrEmpty(options.RestartExeName))
            {
                Console.WriteLine("重启程序...(Restart exe)");

                Process mgrProcess = new();
                mgrProcess.StartInfo.FileName = Path.Combine(options.DecompressFolder, options.RestartExeName);
                mgrProcess.StartInfo.WorkingDirectory = options.DecompressFolder;
                mgrProcess.StartInfo.UseShellExecute = true;

                try
                {
                    mgrProcess.Start();
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                    Console.WriteLine("\n重启失败...(Restart fail)");
                    Console.ReadKey();
                    return;
                }
            }

            Console.WriteLine("更新成功！(Update Succeeded)");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            Console.WriteLine("\n更新失败...(Update fail)");
            Console.ReadKey();
            return;
        }
    }
}
