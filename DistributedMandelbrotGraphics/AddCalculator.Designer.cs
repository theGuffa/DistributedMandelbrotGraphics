
namespace DistributedMandelbrotGraphics {
	partial class AddCalculator {
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
			this.AddCalculatorTextBoxIP = new System.Windows.Forms.TextBox();
			this.AddCalculatorTextBoxPort = new System.Windows.Forms.TextBox();
			this.AddCalculatorButtonOk = new System.Windows.Forms.Button();
			this.AddCalculatorButtonCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "IP address";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "Port";
			// 
			// AddCalculatorTextBoxIP
			// 
			this.AddCalculatorTextBoxIP.Location = new System.Drawing.Point(96, 12);
			this.AddCalculatorTextBoxIP.Name = "AddCalculatorTextBoxIP";
			this.AddCalculatorTextBoxIP.Size = new System.Drawing.Size(100, 23);
			this.AddCalculatorTextBoxIP.TabIndex = 2;
			// 
			// AddCalculatorTextBoxPort
			// 
			this.AddCalculatorTextBoxPort.Location = new System.Drawing.Point(96, 41);
			this.AddCalculatorTextBoxPort.Name = "AddCalculatorTextBoxPort";
			this.AddCalculatorTextBoxPort.Size = new System.Drawing.Size(100, 23);
			this.AddCalculatorTextBoxPort.TabIndex = 3;
			this.AddCalculatorTextBoxPort.Text = "33000";
			// 
			// AddCalculatorButtonOk
			// 
			this.AddCalculatorButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddCalculatorButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.AddCalculatorButtonOk.Location = new System.Drawing.Point(41, 76);
			this.AddCalculatorButtonOk.Name = "AddCalculatorButtonOk";
			this.AddCalculatorButtonOk.Size = new System.Drawing.Size(75, 23);
			this.AddCalculatorButtonOk.TabIndex = 4;
			this.AddCalculatorButtonOk.Text = "OK";
			this.AddCalculatorButtonOk.UseVisualStyleBackColor = true;
			// 
			// AddCalculatorButtonCancel
			// 
			this.AddCalculatorButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddCalculatorButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.AddCalculatorButtonCancel.Location = new System.Drawing.Point(122, 76);
			this.AddCalculatorButtonCancel.Name = "AddCalculatorButtonCancel";
			this.AddCalculatorButtonCancel.Size = new System.Drawing.Size(75, 23);
			this.AddCalculatorButtonCancel.TabIndex = 5;
			this.AddCalculatorButtonCancel.Text = "Cancel";
			this.AddCalculatorButtonCancel.UseVisualStyleBackColor = true;
			// 
			// AddCalculator
			// 
			this.AcceptButton = this.AddCalculatorButtonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.AddCalculatorButtonCancel;
			this.ClientSize = new System.Drawing.Size(209, 111);
			this.Controls.Add(this.AddCalculatorButtonCancel);
			this.Controls.Add(this.AddCalculatorButtonOk);
			this.Controls.Add(this.AddCalculatorTextBoxPort);
			this.Controls.Add(this.AddCalculatorTextBoxIP);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(225, 150);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(225, 150);
			this.Name = "AddCalculator";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Add Calculator";
			this.Load += new System.EventHandler(this.AddCalculator_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button AddCalculatorButtonOk;
		private System.Windows.Forms.Button AddCalculatorButtonCancel;
		public System.Windows.Forms.TextBox AddCalculatorTextBoxIP;
		public System.Windows.Forms.TextBox AddCalculatorTextBoxPort;
	}
}