using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public enum SortType
{
    CreateDate,
    LastPlayDate,
    PlayedTime,
    Name,
    Company,
    PublishDate,
    PinValue,
}

public static class GlobalInfo
{
    /**
    /(repository_folder)
        repositoryinfo.json
        (yyyyMMddTHHmmssZ)
            gameinfo.json
            /sources
                (yyyyMMddTHHmmssZ)
                    sourceinfo.json
                    source.zip
            /localizations
                (yyyyMMddTHHmmssZ)
                    localizationinfo.json
                    localization.zip
            /targets
                (yyyyMMddTHHmmssZ)
                    targetinfo.json
                    /target
                        (StartupName)
            /savedatas
                savedatasettingsinfo.json
                (yyyyMMddTHHmmssZ)
                    savedatainfo.json
                    savedata.zip
        (yyyyMMddTHHmmssZ)
        ......
     **/
    public const string FolderFormatStr = "yyyyMMddTHHmmssZ";

    public const string RepositoryJsonName = "repositoryinfo.json";

    public const string GroupItemCase_Add = "PlaceholderForAdd";

    public const string GameJsonName = "gameinfo.json";

    public const string GameCoverName = "cover";
    public const string GameCoversFolderName = "covers";
    public static readonly string[] GameCoverSupportFormats = [".gif", ".webp", ".png", ".jpg", ".jpeg"];

    public const string GameScreenCaptureFolderName = "screencaptures";
    public const string GameScreenCaptureFileFormatStr = "yyyy-MM-dd_HH-mm-ss";

    public const string GameCharacterFolderName = "characters";

    public const string GameGalleryFolderName = "gallery";
    public const string GameSpecialFolderName = "special";

    public const string GameShortcutFolderName = "shortcut";

    public const string SourcesFolderName = "sources";
    public const string SourceJsonName = "sourceinfo.json";
    public const string SourceZipName = "source.zip";

    public const string LocalizationsFolderName = "localizations";
    public const string LocalizationJsonName = "localizationinfo.json";
    public const string LocalizationZipName = "localization.zip";

    public const string TargetsFolderName = "targets";
    public const string TargetJsonName = "targetinfo.json";
    public const string TargetName = "target";
    public const string TargetZipName = "target.zip";

    public const string SaveDatasFolderName = "savedatas";
    public const string SaveDataSettingsJsonName = "savedatasettingsinfo.json";
    public const string SaveDataJsonName = "savedatainfo.json";
    public const string SaveDataZipName = "savedata.zip";
}
