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
    [property: JsonIgnore]
    private string _illustration;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;

    [ObservableProperty]
    private string _CV;

    [ObservableProperty]
    private DateTime _birthday;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private string _cup;

    [ObservableProperty]
    private double _bust;

    [ObservableProperty]
    private double _waist;

    [ObservableProperty]
    private double _hips;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double _weight;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _bangumiCharacterId;

    [JsonIgnore]
    public string GameFolderPath { get; set; }

    [property: JsonIgnore]
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
