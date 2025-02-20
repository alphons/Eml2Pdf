using PdfSharp.Pdf;
using System.Net.Mail;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Eml2Pdf;

public class PdfHelper
{
	private static string HtmlEscape(string text) => text.Replace("<", "&lt;").Replace(">", "&gt;");
	public static void CreatePdf(EmailMessage email, string pdfPath)
	{

		var htmlContent = new StringBuilder();
		htmlContent.AppendLine("<html><head><meta charset='UTF-8'></head><body style='font-family: Arial, sans-serif;'>");

		htmlContent.AppendLine("<table border='1' style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>");
		string[] importantHeaders = ["From", "Subject", "To", "Date"];
		foreach (var header in importantHeaders)
		{
			htmlContent.AppendLine("<tr>");
			htmlContent.AppendLine($"<td style='padding: 5px;'>{header}</td>");
			htmlContent.AppendLine($"<td style='padding: 5px;'>{HtmlEscape(email.Headers.FirstOrDefault(h => h.Name == header).Value)}</td>");
			htmlContent.AppendLine("</tr>");
		}
		htmlContent.AppendLine("</table>");

		var hasHtmlPart = email.Parts.Any(p => p.ContentType.Contains("text/html"));

		foreach (var part in email.Parts)
		{
			if (hasHtmlPart && part.ContentType.Contains("text/plain"))
				continue;

			if (part.ContentType.Contains("text/html"))
			{
				//File.WriteAllText("debug.html", part.Content, Encoding.GetEncoding(part.Charset));
				htmlContent.AppendLine(part.Content);
			}
			else
			{
				htmlContent.AppendLine("<pre>");
				htmlContent.AppendLine(part.Content);
				htmlContent.AppendLine("</pre>");
			}
		}

		htmlContent.AppendLine("</body></html>");

		var dir = Path.Combine(AppContext.BaseDirectory, "tmp");

		if (email.Attachments.Count > 0)
		{
			Directory.CreateDirectory(dir);

			// save attachements temporary
			foreach (var attachment in email.Attachments)
			{
				var path = Path.Combine(dir, attachment.FileName);
				File.WriteAllBytes(path, attachment.Data);
				htmlContent.Replace($"cid:{attachment.ContentId}", $"file:///{path.Replace('\\','/')}");
			}
			File.WriteAllText("test.html", htmlContent.ToString());
		}


		PdfDocument pdf = PdfGenerator.GeneratePdf(htmlContent.ToString(), PdfSharp.PageSize.A4);

		pdf.Save(pdfPath);

		if (email.Attachments.Count > 0)
		{
			// clear attachements
			foreach (var attachment in email.Attachments)
			{
				var path = Path.Combine(dir, attachment.FileName);
				if(File.Exists(path))
					File.Delete(path);
			}
		}

	}


}
