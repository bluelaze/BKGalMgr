using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static string MD5(this string value)
    {
        if (value.IsNullOrEmpty())
            return string.Empty;

        var data = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
        var sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }

    public static string ValidFileName(this string value, string replaceChar)
    {
        //https://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames
        return string.Join(replaceChar, value.Split(Path.GetInvalidFileNameChars()));
    }
}
