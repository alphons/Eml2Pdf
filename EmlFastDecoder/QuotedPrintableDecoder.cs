using System.Text;

namespace EmlFastDecoder;

public static class QuotedPrintableDecoder
{
	public static string Decode(string input, string charSet = "UTF-8")
	{
		if (string.IsNullOrEmpty(input))
			return input;

		Encoding encoding = Encoding.GetEncoding(charSet);
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
}
