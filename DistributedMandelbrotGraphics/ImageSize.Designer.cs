
namespace DistributedMandelbrotGraphics {
	partial class ImageSize {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.ImageSizeNumericWidth = new System.Windows.Forms.NumericUpDown();
			this.ImageSizeNumericHeight = new System.Windows.Forms.NumericUpDown();
			this.ImageSizeButtonOk = new System.Windows.Forms.Button();
			this.ImageSizeButtonCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.ImageSizeNumericWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ImageSizeNumericHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Width";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 43);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "Height";
			// 
			// ImageSizeNumericWidth
			// 
			this.ImageSizeNumericWidth.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.ImageSizeNumericWidth.Location = new System.Drawing.Point(80, 12);
			this.ImageSizeNumericWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.ImageSizeNumericWidth.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.ImageSizeNumericWidth.Name = "ImageSizeNumericWidth";
			this.ImageSizeNumericWidth.Size = new System.Drawing.Size(120, 23);
			this.ImageSizeNumericWidth.TabIndex = 2;
			this.ImageSizeNumericWidth.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// ImageSizeNumericHeight
			// 
			this.ImageSizeNumericHeight.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.ImageSizeNumericHeight.Location = new System.Drawing.Point(80, 41);
			this.ImageSizeNumericHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.ImageSizeNumericHeight.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.ImageSizeNumericHeight.Name = "ImageSizeNumericHeight";
			this.ImageSizeNumericHeight.Size = new System.Drawing.Size(120, 23);
			this.ImageSizeNumericHeight.TabIndex = 3;
			this.ImageSizeNumericHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// ImageSizeButtonOk
			// 
			this.ImageSizeButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImageSizeButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ImageSizeButtonOk.Location = new System.Drawing.Point(46, 77);
			this.ImageSizeButtonOk.Name = "ImageSizeButtonOk";
			this.ImageSizeButtonOk.Size = new System.Drawing.Size(75, 23);
			this.ImageSizeButtonOk.TabIndex = 4;
			this.ImageSizeButtonOk.Text = "OK";
			this.ImageSizeButtonOk.UseVisualStyleBackColor = true;
			// 
			// ImageSizeButtonCancel
			// 
			this.ImageSizeButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImageSizeButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ImageSizeButtonCancel.Location = new System.Drawing.Point(127, 77);
			this.ImageSizeButtonCancel.Name = "ImageSizeButtonCancel";
			this.ImageSizeButtonCancel.Size = new System.Drawing.Size(75, 23);
			this.ImageSizeButtonCancel.TabIndex = 5;
			this.ImageSizeButtonCancel.Text = "Cancel";
			this.ImageSizeButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ImageSize
			// 
			this.AcceptButton = this.ImageSizeButtonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ImageSizeButtonCancel;
			this.ClientSize = new System.Drawing.Size(214, 112);
			this.Controls.Add(this.ImageSizeButtonCancel);
			this.Controls.Add(this.ImageSizeButtonOk);
			this.Controls.Add(this.ImageSizeNumericHeight);
			this.Controls.Add(this.ImageSizeNumericWidth);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(230, 151);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(230, 151);
			this.Name = "ImageSize";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Image Size";
			((System.ComponentModel.ISupportInitialize)(this.ImageSizeNumericWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ImageSizeNumericHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button ImageSizeButtonOk;
		private System.Windows.Forms.Button ImageSizeButtonCancel;
		public System.Windows.Forms.NumericUpDown ImageSizeNumericWidth;
		public System.Windows.Forms.NumericUpDown ImageSizeNumericHeight;
	}
}