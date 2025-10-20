using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Markup;

namespace BKGalMgr.Extensions;

/// <summary>
/// A markup extension that returns a collection of values of a specific <see langword="enum"/>
/// </summary>
[MarkupExtensionReturnType(ReturnType = typeof(Array))]
public sealed partial class EnumNamesExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the <see cref="global::System.Type"/> of the target <see langword="enum"/>
    /// </summary>
    public Type? Type { get; set; }

    /// <inheritdoc/>
    protected override object ProvideValue()
    {
        // 从toolkit抄的，这个主要的局限是如果是winrt的类型，ComboBox就没法正确显示出来
        // 所以需要现在这里转成string，再通过EnumStringConveretr来进行转换回去
        // TODO: We should probably make a throw helper and throw here if type is null?
        return Enum.GetNames(Type!);
    }
}
