using System.Text;
using System.Text.RegularExpressions;

namespace EmlFastDecoder;

public class FastHelper
{
	private static string[] lines = [];

	private static readonly Regex IsValue = new(@"^\W");

	private static readonly Regex Boundary = new(@"\Wboundary=""([^""]*)""");
	private static readonly Regex RegCharSet = new(@"\Wcharset=""([^""]*)""");

	public class MimePart
	{
		public int Start { get; set; } = -1;
		public int Stop { get; set; } = -1;
		public int StartContent { get; set; } = -1;

		private List<(string Name, string Value)>? headers = null;
		public List<(string Name, string Value)> Headers
		{
			get
			{
				if (headers == null)
				{
					headers = [];
					for (int i = Start; i < Stop; i++)
					{
						var line = lines[i];
						if (line == string.Empty)
						{
							this.StartContent = i + 1;
							return headers;
						}
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
							headers.Add((name, value.ToString()));
						}
					}
				}
				return headers;
			}
		}

		public string CharSet
		{
			get
			{
				var match = RegCharSet.Match(this["Content-Type"]);
				if (match.Success)
					return match.Groups[1].Value;
				else
					return "utf8";
			}
		}

		public string TextContent
		{
			get
			{
				var charset = CharSet;
				var encoding = this["Content-Transfer-Encoding"];

				var val = string.Join(Environment.NewLine, lines.Skip(StartContent).Take(Stop - StartContent).ToArray());

				if(encoding == "quoted-printable")
					val = QuotedPrintableDecoder.Decode(val, charset);

				return val;
			}
		}

		public byte[] BinContent
		{
			get
			{
				var val = string.Join("", lines.Skip(StartContent).Take(Stop - StartContent).ToArray());

				var encoding = this["Content-Transfer-Encoding"];
				if (encoding == "base64")
					return Convert.FromBase64String(val);
				else
					return Encoding.UTF8.GetBytes(val);
			}
		}

		public string this[string name] => this.Headers.FirstOrDefault(x => x.Name == name).Value ?? string.Empty;


		private List<MimePart>? parts = null;
		public List<MimePart> Parts
		{
			get
			{
				if (parts == null)
				{
					parts = [];
					var contentType = this["Content-Type"];
					var hasAttachment = this["Content-Disposition"].Contains("attachment");
					if (contentType == string.Empty)
						return parts;

					if (hasAttachment)
					{
						var attachment = new MimePart()
						{
							Start = StartContent,
							Stop = Stop,
							StartContent = StartContent
						};
						parts.Add(attachment);
						return parts;
					}

					if (!contentType.StartsWith("multipart/"))
						return parts;

					var match = Boundary.Match(contentType);
					if (!match.Success)
						return parts;
					var boundaryId = match.Groups[1].Value;
					var boundary = $"--{boundaryId}";
					var count = 0;
					MimePart? part = null;
					for (int i = Start; i < Stop; i++)
					{
						var line = lines[i];
						if (line.StartsWith(boundary))
						{
							if (part != null)
								part.Stop = i - 1;
							if (line.StartsWith($"{boundary}--"))
								break;
							part = new MimePart()
							{
								Start = i + 1
							};
							parts.Add(part);
							count++;
							//Debug.WriteLine($"{count} {i + 1} {line}");
						}
					}
				}
				return parts;
			}
		}

		public async Task SaveAsync(string Path) => await SaveAsync(Path, DateTime.Now);
		public async Task SaveAsync(string Path, DateTime dtm)
		{
			await File.WriteAllLinesAsync(Path, lines.Skip(StartContent).Take(Stop - StartContent).ToArray());
			File.SetAttributes(Path, FileAttributes.ReadOnly);
			File.SetLastWriteTimeUtc(Path, dtm);
			File.SetCreationTime(Path, dtm);
		}
	}

	public static void Clear()
	{
		lines = [];
	}

	public static async Task<MimePart> ReadEmlAsync(string path)
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		lines = await File.ReadAllLinesAsync(path);

		MimePart emailMessage = new()
		{
			Start = 0,
			Stop = lines.Length
		};

		return emailMessage;
	}

}
