using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Markup;

namespace BKGalMgr.Extensions;

[MarkupExtensionReturnType(ReturnType = typeof(Type))]
public sealed partial class EnumTypeExtension : MarkupExtension
{
    public Type Type { get; set; }

    /// <inheritdoc/>
    protected override object ProvideValue()
    {
        return Type;
    }
}
