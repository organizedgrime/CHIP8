namespace CHIP8
{
    partial class Display
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
            this.displayGrid = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.displayGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // displayGrid
            // 
            this.displayGrid.Location = new System.Drawing.Point(0, 0);
            this.displayGrid.Name = "displayGrid";
            this.displayGrid.Size = new System.Drawing.Size(640, 320);
            this.displayGrid.TabIndex = 0;
            this.displayGrid.TabStop = false;
            // 
            // Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 361);
            this.Controls.Add(this.displayGrid);
            this.Name = "Display";
            this.Text = "Display";
            ((System.ComponentModel.ISupportInitialize)(this.displayGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox displayGrid;
    }
}