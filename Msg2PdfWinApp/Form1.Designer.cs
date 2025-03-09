namespace Msg2PdfWinApp
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			txtLog = new TextBox();
			txtPdf = new TextBox();
			button1 = new Button();
			txtInput = new TextBox();
			SuspendLayout();
			// 
			// txtLog
			// 
			txtLog.AcceptsReturn = true;
			txtLog.AcceptsTab = true;
			txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			txtLog.Location = new Point(12, 107);
			txtLog.Multiline = true;
			txtLog.Name = "txtLog";
			txtLog.ReadOnly = true;
			txtLog.ScrollBars = ScrollBars.Both;
			txtLog.Size = new Size(411, 222);
			txtLog.TabIndex = 11;
			// 
			// txtPdf
			// 
			txtPdf.Location = new Point(12, 69);
			txtPdf.Name = "txtPdf";
			txtPdf.PlaceholderText = "pdf directory";
			txtPdf.Size = new Size(269, 23);
			txtPdf.TabIndex = 10;
			txtPdf.Text = "pdf";
			// 
			// button1
			// 
			button1.Location = new Point(291, 42);
			button1.Name = "button1";
			button1.Size = new Size(125, 23);
			button1.TabIndex = 8;
			button1.Text = "Msg2Pdf";
			button1.UseVisualStyleBackColor = true;
			button1.Click += Button_Click;
			// 
			// txtInput
			// 
			txtInput.Location = new Point(12, 12);
			txtInput.Name = "txtInput";
			txtInput.PlaceholderText = "input directory";
			txtInput.Size = new Size(269, 23);
			txtInput.TabIndex = 6;
			txtInput.Text = "input";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(435, 341);
			Controls.Add(txtLog);
			Controls.Add(txtPdf);
			Controls.Add(button1);
			Controls.Add(txtInput);
			Name = "Form1";
			Text = "Outlook msg to pdf";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TextBox txtLog;
		private TextBox txtPdf;
		private Button button1;
		private TextBox txtInput;
	}
}
