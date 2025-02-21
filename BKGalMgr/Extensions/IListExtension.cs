using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Extensions;

public static class IListExtension
{
    public static void AddRange<TSource>(this IList<TSource> source, IEnumerable<TSource> collection)
    {
        if (collection == null)
        {
            return;
        }

        using (IEnumerator<TSource> en = collection.GetEnumerator())
        {
            while (en.MoveNext())
            {
                source.Add(en.Current);
            }
        }
    }

    public static void MergeRange<TSource>(this IList<TSource> source, IEnumerable<TSource> collection)
    {
        if (collection == null)
        {
            return;
        }

        source.AddRange(collection.Except(source));
    }

    public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        if (source == null)
        {
            return -1;
        }

        if (predicate == null)
        {
            return -1;
        }

        for (int i = 0; i < source.Count(); i++)
        {
            if (predicate(source.ElementAt(i)))
            {
                return i;
            }
        }

        return -1;
    }

    public static void RemoveIf<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
    {
        if (source == null || predicate == null)
            return;

        for (int i = source.Count() - 1; i >= 0; i--)
        {
            if (predicate(source.ElementAt(i)))
            {
                source.RemoveAt(i);
            }
        }
    }

    private static readonly Random rnd = new Random();

    // 随机选择指定数量的元素
    public static IEnumerable<T> TakeRandom<T>(this IList<T> source, int count)
    {
        if (source == null || source.Count == 0)
            return null;
        if (count <= 0)
            return null;
        if (count > source.Count)
            count = source.Count;

        // Fisher-Yates洗牌算法
        var indices = Enumerable.Range(0, source.Count).ToList();
        for (int i = 0; i < count; i++)
        {
            int j = rnd.Next(i, indices.Count);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        return indices.Take(count).Select(i => source[i]);
    }

    //https://stackoverflow.com/questions/3099581/sorting-an-array-of-folder-names-like-windows-explorer-numerically-and-alphabet
    public class StrCmpLogicalComparer : IComparer<string>
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern int StrCmpLogicalW(String x, String y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }

    public static List<string> SortByName(this List<string> source)
    {
        if (source == null)
            return null;

        source.Sort(new StrCmpLogicalComparer());
        return source;
    }
}
