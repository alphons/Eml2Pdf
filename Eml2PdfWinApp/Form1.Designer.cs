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
			textBox1 = new TextBox();
			textBox2 = new TextBox();
			button1 = new Button();
			button2 = new Button();
			SuspendLayout();
			// 
			// textBox1
			// 
			textBox1.Location = new Point(35, 37);
			textBox1.Name = "textBox1";
			textBox1.PlaceholderText = "input directory";
			textBox1.Size = new Size(269, 23);
			textBox1.TabIndex = 0;
			textBox1.Text = "input";
			// 
			// textBox2
			// 
			textBox2.Location = new Point(35, 80);
			textBox2.Name = "textBox2";
			textBox2.PlaceholderText = "output directory";
			textBox2.Size = new Size(269, 23);
			textBox2.TabIndex = 1;
			textBox2.Text = "output";
			// 
			// button1
			// 
			button1.Location = new Point(336, 37);
			button1.Name = "button1";
			button1.Size = new Size(75, 23);
			button1.TabIndex = 2;
			button1.Text = "Eml2Pdf";
			button1.UseVisualStyleBackColor = true;
			button1.Click += Button1_Click;
			// 
			// button2
			// 
			button2.Location = new Point(451, 37);
			button2.Name = "button2";
			button2.Size = new Size(125, 23);
			button2.TabIndex = 3;
			button2.Text = "Eml attachements";
			button2.UseVisualStyleBackColor = true;
			button2.Click += Button2_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(896, 144);
			Controls.Add(button2);
			Controls.Add(button1);
			Controls.Add(textBox2);
			Controls.Add(textBox1);
			Name = "Form1";
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TextBox textBox1;
		private TextBox textBox2;
		private Button button1;
		private Button button2;
	}
}
