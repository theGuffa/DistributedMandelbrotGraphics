
namespace DistributedMandelbrotGraphics {
	partial class QuickGuide {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickGuide));
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.QuickGuideButtonOk = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBox1.Location = new System.Drawing.Point(12, 12);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(334, 466);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
			// 
			// QuickGuideButtonOk
			// 
			this.QuickGuideButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.QuickGuideButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.QuickGuideButtonOk.Location = new System.Drawing.Point(271, 488);
			this.QuickGuideButtonOk.Name = "QuickGuideButtonOk";
			this.QuickGuideButtonOk.Size = new System.Drawing.Size(75, 23);
			this.QuickGuideButtonOk.TabIndex = 1;
			this.QuickGuideButtonOk.Text = "OK";
			this.QuickGuideButtonOk.UseVisualStyleBackColor = true;
			// 
			// QuickGuide
			// 
			this.AcceptButton = this.QuickGuideButtonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(358, 523);
			this.Controls.Add(this.QuickGuideButtonOk);
			this.Controls.Add(this.richTextBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "QuickGuide";
			this.ShowIcon = false;
			this.Text = "Quick Guide";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button QuickGuideButtonOk;
	}
}