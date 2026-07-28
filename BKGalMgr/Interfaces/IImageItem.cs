using System;
using System.Collections.Generic;
using System.Text;

namespace BKGalMgr.Interfaces;

public interface IImageItem
{
    object Args { get; set; }
    string Image { get; set; }

    public void DeleteImage();

    public void SetAsGameBackground();

    public void SetAsAppBackground();
}
