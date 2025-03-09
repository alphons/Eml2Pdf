using Eml2MimePart;
using MimePart2Pdf;
using MsgReader.Outlook;
using System.Text;

namespace Msg2PdfWinApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, EventArgs e)
		{
			this.button1.Enabled = false;
			this.txtInput.Enabled = false;

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			foreach (var msgFilePath in Directory.GetFiles(this.txtInput.Text, "*.msg"))
			{
				var name = Path.GetFileNameWithoutExtension(msgFilePath);

				var pdfPath = Path.Combine(this.txtPdf.Text, $"{name}.pdf");

				PdfHelper.CreatePdf(msgFilePath, pdfPath, false);
			}

			this.txtInput.Enabled = true;
			this.button1.Enabled = true;
		}
	}
}
