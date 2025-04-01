namespace Eml2PdfWinApp
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
			txtInput = new TextBox();
			txtOutput = new TextBox();
			button1 = new Button();
			button2 = new Button();
			textBox3 = new TextBox();
			txtLog = new TextBox();
			button3 = new Button();
			txtHtml = new TextBox();
			SuspendLayout();
			// 
			// txtInput
			// 
			txtInput.Location = new Point(12, 12);
			txtInput.Name = "txtInput";
			txtInput.PlaceholderText = "input directory";
			txtInput.Size = new Size(269, 23);
			txtInput.TabIndex = 0;
			txtInput.Text = "input";
			// 
			// txtOutput
			// 
			txtOutput.Location = new Point(12, 57);
			txtOutput.Name = "txtOutput";
			txtOutput.PlaceholderText = "output directory";
			txtOutput.Size = new Size(269, 23);
			txtOutput.TabIndex = 1;
			txtOutput.Text = "output";
			// 
			// button1
			// 
			button1.Location = new Point(287, 142);
			button1.Name = "button1";
			button1.Size = new Size(125, 23);
			button1.TabIndex = 2;
			button1.Text = "Eml2Pdf";
			button1.UseVisualStyleBackColor = true;
			button1.Click += Button1_Click;
			// 
			// button2
			// 
			button2.Location = new Point(287, 35);
			button2.Name = "button2";
			button2.Size = new Size(125, 23);
			button2.TabIndex = 3;
			button2.Text = "Eml attachements";
			button2.UseVisualStyleBackColor = true;
			button2.Click += Button2_Click;
			// 
			// textBox3
			// 
			textBox3.Location = new Point(12, 143);
			textBox3.Name = "textBox3";
			textBox3.PlaceholderText = "pdf directory";
			textBox3.Size = new Size(269, 23);
			textBox3.TabIndex = 4;
			textBox3.Text = "pdf";
			// 
			// txtLog
			// 
			txtLog.AcceptsReturn = true;
			txtLog.AcceptsTab = true;
			txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			txtLog.Location = new Point(12, 172);
			txtLog.Multiline = true;
			txtLog.Name = "txtLog";
			txtLog.ReadOnly = true;
			txtLog.ScrollBars = ScrollBars.Both;
			txtLog.Size = new Size(508, 251);
			txtLog.TabIndex = 5;
			// 
			// button3
			// 
			button3.Location = new Point(287, 95);
			button3.Name = "button3";
			button3.Size = new Size(125, 23);
			button3.TabIndex = 6;
			button3.Text = "Eml2Html";
			button3.UseVisualStyleBackColor = true;
			button3.Click += ButtonEml2Html_Click;
			// 
			// txtHtml
			// 
			txtHtml.Location = new Point(12, 95);
			txtHtml.Name = "txtHtml";
			txtHtml.PlaceholderText = "output directory";
			txtHtml.Size = new Size(269, 23);
			txtHtml.TabIndex = 7;
			txtHtml.Text = "html";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(532, 435);
			Controls.Add(txtHtml);
			Controls.Add(button3);
			Controls.Add(txtLog);
			Controls.Add(textBox3);
			Controls.Add(button2);
			Controls.Add(button1);
			Controls.Add(txtOutput);
			Controls.Add(txtInput);
			Name = "Form1";
			Text = "Eml workbench - Annet Zwanenburg";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TextBox txtInput;
		private TextBox txtOutput;
		private Button button1;
		private Button button2;
		private TextBox textBox3;
		private TextBox txtLog;
		private Button button3;
		private TextBox txtHtml;
	}
}
