namespace WIG
{
    partial class PreviewForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.previewTabControl = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // previewTabControl
            // 
            this.previewTabControl.Location = new System.Drawing.Point(3, 3);
            this.previewTabControl.Name = "previewTabControl";
            this.previewTabControl.SelectedIndex = 0;
            this.previewTabControl.Size = new System.Drawing.Size(780, 528);
            this.previewTabControl.TabIndex = 0;
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 531);
            this.Controls.Add(this.previewTabControl);
            this.Name = "PreviewForm";
            this.Text = "PreviewForm";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TabControl previewTabControl;
    }
}