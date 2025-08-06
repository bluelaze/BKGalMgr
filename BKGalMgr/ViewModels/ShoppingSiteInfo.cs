using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public partial class ShoppingSiteInfo : ObservableObject
{
    public enum DLsiteProductType
    {
        maniax,
        pro,
    }

    [ObservableProperty]
    private string _GetchuProductId;

    [ObservableProperty]
    private string _DMMProductId;

    [ObservableProperty]
    private string _DLsiteProductId;

    [ObservableProperty]
    private DLsiteProductType _dLsiteType = DLsiteProductType.pro;

    private string _GetchuWebsit = "https://www.getchu.com";

    private string _DMMWebsite = "https://dlsoft.dmm.co.jp";

    private string _DLsite = "https://www.dlsite.com";

    public bool GetchuIsValid()
    {
        return !GetchuProductId.IsNullOrEmpty();
    }

    public bool DMMIsValid()
    {
        return !DMMProductId.IsNullOrEmpty();
    }

    public bool DLsiteIsValid()
    {
        return !DLsiteProductId.IsNullOrEmpty();
    }

    public void GetchuUrlToProductId()
    {
        // https://www.getchu.com/soft.phtml?id=828103
        if (GetchuProductId?.StartsWith("http") != true)
            return;

        int idIndex = GetchuProductId.IndexOf("id=");
        if (idIndex != -1)
        {
            GetchuProductId = GetchuProductId.Substring(idIndex + 3);
        }
    }

    public void DmmUrlToProductId()
    {
        // https://dlsoft.dmm.co.jp/detail/russ_0148/
        if (DMMProductId?.StartsWith("http") != true)
            return;

        DMMProductId = DMMProductId.Split('?').FirstOrDefault();
        DMMProductId = DMMProductId.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
    }

    public void DLsiteUrlToProductId()
    {
        // https://www.dlsite.com/books/work/=/product_id/BJ184578.html
        if (DLsiteProductId?.StartsWith("http") != true)
            return;

        DLsiteProductId = DLsiteProductId.Split('/').LastOrDefault();
        DLsiteProductId = DLsiteProductId.Split('.').FirstOrDefault();
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenInGetchu()
    {
        UrlMisc.OpenUrl($"{_GetchuWebsit}/soft.phtml?id={GetchuProductId}");
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenInDMM()
    {
        UrlMisc.OpenUrl($"{_DMMWebsite}/detail/{DMMProductId}");
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenInDLsite()
    {
        UrlMisc.OpenUrl($"{_DLsite}/{DLsiteType}/work/=/product_id/{DLsiteProductId}");
    }
}
