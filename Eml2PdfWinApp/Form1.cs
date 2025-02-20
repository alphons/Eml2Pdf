using Eml2Pdf;
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

			foreach (var emlPath in Directory.GetFiles(this.textBox1.Text, "*.eml"))
			{
				var name = Path.GetFileNameWithoutExtension(emlPath);

				var pdfPath = Path.Combine(this.textBox2.Text, $"{name}.pdf");

				var email = await EmailDecoder.ParseEmlAsync(emlPath);

				PdfHelper.CreatePdf(email, pdfPath);
			}

			this.textBox1.Enabled = true;
			this.textBox2.Enabled = true;
			this.button1.Enabled = true;
		}
	}
}
