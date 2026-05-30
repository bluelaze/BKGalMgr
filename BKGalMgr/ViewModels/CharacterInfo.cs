using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public partial class CharacterInfo : ObservableObject
{
    [ObservableProperty]
    [JsonIgnore]
    public partial string Illustration { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string TranslatedName { get; set; }

    [ObservableProperty]
    public partial string Id { get; set; } = Guid.NewGuid().ToString();

    [ObservableProperty]
    public partial DateTime CreateDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial string CV { get; set; }

    [ObservableProperty]
    public partial DateTime Birthday { get; set; }

    [ObservableProperty]
    public partial int Age { get; set; }

    [ObservableProperty]
    public partial string BloodType { get; set; }

    [ObservableProperty]
    public partial string Cup { get; set; }

    [ObservableProperty]
    public partial double Bust { get; set; }

    [ObservableProperty]
    public partial double Waist { get; set; }

    [ObservableProperty]
    public partial double Hips { get; set; }

    [ObservableProperty]
    public partial double Height { get; set; }

    [ObservableProperty]
    public partial double Weight { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    public partial string BangumiCharacterId { get; set; }

    [JsonIgnore]
    public string GameFolderPath { get; set; }

    [JsonIgnore]
    public string FolderPath => Path.Combine(GameFolderPath, GlobalInfo.GameCharacterFolderName);

    public void LoadIllustration()
    {
        string illustration = null;
        foreach (var format in GlobalInfo.GameCoverSupportFormats)
        {
            var path = Path.Combine(FolderPath, Id + format);
            if (File.Exists(path))
            {
                illustration = path;
                break;
            }
        }
        Illustration = illustration;
    }

    public string TransformIllustrationPath(string path)
    {
        string absolutePath = path.StartsWith("http") ? (new Uri(path)).AbsolutePath : path;
        var format = Path.GetExtension(absolutePath).ToLower();
        if (GlobalInfo.GameCoverSupportFormats.Contains(format))
            return Path.Combine(FolderPath, Id + format);
        return string.Empty;
    }

    public async Task SaveIllustration()
    {
        if (Illustration.IsNullOrEmpty())
        {
            LoadIllustration();
            return;
        }

        // check format, build path
        var illPath = TransformIllustrationPath(Illustration);
        if (illPath.IsNullOrEmpty())
            return;

        Directory.CreateDirectory(FolderPath);
        // copy local file
        if (File.Exists(Illustration) && Path.GetDirectoryName(Illustration) != FolderPath)
        {
            File.Copy(Illustration, illPath, true);
            Illustration = illPath;
            return;
        }

        // network image
        if (!Illustration.StartsWith("http"))
        {
            LoadIllustration();
            return;
        }
        try
        {
            var fileData = await (new HttpClient()).GetByteArrayAsync(Illustration);
            await File.WriteAllBytesAsync(illPath, fileData);
            Illustration = illPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return;
        }
    }
}
