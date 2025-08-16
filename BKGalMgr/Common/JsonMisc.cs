using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace BKGalMgr.Common;

public class JsonMisc
{
    public static string Serialize<T>(
        T obj,
        bool ignoreNullValues = true,
        bool writeIndented = true,
        JavaScriptEncoder charsetEncoder = null
    )
    {
        if (charsetEncoder == null)
            charsetEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = ignoreNullValues ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
            WriteIndented = writeIndented,
            Encoder = charsetEncoder,
        };
        return JsonSerializer.Serialize<T>(obj, options);
    }

    public static T Deserialize<T>(string str)
    {
        return JsonSerializer.Deserialize<T>(str);
    }

    public static T CloneObject<T>(T obj)
    {
        return Deserialize<T>(Serialize(obj));
    }
}
