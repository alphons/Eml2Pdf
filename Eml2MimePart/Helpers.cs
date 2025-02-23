using System.Text;
using System.Text.RegularExpressions;

namespace Eml2MimePart;

public class Helpers
{
	public static string QuotedPrintableDecode(string input, Encoding encoding)
	{
		if (string.IsNullOrEmpty(input))
			return input;

		var byteList = new List<byte>();
		int i = 0;

		while (i < input.Length)
		{
			if (input[i] == '=' && i + 1 < input.Length)
			{
				if (input[i + 1] == '\r' || input[i + 1] == '\n')
				{
					i++;
					while (i < input.Length && (input[i] == '\r' || input[i] == '\n'))
						i++;
					continue;
				}
				if (i + 2 < input.Length && IsHexDigit(input[i + 1]) && IsHexDigit(input[i + 2]))
				{
					string hex = input.Substring(i + 1, 2);
					byte b = Convert.ToByte(hex, 16);
					byteList.Add(b);
					i += 3;
				}
				else
				{
					byteList.AddRange(encoding.GetBytes("="));
					i++;
				}
			}
			else
			{
				byteList.AddRange(encoding.GetBytes(input[i].ToString()));
				i++;
			}
		}

		return encoding.GetString(byteList.ToArray());
	}

	private static bool IsHexDigit(char c)
	{
		return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
	}

	public static string DecodeMimeEncodedWord(string input)
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
					return QuotedPrintableDecode(encodedText, enc);
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
}
