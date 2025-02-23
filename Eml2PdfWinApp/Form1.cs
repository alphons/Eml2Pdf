using Eml2MimePart;
using MimePart2Pdf;
using System.Diagnostics;
using System.Text;

namespace Eml2PdfWinApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private async void Button1_Click(object sender, EventArgs e)
		{
			this.button1.Enabled = false;
			this.textBox1.Enabled = false;
			this.textBox2.Enabled = false;

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			foreach (var emlPath in Directory.GetFiles(this.textBox2.Text, "*.eml"))
			{
				var name = Path.GetFileNameWithoutExtension(emlPath);

				var pdfPath = Path.Combine(this.textBox3.Text, $"{name}.pdf");

				var email = await MimePart.ReadEmlAsync(emlPath);

				PdfHelper.CreatePdf(email, pdfPath);

			}

			this.textBox1.Enabled = true;
			this.textBox2.Enabled = true;
			this.button1.Enabled = true;
		}

		static async Task SaveMailAttachementsAsync(string OutputDir, MimePart part)
		{
			var contentType = part["Content-Type"];

			if (part["Content-Disposition"].Contains("attachment"))
			{
				if (contentType.Contains("message/rfc822"))
				{
					if (DateTime.TryParse(part.Parts[0]["Date"], out DateTime dtm))
					{
						var outputPath = Path.Combine(OutputDir, $"{dtm:yyyyMMdd-HHmmss}.eml");

						await part.SaveAsync(outputPath, dtm);
					}
					else
					{
						Debug.WriteLine("No Date found in message/rfc822 attachement");
					}
				}
			}

			foreach (var subpart in part.Parts)
			{
				await SaveMailAttachementsAsync(OutputDir, subpart);
			}
		}


		private async void Button2_Click(object sender, EventArgs e)
		{
			this.button2.Enabled = false;

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			foreach (var emlPath in Directory.GetFiles(this.textBox1.Text, "*.eml"))
			{
				var email = await MimePart.ReadEmlAsync(emlPath);

				await SaveMailAttachementsAsync(this.textBox2.Text, email);
			}

			this.button2.Enabled = true;
		}
	}
}
