using System.Text;
using System.Text.RegularExpressions;

namespace Eml2Pdf;

public class EmailDecoder
{
	public static async Task<EmailMessage> ParseEmlAsync(string emlPath)
	{
		var emlContent = await File.ReadAllTextAsync(emlPath);
		var email = new EmailMessage();
		var lines = emlContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
		bool inHeaders = true;
		string headerBuffer = "";
		int i = 0;

		while (i < lines.Length && inHeaders)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line) && !string.IsNullOrWhiteSpace(headerBuffer))
			{
				inHeaders = false;
			}
			else
			{
				if (line.StartsWith(" ") || line.StartsWith("\t"))
				{
					headerBuffer += " " + line.Trim();
				}
				else
				{
					ParseHeader(email, headerBuffer);
					headerBuffer = line;
				}
			}
			i++;
		}

		ParseHeader(email, headerBuffer);

		var headerContentTypeMatch = Regex.Match(emlContent, @"Content-Type: ([^\r\n]+(?:\r\n[ \t][^\r\n]+)*)", RegexOptions.Singleline);
		string contentType = null;
		string boundary = null;
		if (headerContentTypeMatch.Success)
		{
			string fullContentType = headerContentTypeMatch.Groups[1].Value;
			var (type, parameters) = ParseContentType(fullContentType);
			contentType = type;
			boundary = parameters.ContainsKey("boundary") ? "--" + parameters["boundary"] : null;
		}

		string remainingContent = string.Join("\r\n", lines, i, lines.Length - i);

		if (contentType != null && contentType.StartsWith("multipart/") && boundary != null)
		{
			var parts = SplitMimeParts(remainingContent, boundary);
			foreach (var subPart in parts)
			{
				ProcessMimePart(email, subPart, boundary);
			}
		}
		else
		{
			ProcessSinglePart(email, remainingContent, contentType ?? "text/plain", new Dictionary<string, string>(), null);
		}

		return email;
	}

	private static void ParseHeader(EmailMessage email, string header)
	{
		if (string.IsNullOrWhiteSpace(header)) return;
		var parts = header.Split(new[] { ": " }, 2, StringSplitOptions.None);
		if (parts.Length < 2) 
			return;

		string name = parts[0];
		string value = DecodeMimeEncodedWord(parts[1]);


		if(name == "Date")
		{
			if (DateTime.TryParse(value, out DateTime dtm))
				value = dtm.ToLocalTime().ToString();
		}
		email.Headers.Add((name, value));
	}


	private static string DecodeMimeEncodedWord(string input)
	{
		if (string.IsNullOrEmpty(input)) return input;

		var regex = new Regex(@"\=\?([^?]+)\?([QB])\?([^?]+)\?\=", RegexOptions.IgnoreCase);
		return regex.Replace(input, match =>
		{
			string charset = match.Groups[1].Value;
			string encoding = match.Groups[2].Value.ToUpper();
			string encodedText = match.Groups[3].Value;

			try
			{
				Encoding enc = Encoding.GetEncoding(charset);
				if (encoding == "Q")
				{
					return DecodeQuotedPrintable(encodedText, enc);
				}
				else if (encoding == "B")
				{
					byte[] decodedBytes = Convert.FromBase64String(encodedText);
					return enc.GetString(decodedBytes);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to decode MIME encoded-word: {ex.Message}");
			}
			return encodedText; // Fallback: return as-is
		});
	}

	private static void ProcessMimePart(EmailMessage email, string part, string parentBoundary)
	{
		var contentTypeMatch = Regex.Match(part, @"Content-Type: ([^\r\n]+(?:\r\n[ \t][^\r\n]+)*)", RegexOptions.Singleline);
		if (!contentTypeMatch.Success)
		{
			ProcessSinglePart(email, part, "text/plain", new Dictionary<string, string>(), parentBoundary);
			return;
		}

		string fullContentType = contentTypeMatch.Groups[1].Value;
		var (contentType, parameters) = ParseContentType(fullContentType);

		string currentBoundary = parameters.ContainsKey("boundary") ? "--" + parameters["boundary"] : null;

		if (contentType.StartsWith("multipart/") && currentBoundary != null)
		{
			var parts = SplitMimeParts(part, currentBoundary);
			foreach (var subPart in parts)
			{
				ProcessMimePart(email, subPart, currentBoundary);
			}
		}
		else if (contentType.StartsWith("text/"))
		{
			ProcessSinglePart(email, part, contentType, parameters, parentBoundary);
		}
		else if (contentType.StartsWith("image/") || contentType.StartsWith("application/"))
		{
			ParseAttachment(email, part, contentType, parameters, parentBoundary);
		}
	}

	private static List<string> SplitMimeParts(string content, string boundary)
	{
		var parts = new List<string>();
		var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
		string currentPart = "";
		bool inPart = false;

		for (int i = 0; i < lines.Length; i++)
		{
			if (lines[i].StartsWith(boundary))
			{
				if (inPart && !string.IsNullOrWhiteSpace(currentPart))
				{
					parts.Add(currentPart);
				}
				currentPart = "";
				inPart = true;
				if (lines[i].EndsWith("--")) continue;
			}
			if (inPart)
			{
				currentPart += lines[i] + "\r\n";
			}
		}
		if (inPart && !string.IsNullOrWhiteSpace(currentPart))
		{
			parts.Add(currentPart);
		}

		return parts;
	}

	private static void ProcessSinglePart(EmailMessage email, string part, string contentType, Dictionary<string, string> parameters, string parentBoundary = null)
	{
		var mimePart = new MimePart2
		{
			ContentType = contentType,
			Charset = parameters.ContainsKey("charset") ? parameters["charset"] : "utf-8",
			TransferEncoding = Regex.Match(part, @"Content-Transfer-Encoding: ([^\r\n]+)").Groups[1].Value.ToLower()
		};

		mimePart.Content = ExtractContent(part, mimePart.TransferEncoding, mimePart.Charset);

		email.Parts.Add(mimePart);
	}

	private static string ExtractContent(string part, string transferEncoding, string charset)
	{
		var contentMatch = Regex.Match(part, @"(?:\r\n\r\n|\n\n)([\s\S]+)$");
		if (!contentMatch.Success) return "";

		string content = contentMatch.Groups[1].Value.Trim();
		var enc = Encoding.GetEncoding(charset);
		if (transferEncoding == "quoted-printable")
		{
			return DecodeQuotedPrintable(content, enc);
		}
		else if (transferEncoding == "base64")
		{
			byte[] decodedBytes = Convert.FromBase64String(Regex.Replace(content, @"[\r\n]", ""));
			return Encoding.GetEncoding(charset).GetString(decodedBytes);
		}
		return content;
	}

	private static void ParseAttachment(EmailMessage email, string part, string contentType, Dictionary<string, string> parameters, string parentBoundary)
	{
		var attachment = new Attachment();

		if (parameters.ContainsKey("name"))
			attachment.FileName = parameters["name"];

		attachment.ContentType = contentType;

		var contentIdMatch = Regex.Match(part, @"Content-ID: <([^>]+)>");
		if (contentIdMatch.Success)
			attachment.ContentId = contentIdMatch.Groups[1].Value;

		string closingBoundary = parentBoundary + "--";
		string pattern = $@"Content-Transfer-Encoding: base64\s*([\s\S]+?)(?=\s*$)";

		var contentMatch = Regex.Match(part, pattern, RegexOptions.Singleline);

		if (contentMatch.Success)
		{
			string content = contentMatch.Groups[1].Value;
			string base64 = Regex.Replace(content, @"[\r\n]", "");
			try
			{
				attachment.Data = Convert.FromBase64String(base64);
			}
			catch (FormatException ex)
			{
				Console.WriteLine($"Base64 decoding mislukt voor {attachment.FileName}: {ex.Message}");
			}
		}
		else
		{
			Console.WriteLine($"Geen inhoud gevonden voor attachment {attachment.FileName}.");
		}

		email.Attachments.Add(attachment);
	}

	static string DecodeQuotedPrintable(string input, Encoding enc)
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
				bytes.AddRange(enc.GetBytes(input[i].ToString())); // Unicode-ondersteuning
			}
		}
		return enc.GetString([.. bytes]);
	}

	static bool IsHexChar(char c) =>
		c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';

	private static (string contentType, Dictionary<string, string> parameters) ParseContentType(string fullContentType)
	{
		var parts = Regex.Split(fullContentType, @";(?=(?:[^""]*""[^""]*"")*[^""]*$)")
			.Select(p => p.Trim())
			.Where(p => !string.IsNullOrEmpty(p))
			.ToArray();

		if (parts.Length == 0)
			return ("text/plain", new Dictionary<string, string>());

		string contentType = parts[0].Trim();
		var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		for (int i = 1; i < parts.Length; i++)
		{
			var paramMatch = Regex.Match(parts[i], @"([^=]+)=""?([^""]+)""?");
			if (paramMatch.Success)
			{
				string key = paramMatch.Groups[1].Value.Trim();
				string value = paramMatch.Groups[2].Value.Trim();
				parameters[key] = value;
			}
		}

		return (contentType, parameters);
	}
}
