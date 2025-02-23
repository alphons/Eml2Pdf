using Eml2MimePart;
using PdfSharp.Pdf;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Eml2Pdf2;

public class PdfHelper2
{
	private static string HtmlEscape(string text) => text.Replace("<", "&lt;").Replace(">", "&gt;");

	private static string GetHtml(MimePart email)
	{
		if (email["Content-Type"].Contains("text/html"))
			return email.TextContent;
		foreach(var part in email.Parts)
		{
			var html = GetHtml(part);
			if (!string.IsNullOrEmpty(html))
				return html;
		}
		return string.Empty;
	}

	public static void GetAttachements(List<MimePart> attachements, MimePart part)
	{
		var contentType = part["Content-Type"];
		if (!string.IsNullOrWhiteSpace(contentType) && !contentType.StartsWith("text/") && !contentType.StartsWith("multipart/"))
			attachements.Add(part);

		foreach (var subpart in part.Parts)
		{
			GetAttachements(attachements, subpart);
		}
	}

	public static void CreatePdf(MimePart email, string pdfPath)
	{
		var html = GetHtml(email);

		if (string.IsNullOrWhiteSpace(html))
			return;

		int index = html.IndexOf("<body");
		if (index != -1)
		{
			int eindIndex = html.IndexOf('>', index);
			if (eindIndex != -1)
			{
				var htmlTable = new StringBuilder(Environment.NewLine);
				htmlTable.AppendLine("<table border='1' style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>");
				string[] importantHeaders = ["From", "Subject", "To", "Date"];
				foreach (var header in importantHeaders)
				{
					htmlTable.AppendLine("<tr>");
					htmlTable.AppendLine($"<td style='padding: 5px; width:60px'>{header}</td>");
					htmlTable.AppendLine($"<td style='padding: 5px;'>{HtmlEscape(email[header])}</td>");
					htmlTable.AppendLine("</tr>");
				}
				htmlTable.Append("</table>");
				html = html.Insert(eindIndex + 1, htmlTable.ToString());
			}
		}

		var dir = Path.Combine(AppContext.BaseDirectory, "tmp");

		List<MimePart> attachements = [];
		GetAttachements(attachements, email);

		if (attachements.Count > 0)
		{
			Directory.CreateDirectory(dir);

			// save attachements temporary
			foreach (var attachment in attachements)
			{
				var path = Path.Combine(dir, attachment.FileName);

				File.WriteAllBytes(path, attachment.BinaryContent);

				html = html.Replace($"cid:{attachment.ContentId}", $"file:///{path.Replace('\\', '/')}");
			}
		}

		// DEBUGGING
		File.WriteAllText("test.html", html);

		PdfDocument pdf = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);

		pdf.Save(pdfPath);

		if (attachements.Count > 0)
		{
			// clear attachements
			foreach (var attachment in attachements)
			{
				var path = Path.Combine(dir, attachment.FileName);
				if (File.Exists(path))
					File.Delete(path);
			}
		}

	}


}
