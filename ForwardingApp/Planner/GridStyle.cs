using System;
using System.Drawing;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;

namespace ASolute_Mobile.Planner
{
	public class GridStyle : DataGridStyle
    {

        public override Color GetHeaderForegroundColor()
        {
            return Color.FromRgb(255, 255, 255);
        }

        public override Color GetHeaderBackgroundColor()
        {
            return Color.FromRgb(251, 232, 233);
        }

        public override GridLinesVisibility GetGridLinesVisibility()
        {
            return GridLinesVisibility.Horizontal;
        }

        public override Color GetAlternatingRowBackgroundColor()
        {
            return Color.FromRgb(251, 232, 233);
        }

        public override float GetHeaderBorderWidth()
        {
            return 4;
        }

        public override Color GetCaptionSummaryRowBackgroundColor()
        {
            return Color.FromRgb(155, 212, 245);
        }
    }
}
