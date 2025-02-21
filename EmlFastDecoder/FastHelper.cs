using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace EmlFastDecoder;


public class FastHelper
{
	private static string[] lines = [];

	private static Regex IsValue = new(@"^\W");

	private static readonly Regex NV = new(@"([^\s;=]+)=([^;]*?)(?=\s*?(?:;|$))");


	public class EmailMessage
	{
		public Header Header { get; set; }
		public List<MimePart> Parts { get; set; } = [];
		public List<Attachment> Attachments { get; set; } = new List<Attachment>();

		// Optionele convenience properties voor veelgebruikte headers
		public string From => Header.List.FirstOrDefault(h => h.Name.ToLower() == "from").Value;
		public string To => Header.List.FirstOrDefault(h => h.Name.ToLower() == "to").Value;
		public string Subject => Header.List.FirstOrDefault(h => h.Name.ToLower() == "subject").Value;
		public DateTime? Date => DateTime.TryParse(Header.List.FirstOrDefault(h => h.Name.ToLower() == "date").Value, out DateTime date) ? date : (DateTime?)null;
		public string MessageId => Header.List.FirstOrDefault(h => h.Name.ToLower() == "message-id").Value;
		public string ContentType => Header.List.FirstOrDefault(h => h.Name.ToLower() == "content-type").Value;
	}



	public class MimePart
	{
		public string ContentType { get; set; }
		public string Charset { get; set; }
		public string Content { get; set; }
		public string TransferEncoding { get; set; }
	}

	public class Attachment
	{
		public string FileName { get; set; }
		public string ContentType { get; set; }
		public string ContentId { get; set; }
		public byte[] Data { get; set; }
	}

	public class HeaderValue(string Name, string value)
	{
		public string Name { get; } = Name;

		private readonly string value = value;
		public string Value
		{
			get
			{
				var i = value.IndexOf(';');
				if (i > 0)
					return value[..i];
				return value;
			}
		}

		private List<(string Name, string Value)>? properties;
		public List<(string Name, string Value)> Properties
		{
			get
			{
				if (properties == null)
				{
					properties = [];
					if (value != null)
					{
						foreach (Match match in NV.Matches(value))
						{
							string name = match.Groups[1].Value;
							string value = match.Groups[2].Value.Trim(); // Trim voor nette waarden
							properties.Add((name, value));
						}
					}
				}
				return properties;
			}
		}
	}

	public class Header
	{
		public int Start { get; set; } = -1;
		public int Stop { get; set; } = -1;

		private List<HeaderValue>? list;

		public List<HeaderValue> List
		{
			get
			{
				if (list == null)
				{
					list = [];
					for (int i = Start; i < Stop; i++)
					{
						var line = lines[i];
						var s = line.IndexOf(':');
						if (s > 0)
						{
							var name = line[..s];
							var value = new StringBuilder();
							value.Append(line[(s + 1)..].Trim());
							while (i < Stop)
							{
								if (!IsValue.Match(lines[i + 1]).Success)
									break;
								i++;
								value.Append($"{lines[i]}");
							}
							list.Add(new HeaderValue(name, value.ToString()));
						}
					}
				}
				return list;
			}
		}
	}


	public static async Task<EmailMessage> DoitAsync(string path)
	{
		var sw = Stopwatch.StartNew();

		lines = await File.ReadAllLinesAsync(path);

		var emailMessage = new EmailMessage()
		{
			Header = new Header()
			{
				Start = 0
			}
		};

		for (int i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			if (emailMessage.Header.Stop < 0 && line == string.Empty)
			{
				emailMessage.Header.Stop = i;
				break;
			}
		}
		Debug.WriteLine(sw.ElapsedMilliseconds + "mS");

		var contentTypeHeader = emailMessage.Header.List.FirstOrDefault(x => x.Name == "Content-Type");
		if (contentTypeHeader == null)
			return emailMessage;

		switch (contentTypeHeader.Value)
		{
			default:
				break;
			case "multipart/mixed":
				var boundaryProperty = contentTypeHeader.Properties.FirstOrDefault(x => x.Name == "boundary");
				if (boundaryProperty.Value == null)
					return emailMessage;
				var boundary = boundaryProperty.Value.Trim('"');
				break;
		}

		//foreach (var item in emailMessage.Header.List)
		//{
		//	Debug.WriteLine($"{item.Name} = {item.Value}");
		//	Debug.WriteLine($"=======================");
		//	if (item.Properties.Count > 0)
		//	{
		//		foreach (var prop in item.Properties)
		//		{
		//			Debug.WriteLine($"{prop.Name} = {prop.Value}");
		//		}
		//		Debug.WriteLine($"=======================");
		//	}
		//}

		return emailMessage;
	}

}
