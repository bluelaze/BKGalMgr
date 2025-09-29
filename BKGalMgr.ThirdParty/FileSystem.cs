using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BKGalMgr.ThirdParty;

public class FileSystem
{
    public static string[]? PickFile(string initialDirectory, List<string> fileTypeFilter)
    {
        using OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.InitialDirectory = initialDirectory;
        openFileDialog.Filter = string.Join('|', fileTypeFilter);
        openFileDialog.RestoreDirectory = true;
        openFileDialog.Multiselect = true;
        if (openFileDialog.ShowDialog() == DialogResult.OK)
            return openFileDialog.FileNames;

        return null;
    }
}
