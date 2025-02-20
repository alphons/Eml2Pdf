using Eml2PdfHelper;
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
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			foreach (var emlPath in Directory.GetFiles(this.textBox1.Text, "*.eml"))
			{
				var name = Path.GetFileNameWithoutExtension(emlPath);

				var pdfPath = Path.Combine(this.textBox2.Text, name);

				var email = await Helper.ParseMultipartEmlAsync(emlPath);

				Helper.CreatePdf(email, pdfPath);
			}

			Console.WriteLine("Conversie voltooid!");
		}
	}
}
