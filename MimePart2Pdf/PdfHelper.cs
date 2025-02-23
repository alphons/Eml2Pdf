using Eml2MimePart;
using PdfSharp.Pdf;
using System.Globalization;
using System.IO;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace MimePart2Pdf;

public class PdfHelper
{
	private static string HtmlEscape(string text) => text.Replace("<", "&lt;").Replace(">", "&gt;");

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

	private static readonly CultureInfo Nederland = new ("nl-NL");


	public static void CreatePdf(MimePart email, string pdfPath)
	{
		bool UseTable = false;

		var html = GetHtml(email);

		if (string.IsNullOrWhiteSpace(html))
			return;

		var datum = email["Date"];

		if (DateTime.TryParse(datum, out DateTime dtm))
			datum = dtm.ToString("dddd dd MMMM yyyy HH:mm", Nederland);

		int index = html.IndexOf("<body");
		if (index != -1)
		{
			int eindIndex = html.IndexOf('>', index);
			if (eindIndex != -1)
			{
				var htmlHeader = new StringBuilder(Environment.NewLine);
				if (UseTable)
				{
					htmlHeader.AppendLine("<table border='1' style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>");
					string[] importantHeaders = ["From", "Subject", "To", "Date"];
					foreach (var header in importantHeaders)
					{
						htmlHeader.AppendLine("<tr>");
						htmlHeader.AppendLine($"<td style='padding: 5px; width:60px'>{header}</td>");
						htmlHeader.AppendLine($"<td style='padding: 5px;'>{HtmlEscape(email[header])}</td>");
						htmlHeader.AppendLine("</tr>");
					}
					htmlHeader.Append("</table>");
				}
				else
				{
					htmlHeader.AppendLine($"<div><b>Van:</b> {HtmlEscape(email["From"])}</div>");
					htmlHeader.AppendLine($"<div><b>Verzonden:</b> {datum}</div>");
					htmlHeader.AppendLine($"<div><b>Aan:</b> {HtmlEscape(email["To"])}</div>");
					var cc = email["CC"];
					if(!string.IsNullOrWhiteSpace(cc))
						htmlHeader.AppendLine($"<div><b>Cc:</b> {HtmlEscape(cc)}</div>");
					htmlHeader.AppendLine($"<div><b>Onderwerp:</b> {HtmlEscape(email["Subject"])}</div>");
					var priority = email["Priority"];
					if (!string.IsNullOrWhiteSpace(priority))
						htmlHeader.AppendLine($"<div><b>Prioriteit:</b> {HtmlEscape(priority)}</div>");
					var importance = email["Importance"];
					if (!string.IsNullOrWhiteSpace(importance))
						htmlHeader.AppendLine($"<div><b>Urgentie:</b> {HtmlEscape(importance)}</div>");
					htmlHeader.AppendLine("<div>&nbsp;</div>");
				}


				html = html.Insert(eindIndex + 1, htmlHeader.ToString());
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
		//File.WriteAllText("test.html", html);

		PdfDocument pdf = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);

		pdf.Save(pdfPath);

		//File.SetAttributes(pdfPath, FileAttributes.ReadOnly);
		File.SetLastWriteTimeUtc(pdfPath, dtm);
		File.SetCreationTime(pdfPath, dtm);


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
