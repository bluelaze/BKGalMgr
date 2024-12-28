using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
    // ���ѡ��ָ��������Ԫ��
    public static IEnumerable<T> TakeRandom<T>(this IList<T> source, int count)
    {
        if (source == null || source.Count == 0)
            return null;
        if (count <= 0)
            return null;
        if (count > source.Count)
            count = source.Count;

        // Fisher-Yatesϴ���㷨
        var indices = Enumerable.Range(0, source.Count).ToList();
        for (int i = 0; i < count; i++)
        {
            int j = rnd.Next(i, indices.Count);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        return indices.Take(count).Select(i => source[i]);
    }
}
