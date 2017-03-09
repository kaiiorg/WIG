using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using kaiiorg.wallplate;
using kaiiorg.customcontrols;

//WIG: Wallplate Image Generator
namespace WIG
{
    public partial class MainForm : Form
    {
        //If the preview image box is in its own window
        static public bool previewInOwnWindow = false;
        PreviewForm previewForm;
        AboutForm aboutForm;

        //Queue of plate information
        static Queue<Plate> plates = new Queue<Plate>();
        //Queue of images generated from the plate information
        static Queue<Image> previewImages = new Queue<Image>();
        static Queue<Image> laserImages = new Queue<Image>();
        //The number of images that have been reviewed
        static int imagesReviewed = 0;
        //Total images
        static int totalImages = 0;
        //The max queue depth.
        const int maxQueueDepth = 20;
        //The worker thread to process the images
        BackgroundWorker batchProcessor = new BackgroundWorker();

        //Get all of the DeviceType names
        string[] deviceNames = Enum.GetNames(typeof(DeviceTypes));

        #region Form Handling
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Populate the comboboxes
            //Get the comboboxes. They seem to always be in reverse order. This method may go terribily wrong later.
            List<ComboBox> comboBoxes = individualModeDataGroupBox.Controls.OfType<ComboBox>().ToList();
            comboBoxes.Reverse();

            for(int i = 0; i < 10; ++i)
            {
                comboBoxes[i].Items.AddRange(deviceNames);
                comboBoxes[i].SelectedIndex = 0;
            }

            //Setup the background worker
            batchProcessor.WorkerReportsProgress = true;
            batchProcessor.WorkerSupportsCancellation = true;
            batchProcessor.DoWork += new DoWorkEventHandler(batchProcessor_DoWork);
            batchProcessor.ProgressChanged += new ProgressChangedEventHandler(batchProcessor_ProgressChanged);
            batchProcessor.RunWorkerCompleted += new RunWorkerCompletedEventHandler(batchProcessor_RunWorkerCompleted);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        //Put the preview tab page back in the main tab control
        private void previewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mainTabControl.TabPages.Add(previewForm.previewTabControl.TabPages["previewTabPage"]);
            previewInOwnWindow = false;
        }
        #endregion

