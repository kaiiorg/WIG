using System;
using System.Collections.Generic;
using System.Drawing;

namespace kaiiorg.wallplate
{
    /// <summary>
    /// The supported types of wallplate device.
    /// </summary>
    enum DeviceTypes { Outlet, Toggle, Rocker, None};

    /// <summary>
    /// A static class used to generate text for printing or engraving on United States standard electrical wallplates
    /// </summary>
    static class Wallplate
    {
        /// <summary>
        /// The standard electrical wallplate widths in inches for 1 to 10 devices per wallplate.
        /// </summary>
        static readonly float[] wallplateWidths = { 2.75F, 4.5F, 6.375F, 8.1875F, 10F, 11.8125F, 13.625F, 15.4375F, 17.25F, 19.0625F };
        /// <summary>
        /// The offset needed for to the middle of each device position, from the very edge
        /// </summary>
        static readonly float wallplateWidthCenterOffset = 1.375F;

        /// <summary>
        /// The standard electrical wallpalte height in inches.
        /// </summary>
        static readonly float wallplateHeight = 4.5F;

        /// <summary>
        /// The default type of wallplate device to draw
        /// </summary>
        static public DeviceTypes defaultDevice { get; set; } = DeviceTypes.Outlet;

        /// <summary>
        /// The Dots Per Inch (DPI) of the image. 
        /// Set this to the DPI of the printer or laser engraver for proper scaling.
        /// </summary>
        static public float dpi { get; set; } = 96;

        /// <summary>
        /// The pen for drawing lines.
        /// </summary>
        static public Pen linePen { get; set; } = new Pen(Color.Black, 1);

        /// <summary>
        /// The brush used to draw text.
        /// </summary>
        static public SolidBrush brush { get; set; } = new SolidBrush(Color.Black);

        /// <summary>
        /// The font used to draw text.
        /// </summary>
        static public Font font { get; set; } = new Font(FontFamily.GenericMonospace, 18, FontStyle.Bold);

        #region Image Rendering

        /// <summary>
        /// Draws a preview of the text on the outlet cover. 
        /// Should not be used on a laser engraver or printer, as it is only for visualization.
        /// </summary>
        /// <param name="deviceCount">The number of devices to render</param>
        /// <param name="text">The text to render at each device, empty or null strings are skipped.</param>
        /// <param name="deviceTypes">The type of device at each position</param>
        /// <returns>An image with text at each device position with the device preview</returns>
        static public Image renderPreview(int deviceCount, string[] text, DeviceTypes[] deviceTypes)
        {
            //Render the text and overlay the device preview on top of it.
            return CombineImages(renderText(deviceCount, text, deviceTypes),
                                 renderDevice(deviceCount, deviceTypes));
        }

        /// <summary>
        /// Draws a preview of the text on the outlet cover. 
        /// Should not be used on a laser engraver or printer, as it is only for visualization.
        /// </summary>
        /// <param name="deviceCount">The number of devices to render</param>
        /// <param name="text">The text to render at each device, empty or null strings are skipped.</param>
        /// <param name="deviceTypes">The type of device at each position</param>
        /// <param name="laser">The text only, without the preview.</param>
        /// <returns>An image with text at each device position with the device preview</returns>
        static public Image renderPreview(int deviceCount, string[] text, DeviceTypes[] deviceTypes, ref Image laser)
        {
            //Get the text only for the laser, return it via a call by reference.
            laser = renderText(deviceCount, text, deviceTypes);
            //Render the text and overlay the device preview on top of it.
            return CombineImages( new Bitmap(laser),
                                 renderDevice(deviceCount, deviceTypes));
        }

