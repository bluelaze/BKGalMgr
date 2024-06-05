using System.Drawing;
using ShareX.HelpersLib;
using ShareX.ScreenCaptureLib;

namespace BKGalMgr.ThirdParty;

public class ScreenCapture
{
    public struct DataStruct
    {
        public Bitmap captureBmp;
        public RectangleF captureRect;
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

        form.ShowDialog();

        result.captureBmp = form.GetResultImage();

        if (RegionCaptureForm.LastRegionFillPath != null)
        {
            Rectangle screenRectangle = CaptureHelpers.GetScreenBounds();
            result.captureRect = RectangleF.Intersect(
                RegionCaptureForm.LastRegionFillPath.GetBounds(),
                new RectangleF(0, 0, screenRectangle.Width, screenRectangle.Height)
            );
        }
        return result;
    }
}
