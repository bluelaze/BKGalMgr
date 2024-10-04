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

public static class ObservableCollectionExtension
{
    public static void AddRange<T>(this ObservableCollection<T> value, IEnumerable<T> collection)
    {
        if (collection == null)
        {
            return;
        }

        using (IEnumerator<T> en = collection.GetEnumerator())
        {
            while (en.MoveNext())
            {
                value.Add(en.Current);
            }
        }
    }

    public static void MergeRange<T>(this ObservableCollection<T> value, IEnumerable<T> collection)
    {
        if (collection == null)
        {
            return;
        }

        value.AddRange(collection.Except(value));
    }
}
