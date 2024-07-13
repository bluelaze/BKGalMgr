using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using BKGalMgr.Views.Controls;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class GroupToGroupsConverter : DependencyObject, IValueConverter
{
    public ObservableCollection<GroupInfo> Groups
    {
        get { return (ObservableCollection<GroupInfo>)GetValue(GroupsProperty); }
        set { SetValue(GroupsProperty, value); }
    }
    public static readonly DependencyProperty GroupsProperty = DependencyProperty.Register(
        nameof(Groups),
        typeof(ObservableCollection<GroupInfo>),
        typeof(GroupToGroupsConverter),
        new PropertyMetadata(null)
    );

    class GroupList : BindingList<GroupInfo>
    {
        public IList<string> Group { get; set; }
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (Groups == null)
            return null;

        var group = value as IList<string>;
        if (group == null)
            return null;

        GroupList gl = new() { Group = group };
        foreach (var item in Groups)
        {
            if (item.Name == GlobalInfo.GroupItemCase_Add)
                continue;
            gl.Add(new() { Name = item.Name, IsChecked = group.Contains(item.Name) });
        }
        gl.ListChanged += Groups_ListChanged;

        return gl;
    }

    private void Groups_ListChanged(object sender, ListChangedEventArgs e)
    {
        if (sender is GroupList gl)
        {
            foreach (var item in gl)
            {
                if (item.IsChecked)
                {
                    if (!gl.Group.Contains(item.Name))
                        gl.Group.Add(item.Name);
                }
                else
                {
                    if (gl.Group.Contains(item.Name))
                        gl.Group.Remove(item.Name);
                }
            }
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
