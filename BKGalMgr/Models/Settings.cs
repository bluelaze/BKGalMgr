﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;

namespace BKGalMgr.Models;

public class Settings
{
    public List<string> RepositoryPath { get; set; } = new();
    public string SelectedRepositoryPath { get; set; } = string.Empty;
    public Theme AppTheme { get; set; } = Theme.Light;
    public BackdropMaterial AppBackdropMaterial { get; set; } = BackdropMaterial.Mica;
    public CompressionLevel ZipLevel { get; set; } = CompressionLevel.NoCompression;
    public SupportLanguages Language { get; set; } = SupportLanguages.system;

    public BangumiInfo Bangumi { get; set; } = new();

    public LocalEmulatorInfo LocalEmulator { get; set; } = new();
}
