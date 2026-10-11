using System.Drawing;
using System.Reflection;
using ShareX.HelpersLib;
using ShareX.ScreenCaptureLib;

namespace BKGalMgr.ThirdParty;

public class ScreenCapture
{
    public struct DataStruct
    {
        public Bitmap CaptureBitmap;
        public RectangleF CaptureRect;
    }

    public ScreenCapture() { }

    public static DataStruct CaptureRegion()
    {
        DataStruct result = new DataStruct();

        Screenshot screenshot = new Screenshot();
        screenshot.CaptureCursor = false;

        using Bitmap canvas = screenshot.CaptureFullscreen();
        using RegionCaptureForm form = new RegionCaptureForm(
            RegionCaptureMode.Default,
            new RegionCaptureOptions(),
            canvas
        );

        if (
            typeof(RegionCaptureForm)
                .GetProperty("ShapeManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(form)
            is object shapeManagerObj
        )
        {
            List<SimpleWindowInfo>? windows =
                shapeManagerObj
                    .GetType()
                    .GetProperty("Windows", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(shapeManagerObj) as List<SimpleWindowInfo>;

            if (windows != null)
            {
                if (
                    RegionCaptureForm.LastRegionFillPath != null
                    && !windows.Any(t => t.Rectangle == RegionCaptureForm.LastRegionFillPath.GetBounds().Round())
                )
                {
                    windows.Insert(
                        0,
                        new SimpleWindowInfo(IntPtr.Zero)
                        {
                            IsWindow = true,
                            Rectangle = RegionCaptureForm.LastRegionFillPath.GetBounds().Round(),
                        }
                    );
                }
            }
        }

        form.ShowDialog();

        result.CaptureBitmap = form.GetResultImage();

        if (RegionCaptureForm.LastRegionFillPath != null)
        {
            result.CaptureRect = RegionCaptureForm.LastRegionFillPath.GetBounds().Round();
        }
        return result;
    }
}
