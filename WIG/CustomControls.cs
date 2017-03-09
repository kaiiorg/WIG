using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace kaiiorg.customcontrols
{

    /// <summary>
    /// Custom progress bar that follows the ForeColor property without turning visual styles off.
    /// Based on code found around the internet :P
    /// </summary>
    public class flatColorProgressBar : ProgressBar
    {
        public bool BackColorGradient { get; set; } = false;

        public flatColorProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            LinearGradientBrush brush = null;
            SolidBrush brush2 = null;
            Rectangle rec = new Rectangle(0, 0, this.Width, this.Height);
            double scaleFactor = (((double)Value - (double)Minimum) / ((double)Maximum - (double)Minimum));

            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);

            rec.Width = (int)((rec.Width * scaleFactor) - 4);
            rec.Height -= 4;
            if (BackColorGradient)
            {
                brush = new LinearGradientBrush(rec, this.ForeColor, this.BackColor, LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
            }
            else
            {
                brush2 = new SolidBrush(this.ForeColor);
                e.Graphics.FillRectangle(brush2, 2,2, rec.Width, rec.Height);
            }
            
        }
    }
}