        /// <summary>
        /// Renders the devices. Should only be used as a preview.
        /// </summary>
        /// <param name="deviceCount">The number of devices to render</param>
        /// <param name="deviceTypes">The type of device at each position</param>
        /// <returns>An image with device preview</returns>
        static public Image renderDevice(int deviceCount, DeviceTypes[] deviceTypes)
        {
            if (deviceCount < 1 || deviceCount > 10)
                throw new IndexOutOfRangeException("deviceCount must be between 1 and 10");

            //Find the width and height in pixels.
            //DPI * height in inches.
            int width = (int)(dpi * wallplateWidths[deviceCount - 1]);
            int height = (int)(dpi * wallplateHeight);

            //Create the blank image 
            Image img = new Bitmap(width, height);
            Graphics wallplate = Graphics.FromImage(img);

            //Set the background color to white.
            wallplate.Clear(Color.Transparent);

            //Draw a rectangle around the outside.
            wallplate.DrawRectangle(linePen, 0, 0, width - 1, height - 1);


            //Add all the devices
            for (int i = 0; i < deviceCount; ++i)
            {
                //Offset to get to the center of the current device
                SizeF xyOffset = new SizeF(dpi * (wallplateWidths[i] - wallplateWidthCenterOffset),
                                            0);

                //Add new cases here (after adding to the DeviceTypes enum) to support new DeviceTypes
                switch (deviceTypes[i])
                {
                    case DeviceTypes.None:
                        continue;
                    case DeviceTypes.Outlet:
                        #region DeviceTypes.Outlet
                        //screw
                        wallplate.DrawEllipse(linePen, 
                                              xyOffset.Width - (dpi * 0.25F / 2),
                                              dpi * 2.125F,
                                              dpi * 0.25F,
                                              dpi * 0.25F);
                        //outlet (top)
                        wallplate.DrawLine(linePen, 
                                           xyOffset.Width - (dpi * 0.65F / 2), //offset - half line length
                                           dpi * 0.93F, 
                                           xyOffset.Width + (dpi * 0.65F / 2), 
                                           dpi * 0.93F);
                        wallplate.DrawLine(linePen,
                                           xyOffset.Width - (dpi * 0.65F / 2), //offset - half line length
                                           dpi * 2.03F,
                                           xyOffset.Width + (dpi * 0.65F / 2),
                                           dpi * 2.03F);
                                           
                        wallplate.DrawArc(linePen,
                                          xyOffset.Width - (dpi * (0.65F + 0.6F) / 2), //offset - half of line length and arc width)
                                          dpi * 0.93F,
                                          dpi * 0.6F,
                                          dpi * 1.1F,
                                          90F, 
                                          180F);
                        wallplate.DrawArc(linePen,
                                          xyOffset.Width - (dpi * ((0.6F / 2) - (0.65F / 2))), //offset - (half arc width - half line length)
                                          dpi * 0.93F,
                                          dpi * 0.6F,
                                          dpi * 1.1F,
                                          -90F,
                                          180F);
                        //outlet (bottom)
                        wallplate.DrawLine(linePen,
                                           xyOffset.Width - (dpi * 0.65F / 2), //offset - half line length
                                           dpi * 2.46F,
                                           xyOffset.Width + (dpi * 0.65F / 2),
                                           dpi * 2.46F);
                        wallplate.DrawLine(linePen,
                                           xyOffset.Width - (dpi * 0.65F / 2), //offset - half line length
                                           dpi * 3.56F,
                                           xyOffset.Width + (dpi * 0.65F / 2),
                                           dpi * 3.56F);

                        wallplate.DrawArc(linePen,
                                          xyOffset.Width - (dpi * (0.65F + 0.6F) / 2), //offset - half of line length and arc width)
                                          dpi * 2.46F,
                                          dpi * 0.6F,
                                          dpi * 1.1F,
                                          90F,
                                          180F);
                        wallplate.DrawArc(linePen,
                                          xyOffset.Width - (dpi * ((0.6F / 2) - (0.65F / 2))), //offset - (half arc width - half line length)
                                          dpi * 2.46F,
                                          dpi * 0.6F,
                                          dpi * 1.1F,
                                          -90F,
                                          180F);
                        #endregion
                        break;
                    case DeviceTypes.Toggle:
                        #region DeviceTypes.Toggle
                        //screw (top)
                        wallplate.DrawEllipse(linePen, 
                                              xyOffset.Width - (dpi * 0.25F / 2),
                                              dpi * 1.05F,
                                              dpi * 0.25F,
                                              dpi * 0.25F);
                        //screw (bottom)
                        wallplate.DrawEllipse(linePen, 
                                              xyOffset.Width - (dpi * 0.25F / 2),
                                              dpi * (1.05F + 2.375F),
                                              dpi * 0.25F,
                                              dpi * 0.25F);
                        //toggle cutout
                        wallplate.DrawRectangle(linePen,
                                                xyOffset.Width - (dpi * 0.42F / 2),
                                                dpi * 1.89F,
                                                dpi * 0.42F,
                                                dpi * 0.9F);
                        #endregion
                        break;
                    case DeviceTypes.Rocker:
                        #region DeviceTypes.Rocker
                        //screw (top)
                        wallplate.DrawEllipse(linePen,
                                              xyOffset.Width - (dpi * 0.25F / 2),
                                              dpi * (0.344F),
                                              dpi * 0.25F,
                                              dpi * 0.25F);
                        //screw (bottom)
                        wallplate.DrawEllipse(linePen,
                                              xyOffset.Width - (dpi * 0.25F / 2),
                                              dpi * 3.812F,
                                              dpi * 0.25F,
                                              dpi * 0.25F);
                        //Rocker cutout
                        wallplate.DrawRectangle(linePen,
                                                xyOffset.Width - (dpi * 1.310F / 2),
                                                dpi * 0.935F,
                                                dpi * 1.310F,
                                                dpi * 2.630F);
                        #endregion
                        break;
                    default:
                        throw new NotSupportedException("Device preview rendering for the " +
                                                        deviceTypes[i].ToString() +
                                                        " type of device isn't supported yet.");
                }
            }

            wallplate.Save();
            wallplate.Dispose();
            return img;
        }

