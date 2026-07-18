using System;
using System.Collections.Generic;
using System.Text;

namespace BKGalMgr.Interfaces.Page;

public interface IExtendsContentIntoTitleBarPage
{
    public record PageParameter(IExtendsContentIntoTitleBarPage ParentPage, object Parameter = null);

    public void ShowExtendedContent();

    public void HideExtendedContent();

    public virtual void NavigateTo(Type pageType, object parameter = null) { }
}