        #region Menu Strip
        //Move the preview tab page from one form to another.
        private void movePreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (previewInOwnWindow)
            {
                previewForm.Close();
                previewInOwnWindow = false;
            }
            else
            {
                previewForm = new PreviewForm();
                previewForm.FormClosing += new FormClosingEventHandler(previewForm_FormClosing);

                previewForm.previewTabControl.TabPages.Add(mainTabControl.TabPages["previewTabPage"]);
                previewForm.Show();
                previewInOwnWindow = true;
            }
        }

        //Exit program
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        //Show about box.
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutForm = new AboutForm();
            aboutForm.Show();
        }
        #endregion

        #region Control Handling
        //Set the new dpi on value change.
        private void dpiNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            Wallplate.dpi = (float)dpiNumericUpDown.Value;
        }

        private void changeFontButton_Click(object sender, EventArgs e)
        {
            FontDialog f = new FontDialog();
            f.Font = Wallplate.font;
            f.ShowApply = false;
            f.FontMustExist = true;
            if(f.ShowDialog() != DialogResult.Cancel)
            {
                Wallplate.font = f.Font;
            }
        }

        //Set the value in the trackbar to the numeric updown
        private void deviceCountTrackBar_Scroll(object sender, EventArgs e)
        {
           deviceCountNumericUpDown.Value = deviceCountTrackBar.Value;
        }
        //Set the value in the numeric updown to the trackbar
        private void deviceCountNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            deviceCountTrackBar.Value = (int)deviceCountNumericUpDown.Value;
            changeDeviceNumber();
        }
        //Enable/Disable the number of combox and text boxes according to the numeric up/down
        private void changeDeviceNumber()
        {
            //Get the comboboxes. They seem to always be in reverse order. This method may go terribily wrong later.
            List<ComboBox> comboBoxes = individualModeDataGroupBox.Controls.OfType<ComboBox>().ToList();
            comboBoxes.Reverse();
            //Get the textboxes. They seem to always be in reverse order. This method may go terribily wrong later.
            List<TextBox> textBoxes = individualModeDataGroupBox.Controls.OfType<TextBox>().ToList();
            textBoxes.Reverse();

            for(int i = 0; i < 10; ++i)
            {
                //Enable the control if its number is less than or equal to the numeric up/down
                comboBoxes[i].Enabled = i <= deviceCountNumericUpDown.Value - 1;
                textBoxes[i].Enabled = i <= deviceCountNumericUpDown.Value - 1;
                //Select the first option of the comboBox and clear the text of the textbox if it gets disabled.
                if(!(i <= deviceCountNumericUpDown.Value - 1))
                {
                    comboBoxes[i].SelectedIndex = 0;
                    textBoxes[i].Text = "";
                }
            }
        }
        #endregion

        #region Individual Mode
        //Generate the preview
        private void previewButton_Click(object sender, EventArgs e)
        {
            //Get the comboboxes. They seem to always be in reverse order. This method may go terribily wrong later.
            List<ComboBox> comboBoxes = individualModeDataGroupBox.Controls.OfType<ComboBox>().ToList();
            comboBoxes.Reverse();
            //Get the textboxes. They seem to always be in reverse order. This method may go terribily wrong later.
            List<TextBox> textBoxes = individualModeDataGroupBox.Controls.OfType<TextBox>().ToList();
            textBoxes.Reverse();

            Plate p = new Plate();
            p.count = (int)deviceCountNumericUpDown.Value;

            for (int i = 0; i < deviceCountNumericUpDown.Value; ++i)
            {
                DeviceTypes d = (DeviceTypes)Enum.Parse(typeof(DeviceTypes), comboBoxes[i].SelectedItem.ToString(), true);

                p.deviceTypes.Add(d);
                p.text.Add(textBoxes[i].Text);
            }

            //Delete the current picture and force GC to collect.
            if (previewPictureBox.Image != null)
            {
                previewPictureBox.Image.Dispose();
                GC.Collect();
            }
            if (laserPictureBox.Image != null)
            {
                laserPictureBox.Image.Dispose();
                GC.Collect();
            }

            Image img = new Bitmap(1,1);
            
            //Set the preview image and the hidden laser image
            previewPictureBox.Image = Wallplate.renderPreview(
                                               p.count,
                                               p.text.ToArray(),
                                               p.deviceTypes.ToArray(), 
                                               ref img);

            laserPictureBox.Image = img;

            previewPictureBox.BringToFront();
            showPreviewButton.Enabled = false;
            showLaserButton.Enabled = true;

        }

        //Save the preview to file.
        private void savePreviewButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog savePath = new SaveFileDialog();
            savePath.Filter = "Bitmap files (*.bmp)|*.bmp";
            savePath.FilterIndex = 1;
            savePath.RestoreDirectory = true;

            if (savePath.ShowDialog() == DialogResult.OK)
            {
                //Get the image from the preview box
                Image img = previewPictureBox.Image;
                img.Save(savePath.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        private void saveLaserButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog savePath = new SaveFileDialog();
            savePath.Filter = "Bitmap files (*.bmp)|*.bmp";
            savePath.FilterIndex = 1;
            savePath.RestoreDirectory = true;

            if (savePath.ShowDialog() == DialogResult.OK)
            {
                //Get the image from the preview box
                Image img = laserPictureBox.Image;
                img.Save(savePath.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        private void showLaserButton_Click(object sender, EventArgs e)
        {
            showPreviewButton.Enabled = true;
            laserPictureBox.BringToFront();
            showLaserButton.Enabled = false;
        }

        private void showPreviewButton_Click(object sender, EventArgs e)
        {
            showLaserButton.Enabled = true;
            previewPictureBox.BringToFront();
            showPreviewButton.Enabled = false;
        }

        #endregion

        #region Batch Mode

        private void batchImportButton_Click(object sender, EventArgs e)
        {
            //Clear everything in the plates queue
            while (plates.Count > 0)
            {
                lock (plates)
                {
                    plates.Dequeue();
                }
            }

            //Read in .csv file

            //TEST DATA:
            Random rnd = new Random();
            for (int i = 0; i < 30; ++i)
            {
                Plate tempPlate = new Plate();
                tempPlate.fileNames.Add(string.Format("wallplate_{0}", i + 1));
                for (int j = 1; j <= rnd.Next(1, 10); ++j)
                {
                    tempPlate.count = j;
                    tempPlate.text.Add(string.Format("Sample {0}", j));
                    DeviceTypes d = DeviceTypes.None;
                    switch (rnd.Next(0, 2))
                    {
                        case 0:
                            d = DeviceTypes.Outlet;
                            break;
                        case 1:
                            d = DeviceTypes.Toggle;
                            break;
                        default:
                            d = DeviceTypes.Rocker;
                            break;
                    }
                    tempPlate.deviceTypes.Add(d);
                }
                //Probably don't need to lock it, but just to be safe.
                lock (plates)
                    plates.Enqueue(tempPlate);
            }

            //Enable the start button if the List<Plate> size is > 0
            if (plates.Count > 0)
                batchStartButton.Enabled = true;
        }

        private void batchExportTemplateButton_Click(object sender, EventArgs e)
        {
            //Export the template

        }

        private void batchStartButton_Click(object sender, EventArgs e)
        {
            batchStartButton.Enabled = false;
            gotoBatchMode(true);
            
        }

        #region Batch Processor Events
        //Worker in a new thread to do the processing in the background
        //Waits when the queue is at the max queue depth
        //Ends when the plates queue is empty
        static void batchProcessor_DoWork(object sender, EventArgs e)
        {
 
                BackgroundWorker worker = (BackgroundWorker)sender;
                Plate tempPlate;
                Image laserImage = new Bitmap(1,1);
                Image previewImage = new Bitmap(1, 1);

                //Loop until there are no plates left to work on and the worker hasn't been told to stop.
                while (plates.Count > 0 && !worker.CancellationPending)
                {
                    //Loop until the depth limit is reached and the worker hasn't been told to stop.
                    while (laserImages.Count < maxQueueDepth && !worker.CancellationPending)
                    {
                        //Lock the plates queue, dequeue the first plate
                        lock (plates)
                            tempPlate = plates.Dequeue();

                        //Generate the Images
                        previewImage = Wallplate.renderPreview(
                            tempPlate.count, 
                            tempPlate.text.ToArray(), 
                            tempPlate.deviceTypes.ToArray(), 
                            ref laserImage);

                        //Lock the laser image queue, add the laserImage
                        lock (laserImages)
                            laserImages.Enqueue(laserImage);
                        //Lock the preview image queue, add the previewImage
                        lock (previewImage)
                            previewImages.Enqueue(previewImage);

                        //Report how full the queue is.
                        worker.ReportProgress((int)((double)laserImages.Count / maxQueueDepth * 100.0));
                    Thread.Sleep(25);
                    }
                Thread.Sleep(250);
                }

        }

        private void batchProcessor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            updateProgressBar(e.ProgressPercentage);
        }

        private void batchProcessor_RunWorkerCompleted(object sender, EventArgs e)
        {

        }
        #endregion

        private void batchAbortButton_Click(object sender, EventArgs e)
        {
            gotoBatchMode(false);
        }

        private void batchAcceptButton_Click(object sender, EventArgs e)
        {
            //Save the image to file.

            //Load the next image to the picture box.
            showNextImages();
            updateProgressBar((int)((double)laserImages.Count / maxQueueDepth * 100.0));
        }

        private void batchRejectButton_Click(object sender, EventArgs e)
        {
            //Add values to .csv file for error output

            //Load the next image to the picture box.
            showNextImages();
            updateProgressBar((int)((double)laserImages.Count / maxQueueDepth * 100.0));
        }

        #region Util

        /// <summary>
        /// Makes all changes needed to enter batch mode.
        /// If <paramref name="toBatch"/> is false, makes changes to go back to individual mode.
        /// </summary>
        /// <param name="toBatch">If batch mode or individual mode should be entered.</param>
        private void gotoBatchMode(bool toBatch)
        {
            //Enable/Disable all the individual mode related controls.
            previewButton.Enabled = !toBatch;
            saveLaserButton.Enabled = !toBatch;
            savePreviewButton.Enabled = !toBatch;
            deviceCountNumericUpDown.Enabled = !toBatch;
            deviceCountTrackBar.Enabled = !toBatch;
            deviceCountNumericUpDown.Value = 1;
            changeDeviceNumber();

            //Enable/Disable all the batch related controls.
            bofLabel.Enabled = toBatch;
            bnLabel.Enabled = toBatch;
            bqdLabel.Enabled = toBatch;
            batchCurrentLabel.Enabled = toBatch;
            batchTotalLabel.Enabled = toBatch;
            batchProgressBar.Enabled = toBatch;
            batchQueueDepthProgressBar.Enabled = toBatch;
            batchAcceptAllCheckBox.Enabled = toBatch;
            batchAcceptAllCheckBox.Checked = false;
            batchAcceptButton.Enabled = toBatch;
            batchRejectButton.Enabled = toBatch;
            batchAbortButton.Enabled = toBatch;
            batchStartButton.Enabled = toBatch;

            if (toBatch)
            {
                
                //Disable all of the text boxes and comboboxes
                List<TextBox> textBoxes = individualModeDataGroupBox.Controls.OfType<TextBox>().ToList();
                List<ComboBox> comboBoxes = individualModeDataGroupBox.Controls.OfType<ComboBox>().ToList();
                for (int i = 0; i < 10; ++i)
                {
                    textBoxes[i].Text = "";
                    textBoxes[i].Enabled = false;
                    comboBoxes[i].SelectedIndex = 0;
                    comboBoxes[i].Enabled = false;
                }

                //Set the total number of images to be viewed in the label.
                batchTotalLabel.Text = plates.Count.ToString();
                totalImages = plates.Count;

                //Start backgroundWorkler to fill queues asynchronously.
                batchProcessor.RunWorkerAsync();

                //Wait until an image available is to view
                while (laserImages.Count < 1)
                    //Sleep 1 ms
                    Thread.Sleep(1);

                //Show the first images in the picture boxes
                showNextImages();
                GC.Collect();
            }
            else
            {
                //Tell the batchWorker to stop.
                if (batchProcessor.IsBusy)
                    batchProcessor.CancelAsync();

                //Remove all items from the queues
                while (laserImages.Count > 0)
                {
                    lock (laserImages)
                        laserImages.Dequeue();
                    lock (previewImages)
                        previewImages.Dequeue();
                }
                
                //Reset UI elements
                batchCurrentLabel.Text = "________";
                batchTotalLabel.Text = "________";
                batchProgressBar.Value = 0;
                batchQueueDepthProgressBar.Value = 0;
                totalImages = 0;
                imagesReviewed = 0;

                laserPictureBox.Image = null;
                previewPictureBox.Image = null;

                GC.Collect();
            }
        }

        //Locks the images queues and shows them in the picture boxes
        private void showNextImages()
        {
            //Change the number of images reviewed, update progress bar
            imagesReviewed++;
            batchCurrentLabel.Text = string.Format("{0}", imagesReviewed);
            if (imagesReviewed != totalImages &&
                batchProgressBar.Value != (int)(((double)imagesReviewed / totalImages) * 100.0) % batchProgressBar.Maximum
                )
            {
                batchProgressBar.Value = (int)(((double)imagesReviewed / totalImages) * 100.0) % batchProgressBar.Maximum;
            }
            else
            {
                if (batchProgressBar.Value != 100)
                    batchProgressBar.Value = 100;
            }
            

            //Update the images if there are any. If not, exit batch mode.
            if (laserImages.Count > 0 && previewImages.Count > 0)
            {
                lock (laserImages)
                    laserPictureBox.Image = laserImages.Dequeue();
                lock (previewImages)
                    previewPictureBox.Image = previewImages.Dequeue();
            }
            else
            {
                batchAbortButton.PerformClick();
            }
        }

        private void updateProgressBar(int progress)
        {
            if (batchQueueDepthProgressBar.Value != progress)
                batchQueueDepthProgressBar.Value = progress;
        }
        #endregion
        #endregion
    }
}
