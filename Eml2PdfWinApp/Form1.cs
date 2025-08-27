using Eml2MimePart;
using MimePart2Pdf;
using System.Diagnostics;
using System.IO;
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
					catch (Exception ex)
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
			await SaveMailAttachementsAsync(log, OutputDir, subpart);
		}
	}

	static async Task SaveHtmlMailBodyAsync(TextBox log, string OutputDir, MimePart part)
	{

		if (DateTime.TryParse(part["Date"], out DateTime dtm))
		{
			var outputPath = Path.Combine(OutputDir, $"{dtm:yyyyMMdd-HHmmss}.eml");

			try
			{
				await part.SaveAsync(outputPath, dtm);
			}
			catch (Exception ex)
			{
				log.Invoke((MethodInvoker)delegate
				{
					log.Text = $"{outputPath} {ex.Message}";
				});
			}
		}
	}





	private async void Button2_Click(object sender, EventArgs e)
	{
		this.button2.Enabled = false;

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		foreach (var emlPath in Directory.GetFiles(this.txtInput.Text, "*.eml"))
		{
			var email = await MimePart.ReadEmlAsync(emlPath);

			//await SaveHtmlMailBodyAsync(this.txtLog, this.txtOutput.Text, email);
			await SaveMailAttachementsAsync(this.txtLog, this.txtOutput.Text, email);
		}

		this.button2.Enabled = true;
	}

	private static string GetHtml(MimePart email)
	{
		if (email["Content-Type"].Contains("text/html"))
			return email.TextContent;
		foreach (var part in email.Parts)
		{
			var html = GetHtml(part);
			if (!string.IsNullOrEmpty(html))
				return html;
		}
		return string.Empty;
	}


	private async void ButtonEml2Html_Click(object sender, EventArgs e)
	{
		this.button3.Enabled = false;

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		var htmlDir = this.txtHtml.Text;

		foreach (var emlPath in Directory.GetFiles(this.txtOutput.Text, "*.eml"))
		{
			var email = await MimePart.ReadEmlAsync(emlPath);

			var html = GetHtml(email);

			if (string.IsNullOrWhiteSpace(html))
				return;

			var name = Path.GetFileNameWithoutExtension(emlPath);

			var dtm = DateTime.ParseExact(name, "yyyyMMdd-HHmmss", null);

			var output = Path.Combine(htmlDir, name + ".html");

			try
			{
				await File.WriteAllTextAsync(output, html);

				//File.SetAttributes(output, FileAttributes.ReadOnly);
				File.SetLastWriteTimeUtc(output, dtm);
				File.SetCreationTime(output, dtm);

			}
			catch (Exception eee)
			{
				this.txtLog.Invoke((MethodInvoker)delegate
				{
					this.txtLog.Text = $"{name} {eee.Message}";
				});
			}
		}

		this.button3.Enabled = true;
	}
}
