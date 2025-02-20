using PdfSharp.Pdf;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Eml2Pdf
{
	public class Helper
	{
		public class EmailContent(Dictionary<string, string> headers, List<EmailPart> parts)
		{
			public Dictionary<string, string> Headers { get; set; } = headers;
			public List<EmailPart> Parts { get; set; } = parts;
		}

		public record EmailPart(string ContentType, string Content, string Charset, string? Filename = null, string? TransferEncoding = null);

		public static EmailContent ParseMultipartEml(string filePath)
		{
			var headers = new Dictionary<string, string>();
			var parts = new List<EmailPart>();
			var currentPartContent = new StringBuilder();
			string? boundary = null;
			bool inBody = false;
			string? partContentType = null;
			string partCharset = "utf-8";
			string? partTransferEncoding = null;
			string? partFilename = null;

			using var reader = new StreamReader(filePath, Encoding.UTF8);
			while (reader.ReadLine() is { } line)
			{
				if (!inBody)
				{
					if (string.IsNullOrWhiteSpace(line))
					{
						inBody = true;
						continue;
					}

					if (line.StartsWith("Content-Type: multipart"))
					{
						int boundaryIndex = line.IndexOf("boundary=");
						if (boundaryIndex < 0)
						{
							line = reader.ReadLine();
							if (line == null)
								break;
							boundaryIndex = line.IndexOf("boundary=");
						}
						if (boundaryIndex > -1)
							boundary = "--" + line[(boundaryIndex + 9)..].Trim('"');
					}
					else if (line.Contains(':'))
					{
						var split = line.Split([':'], 2);
						string key = split[0].Trim();
						string value = split[1].Trim();
						string[] importantHeaders = ["From", "Subject", "To", "Date"];
						if (importantHeaders.Contains(key))
							headers[key] = DecodeRfc2047(value);
					}
				}
				else
				{
					if (boundary != null && line.StartsWith(boundary))
					{
						if (partContentType != null && currentPartContent.Length > 0)
						{
							var newPart = new EmailPart(
								partContentType,
								currentPartContent.ToString().Trim(),
								partCharset,
								partFilename,
								partTransferEncoding
							);
							parts.Add(newPart);
							partContentType = null;
							partCharset = "utf-8";
						}

						if (line.EndsWith("--"))
							break;

						partTransferEncoding = null;
						partFilename = null;
						currentPartContent.Clear();
						continue;
					}

					if (partContentType != null)
					{
						if (line.StartsWith("Content-Type:"))
						{
							partContentType = line[13..].Trim();
							var charSetLine = partContentType;
							if (!charSetLine.Contains("charset="))
								charSetLine = reader.ReadLine();
							if (charSetLine == null)
								break;
							if (charSetLine.Contains("charset="))
							{
								int charsetIndex = charSetLine.IndexOf("charset=");
								partCharset = charSetLine[(charsetIndex + 8)..].Split(';')[0].Trim('"');
							}
						}
						else if (line.StartsWith("Content-Transfer-Encoding:"))
							partTransferEncoding = line[26..].Trim();
						else if (line.StartsWith("Content-Disposition:") && line.Contains("filename="))
						{
							int fnIndex = line.IndexOf("filename=");
							partFilename = line[(fnIndex + 9)..].Trim('"');
						}
						else if (!string.IsNullOrWhiteSpace(line) || currentPartContent.Length > 0)
							currentPartContent.AppendLine(line);
					}
					else if (line.StartsWith("Content-Type:"))
					{
						partContentType = line[13..].Trim();
						var charSetLine = partContentType;
						if (!charSetLine.Contains("charset="))
							charSetLine = reader.ReadLine();
						if (charSetLine == null)
							break;
						if (charSetLine.Contains("charset="))
						{
							int charsetIndex = charSetLine.IndexOf("charset=");
							partCharset = charSetLine[(charsetIndex + 8)..].Split(';')[0].Trim('"');
						}
					}
					else if (!string.IsNullOrWhiteSpace(line))
					{
						currentPartContent.AppendLine(line);
					}
				}
			}

			if (partContentType != null && currentPartContent.Length > 0)
			{
				var newPart = new EmailPart(
					partContentType,
					currentPartContent.ToString().Trim(),
					partCharset,
					partFilename,
					partTransferEncoding
				);
				parts.Add(newPart);
			}
			else if (currentPartContent.Length > 0 && parts.Count == 0)
			{
				var newPart = new EmailPart("text/plain", currentPartContent.ToString().Trim(), "utf-8", TransferEncoding: "7bit");
				parts.Add(newPart);
			}

			return new EmailContent(headers, parts);
		}

		public static void CreatePdf(EmailContent email, string pdfPath)
		{
			var htmlContent = new StringBuilder();
			htmlContent.AppendLine("<html><head><meta charset='UTF-8'></head><body style='font-family: Arial, sans-serif;'>");

			htmlContent.AppendLine("<table border='1' style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>");
			string[] importantHeaders = ["From", "Subject", "To", "Date"];
			foreach (var header in importantHeaders)
			{
				htmlContent.AppendLine("<tr>");
				htmlContent.AppendLine($"<td style='padding: 5px;'>{header}</td>");
				htmlContent.AppendLine($"<td style='padding: 5px;'>{email.Headers.GetValueOrDefault(header, "N/A")}</td>");
				htmlContent.AppendLine("</tr>");
			}
			htmlContent.AppendLine("</table>");

			bool hasHtmlPart = email.Parts.Any(p => p.ContentType.Contains("text/html"));
			foreach (var part in email.Parts)
			{
				if (hasHtmlPart && part.ContentType.Contains("text/plain"))
					continue;

				if (part.Filename != null)
					htmlContent.AppendLine($"<p><strong>Attachment:</strong> {part.Filename}</p>");

				string decodedContent = part.Content;
				if (part.TransferEncoding?.ToLower() == "quoted-printable")
				{
					decodedContent = DecodeQuotedPrintable(part.Content);
				}

				if (part.ContentType.Contains("text/html"))
					htmlContent.AppendLine(decodedContent);
				else
				{
					htmlContent.AppendLine("<pre>");
					htmlContent.AppendLine(decodedContent);
					htmlContent.AppendLine("</pre>");
				}
			}

			htmlContent.AppendLine("</body></html>");

			PdfDocument pdf = PdfGenerator.GeneratePdf(htmlContent.ToString(), PdfSharp.PageSize.A4);
			pdf.Save(pdfPath);
		}

		static string DecodeRfc2047(string input)
		{
			if (string.IsNullOrEmpty(input) || !input.StartsWith("=?"))
				return input;

			try
			{
				int start = input.IndexOf("=?");
				int end = input.LastIndexOf("?=");
				if (start < 0 || end <= start)
					return input;

				string encoded = input[(start + 2)..end];
				string[] parts = encoded.Split('?');
				if (parts.Length != 3)
					return input;

				string charset = parts[0];
				string encoding = parts[1];
				string value = parts[2];

				Encoding enc = Encoding.GetEncoding(charset);
				return encoding.ToUpper() switch
				{
					"Q" => DecodeQuotedPrintable(value),
					"B" => enc.GetString(Convert.FromBase64String(value)),
					_ => input
				};
			}
			catch
			{
				return input;
			}
		}

		static string DecodeQuotedPrintable(string input)
		{
			var bytes = new List<byte>();
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] == '=' && i + 2 < input.Length &&
					IsHexChar(input[i + 1]) && IsHexChar(input[i + 2]))
				{
					string hex = input.Substring(i + 1, 2);
					bytes.Add(Convert.ToByte(hex, 16));
					i += 2;
				}
				else if (input[i] == '=')
				{
					while (i + 1 < input.Length && (input[i + 1] == '\r' || input[i + 1] == '\n'))
						i++;
				}
				else
				{
					bytes.Add((byte)input[i]);
				}
			}
			return Encoding.UTF8.GetString([.. bytes]);
		}

		static bool IsHexChar(char c) =>
			c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';
	}
}
