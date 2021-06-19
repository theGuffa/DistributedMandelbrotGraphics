
namespace DistributedMandelbrotGraphics {
	partial class About {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.AboutButtonOk = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(158, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(237, 21);
			this.label1.TabIndex = 0;
			this.label1.Text = "Distributed Mandelbrot Graphics";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(240, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "Version 0.9.1";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(186, 77);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(181, 15);
			this.label3.TabIndex = 2;
			this.label3.Text = "Copyright 2021 Göran Andersson";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(13, 118);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(527, 83);
			this.label4.TabIndex = 3;
			this.label4.Text = resources.GetString("label4.Text");
			// 
			// AboutButtonOk
			// 
			this.AboutButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.AboutButtonOk.Location = new System.Drawing.Point(239, 204);
			this.AboutButtonOk.Name = "AboutButtonOk";
			this.AboutButtonOk.Size = new System.Drawing.Size(75, 23);
			this.AboutButtonOk.TabIndex = 4;
			this.AboutButtonOk.Text = "OK";
			this.AboutButtonOk.UseVisualStyleBackColor = true;
			// 
			// About
			// 
			this.AcceptButton = this.AboutButtonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(551, 236);
			this.Controls.Add(this.AboutButtonOk);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(567, 275);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(567, 275);
			this.Name = "About";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "About Distributed Mandelbrot Graphics";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button AboutButtonOk;
	}
}