        /// <summary>
        /// Renders the per-device text used for the printer or laser engraver.
        /// </summary>
        /// <param name="deviceCount">The number of devices to render</param>
        /// <param name="text">The text to render at each device, empty or null strings are skipped.</param>
        /// <param name="deviceTypes">The type of device at each position</param>
        /// <returns>An image with text at each device position</returns>
        static public Image renderText(int deviceCount, string[] text, DeviceTypes[] deviceTypes)
        {
            if (deviceCount < 1 || deviceCount > 10)
                throw new IndexOutOfRangeException("deviceCount must be between 1 and 10");

            //Find the width and height in pixels.
            //DPI * height in inches.
            int width = (int)(dpi * wallplateWidths[deviceCount - 1]);
            int height = (int)(dpi * wallplateHeight);

            //Create the blank image 
            Image img = new Bitmap(width, height);
            Graphics wallplate = Graphics.FromImage(img);

            //Set the background color to white.
            wallplate.Clear(Color.White);

            //Add all the strings, skip if there isn't a string to add
            for (int i = 0; i < deviceCount; ++i)
            {
                if (text[i] == null || text[i] == "" || deviceTypes[i] == DeviceTypes.None)
                    continue;

                SizeF textSize = wallplate.MeasureString(text[i], font);
                SizeF xyOffset = new SizeF(0, 0);

                //Add new cases here (after adding to the DeviceTypes enum) to support new DeviceTypes
                switch (deviceTypes[i])
                {
                    case DeviceTypes.None:
                        continue;
                    case DeviceTypes.Outlet:
                    case DeviceTypes.Toggle:
                        //X offset              (width of wallplate) - (offset from edge)      -     (half of text width)
                        xyOffset.Width = (dpi * (wallplateWidths[i] - wallplateWidthCenterOffset)) - (textSize.Width / 2);
                        //Y offset
                        xyOffset.Height = dpi * 0.25F;
                        break;
                    case DeviceTypes.Rocker:
                        //X offset              (width of wallplate) - (offset from edge)      -     (half of text width)
                        xyOffset.Width = (dpi * (wallplateWidths[i] - wallplateWidthCenterOffset)) - (textSize.Width / 2);
                        //Y offset
                        xyOffset.Height = dpi * 0.6F;
                        break;
                    default:
                        throw new NotSupportedException("Text rendering for the " + 
                                                        deviceTypes[i].ToString() +
                                                        " type of device isn't supported yet.");
                }

                //Draw the string
                wallplate.DrawString(
                                text[i],
                                font,
                                brush,
                                xyOffset.Width,
                                xyOffset.Height
                            );
            }

            wallplate.Save();
            wallplate.Dispose();
            return img;
        }

        #endregion

        #region Utility functions
        static private Image CombineImages(Image a, Image b)
        {
            using (Graphics g = Graphics.FromImage(a))
            {
                g.DrawImage(b, 0, 0);
                g.Save();
            }
            return a;
        }
        #endregion
    }

    //Basic class to represent the data of a single plate.
    class Plate
    {
        public List<DeviceTypes> deviceTypes;
        public List<string> text;
        public List<string> fileNames;
        public int count;

        public Plate()
        {
            deviceTypes = new List<DeviceTypes>();
            text = new List<string>();
            fileNames = new List<string>();
            count = 0;
        }
    }
}
