using Eml2MimePart;
using MimePart2Pdf;
using System.Diagnostics;
using System.Text;

namespace Eml2PdfWinApp;

public partial class Form1 : Form
{
	public Form1()
	{
		InitializeComponent();
	}

	private async void Button1_Click(object sender, EventArgs e)
	{
		this.button1.Enabled = false;
		this.txtInput.Enabled = false;
		this.txtOutput.Enabled = false;

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		foreach (var emlPath in Directory.GetFiles(this.txtOutput.Text, "*.eml"))
		{
			var name = Path.GetFileNameWithoutExtension(emlPath);

			var pdfPath = Path.Combine(this.textBox3.Text, $"{name}.pdf");

			var email = await MimePart.ReadEmlAsync(emlPath);

			PdfHelper.CreatePdf(email, pdfPath);

		}

		this.txtInput.Enabled = true;
		this.txtOutput.Enabled = true;
		this.button1.Enabled = true;
	}

	static async Task SaveMailAttachementsAsync(TextBox log, string OutputDir, MimePart part)
	{
		var contentType = part["Content-Type"];

		if (part["Content-Disposition"].Contains("attachment"))
		{
			if (contentType.Contains("message/rfc822"))
			{
				if (DateTime.TryParse(part.Parts[0]["Date"], out DateTime dtm))
				{
					var outputPath = Path.Combine(OutputDir, $"{dtm:yyyyMMdd-HHmmss}.eml");

					try
					{
						await part.SaveAsync(outputPath, dtm);
					}
					catch(Exception ex)
					{
						log.Invoke((MethodInvoker)delegate
						{
							log.Text = $"{outputPath} {ex.Message}"; 
						});
					}
				}
				else
				{
					Debug.WriteLine("No Date found in message/rfc822 attachement");
				}
			}
		}

		foreach (var subpart in part.Parts)
		{
			await SaveMailAttachementsAsync(log,OutputDir, subpart);
		}
	}


	private async void Button2_Click(object sender, EventArgs e)
	{
		this.button2.Enabled = false;

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		foreach (var emlPath in Directory.GetFiles(this.txtInput.Text, "*.eml"))
		{
			var email = await MimePart.ReadEmlAsync(emlPath);

			await SaveMailAttachementsAsync(this.txtLog, this.txtOutput.Text, email);
		}

		this.button2.Enabled = true;
	}
}
