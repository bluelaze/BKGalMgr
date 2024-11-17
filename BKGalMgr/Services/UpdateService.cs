using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.Models;
using Octokit;

namespace BKGalMgr.Services;

public enum UpdateStatus
{
    Check,
    Checking,
    CheckFail,
    IsLatestVersion,
    DownloadNewVersion,
    Downloading,
    DownloadFail,
    RestartToUpdate,
    UpdateFail,
}

public class UpdateService
{
    public UpdateStatus CurrentUpdateStatus { get; set; } = UpdateStatus.Check;

    public string LastVersionUrl { get; set; }

    public string LastVersionZipPath { get; set; }

    public string UpdateFolder { get; init; } = Path.Combine(Directory.GetCurrentDirectory(), "Update");

    public Action<UpdateStatus, string> UpdateStatusChanged;

    public UpdateService() { }

    public string GetStatusMessage(string arg)
    {
        switch (CurrentUpdateStatus)
        {
            case UpdateStatus.Check:
                return LanguageHelper.GetString("Settings_About_Update_Check");
            case UpdateStatus.Checking:
                return LanguageHelper.GetString("Settings_About_Update_Checking");
            case UpdateStatus.CheckFail:
                return LanguageHelper.GetString("Settings_About_Update_CheckFail");
            case UpdateStatus.IsLatestVersion:
                return LanguageHelper.GetString("Settings_About_Update_IsLatestVersion");
            case UpdateStatus.DownloadNewVersion:
                return LanguageHelper.GetString("Settings_About_Update_DownloadNewVersion").Format(arg);
            case UpdateStatus.Downloading:
                return LanguageHelper.GetString("Settings_About_Update_Downloading").Format(arg);
            case UpdateStatus.DownloadFail:
                return LanguageHelper.GetString("Settings_About_Update_DownloadFail");
            case UpdateStatus.RestartToUpdate:
                return LanguageHelper.GetString("Settings_About_Update_RestartToUpdate");
            case UpdateStatus.UpdateFail:
                return LanguageHelper.GetString("Settings_About_Update_UpdateFail");
        }
        return "Error Update Status";
    }

    private void RiasesStatusChangedEvent(UpdateStatus updateStatus, string arg = null)
    {
        CurrentUpdateStatus = updateStatus;
        UpdateStatusChanged?.Invoke(CurrentUpdateStatus, GetStatusMessage(arg));
    }

    public async Task CheckForUpdates()
    {
        switch (CurrentUpdateStatus)
        {
            case UpdateStatus.Check:
            case UpdateStatus.CheckFail:
                {
                    try
                    {
                        RiasesStatusChangedEvent(UpdateStatus.Checking);
                        // https://octokitnet.readthedocs.io/en/documentation/http-client/#proxy-support
                        var client = new GitHubClient(new Connection(new ProductHeaderValue("BKGalMgr")));
                        var release = await client.Repository.Release.GetLatest("bluelaze", "BKGalMgr");
                        var lastVersion = Version.Parse(release.TagName.Substring(1));
                        if (lastVersion > Assembly.GetEntryAssembly().GetName().Version)
                        {
                            var asset = release.Assets.Where(t => t.Name.EndsWith("x64.zip")).First();
                            LastVersionUrl = asset.BrowserDownloadUrl;
                            LastVersionZipPath = Path.Combine(UpdateFolder, asset.Name);
                            RiasesStatusChangedEvent(UpdateStatus.DownloadNewVersion, release.TagName);
                        }
                        else
                        {
                            RiasesStatusChangedEvent(UpdateStatus.IsLatestVersion);
                        }
                    }
                    catch
                    {
                        RiasesStatusChangedEvent(UpdateStatus.CheckFail);
                    }
                }
                break;
            case UpdateStatus.Checking:
                break;
            case UpdateStatus.IsLatestVersion:
                break;
            case UpdateStatus.DownloadNewVersion:
            case UpdateStatus.DownloadFail:
                {
                    RiasesStatusChangedEvent(UpdateStatus.Downloading, "0");
                    using WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        float progress = e.TotalBytesToReceive > 0 ? e.BytesReceived / (float)e.TotalBytesToReceive : 0;
                        App.PostUITask(() =>
                        {
                            RiasesStatusChangedEvent(UpdateStatus.Downloading, "{0:0.0}%".Format(progress * 100));
                        });
                    };
                    webClient.DownloadFileCompleted += (
                        object sender,
                        System.ComponentModel.AsyncCompletedEventArgs e
                    ) =>
                    {
                        if (e.Error == null)
                            App.PostUITask(() =>
                            {
                                RiasesStatusChangedEvent(UpdateStatus.RestartToUpdate);
                            });
                        else
                            App.PostUITask(() =>
                            {
                                RiasesStatusChangedEvent(UpdateStatus.DownloadFail);
                            });
                    };
                    Directory.CreateDirectory(UpdateFolder);
                    await webClient.DownloadFileTaskAsync(LastVersionUrl, LastVersionZipPath);
                }
                break;
            case UpdateStatus.Downloading:
                break;
            case UpdateStatus.RestartToUpdate:
            case UpdateStatus.UpdateFail:
                {
                    try
                    {
                        // backup files
                        var settins = App.GetRequiredService<SettingsDto>();
                        settins.SaveSettings();
                        var settingsFile = Path.Combine(UpdateFolder, Path.GetFileName(settins.SettingsFilePath));
                        File.Copy(settins.SettingsFilePath, settingsFile, true);

                        var backupFiles = new List<string>();
                        backupFiles.Add(settingsFile);

                        // run update exe
                        var updateProgram = Path.Combine(UpdateFolder, "BKGalMgr.Update.exe");
                        File.Copy(
                            Path.Combine(Directory.GetCurrentDirectory(), "BKGalMgr.Update.exe"),
                            updateProgram,
                            true
                        );

                        Process updateProcess = new();
                        updateProcess.StartInfo.FileName = updateProgram;
                        updateProcess.StartInfo.WorkingDirectory = UpdateFolder;
                        updateProcess.StartInfo.UseShellExecute = true;
                        updateProcess.StartInfo.ArgumentList.Add($"--zip-file-name={LastVersionZipPath}");
                        updateProcess.StartInfo.ArgumentList.Add(
                            $"--decompress-folder={Directory.GetCurrentDirectory()}"
                        );
                        updateProcess.StartInfo.ArgumentList.Add($"--restart-exe-name=BKGalMgr.exe");
                        foreach (var file in backupFiles)
                            updateProcess.StartInfo.ArgumentList.Add($"--copy-files={file}");

                        updateProcess.Start();
                        App.MainWindow.Exit();
                    }
                    catch
                    {
                        RiasesStatusChangedEvent(UpdateStatus.UpdateFail);
                        return;
                    }
                }
                break;
            default:
                RiasesStatusChangedEvent(UpdateStatus.CheckFail);
                break;
        }
    }
}
