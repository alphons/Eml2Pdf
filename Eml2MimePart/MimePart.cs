﻿
using System.Text;

namespace Eml2MimePart;
public class MimePart(string[] lines)
{
	private const int InvalidIndex = -1;
	private readonly string[] _lines = lines;
	private int _start = InvalidIndex;
	private int _stop = InvalidIndex;
	private int _startContent = InvalidIndex;

	private IReadOnlyList<(string Name, string Value)>? headers;

	private IReadOnlyList<MimePart>? parts;

	private IReadOnlyList<(string Name, string Value)> BuildHeaders()
	{
		var result = new List<(string Name, string Value)>();
		for (int i = _start; i < _stop; i++)
		{
			var line = _lines[i];
			if (string.IsNullOrEmpty(line))
			{
				_startContent = i + 1;
				return result;
			}
			var s = line.IndexOf(':');
			if (s > 0)
			{
				var name = line[..s];
				var value = new StringBuilder(line[(s + 1)..].Trim());
				// NB: Laatste regel wordt genegeerd als deze exact op 'stop' valt, wat bij MIME-headers zeldzaam is.
				while (i < _stop - 1 && RegexHelper.IsValue.IsMatch(_lines[i + 1]))
				{
					value.Append(_lines[++i]);
				}
				result.Add((name, Helpers.DecodeMimeEncodedWord(value.ToString())));
			}
		}
		return result;
	}

	private void EnsureBoundariesInitialized()
	{
		if (_startContent == InvalidIndex || _stop == InvalidIndex)
			throw new InvalidOperationException("Content boundaries not initialized");
	}

	// Parseert multipart MIME-content en retourneert een lijst van sub-parts gescheiden door de boundary.
	private IReadOnlyList<MimePart> ParseMultipart(string boundary)
	{
		var result = new List<MimePart>();
		MimePart? currentPart = null;

		for (int i = _start; i < _stop; i++)
		{
			var line = _lines[i];
			if (line.StartsWith($"{boundary}--"))
			{
				if (currentPart != null) // Optioneel, maar kan weg
					currentPart._stop = i - 1;
				break;
			}

			if (line.StartsWith(boundary))
			{
				if (currentPart is not null)
					currentPart._stop = i - 1;
				currentPart = new MimePart(_lines) { _start = i + 1 };
				result.Add(currentPart);
			}
		}
		if (currentPart is not null && currentPart._stop == InvalidIndex)
			currentPart._stop = _stop - 1;
		return result.AsReadOnly();
	}

	private IReadOnlyList<MimePart> BuildParts()
	{
		var contentType = this["Content-Type"];

		if (string.IsNullOrEmpty(contentType))
			return [];

		var disposition = this["Content-Disposition"];
		if (!string.IsNullOrEmpty(disposition) && disposition.Split(';')[0].Trim().Equals("attachment", StringComparison.OrdinalIgnoreCase))
		{
			return
			[
				new(_lines)
				{
					_start = _startContent,
					_stop = _stop,
					_startContent = _startContent
				}
			];
		}

		if (!contentType.StartsWith("multipart/"))
			return [];

		var boundaryMatch = RegexHelper.Boundary.Match(contentType);
		if (!boundaryMatch.Success)
			return [];

		var boundary = $"--{boundaryMatch.Groups[1].Value}";
		return ParseMultipart(boundary);
	}

	// Only valif for attachements
	public string FileName
	{
		get
		{
			var contentType = this["Content-Type"];

			var match = RegexHelper.FileName.Match(contentType);

			if (!match.Success)
				return string.Empty;

			return match.Groups[1].Value;
		}
	}


	public string ContentId
	{
		get
		{
			var contentId = this["Content-ID"];

			var match = RegexHelper.ContentId.Match(contentId);

			if (!match.Success)
				return string.Empty;

			return match.Groups[1].Value;
		}
	}


	// Headers van een part indien aanwezig
	public IReadOnlyList<(string Name, string Value)> Headers => headers ??= BuildHeaders();

	// Retourneert de charset uit Content-Type, of Encoding.UTF8.WebName ("utf-8") als deze niet gespecificeerd is.
	//public string CharSet => RegexHelper.CharSet.Match(this["Content-Type"])?.Groups[1].Value ?? Encoding.UTF8.WebName;

	public string CharSet
	{
		get
		{
			string contentType = this["Content-Type"];
			if (string.IsNullOrEmpty(contentType))
				return Encoding.UTF8.WebName;

			var match = RegexHelper.CharSet.Match(contentType);
			return match?.Success == true ? match.Groups[1].Value : Encoding.UTF8.WebName;
		}
	}

	// Retourneert de tekstuele inhoud van deze MIME-part, gedecodeerd volgens de Content-Transfer-Encoding.
	public string TextContent
	{
		get
		{
			EnsureBoundariesInitialized();

			var val = string.Join(Environment.NewLine, _lines[_startContent.._stop]);

			return this["Content-Transfer-Encoding"] switch
			{
				"7bit" => val, // Pure ASCII, geen decoding nodig
				"8bit" => val, // 8-bit tekst, vertrouw op CharSet
				"quoted-printable" => Helpers.QuotedPrintableDecode(val, Encoding.GetEncoding( CharSet )),
				"base64" => Encoding.GetEncoding(CharSet).GetString( Convert.FromBase64String(string.Join(string.Empty, _lines[_startContent.._stop]))),
				"binary" => throw new InvalidOperationException("Binary content cannot be represented as text"),
				"" => val, // Geen encoding gespecificeerd, assumeer plain text
				var enc => throw new NotSupportedException($"Unsupported Content-Transfer-Encoding: {enc}")
			};
		}
	}

	// Retourneert de binaire inhoud van deze MIME-part, gedecodeerd volgens de Content-Transfer-Encoding.
	public byte[] BinaryContent
	{
		get
		{
			EnsureBoundariesInitialized();

			var val = string.Join("", _lines[_startContent.._stop]);
			var enc = Encoding.GetEncoding(CharSet);
			return this["Content-Transfer-Encoding"] switch
			{
				"7bit" => Encoding.ASCII.GetBytes(val), // Alleen ASCII, veilig als bytes
				"8bit" => Encoding.GetEncoding(CharSet).GetBytes(val), // Gebruik de gespecificeerde charset
				"binary" => Encoding.GetEncoding(CharSet).GetBytes(val), // NB: Binary data kan corrupt raken; overweeg byte[] input voor echte support
				"quoted-printable" => enc.GetBytes(Helpers.QuotedPrintableDecode(val, enc)),
				"base64" => Convert.FromBase64String(val),
				"" => Encoding.UTF8.GetBytes(val), // Geen encoding, default UTF-8
				var enco => throw new NotSupportedException($"Unsupported Content-Transfer-Encoding: {enco}")
			};
		}
	}

	// Geeft de waarde van een header met de opgegeven naam, of een lege string als deze niet bestaat.
	public string this[string name] => Headers.FirstOrDefault(x => x.Name == name).Value ?? string.Empty;

	// Retourneert een read-only lijst van sub-parts (attachments of multipart secties).
	public IReadOnlyList<MimePart> Parts => parts ??= BuildParts();

	// Slaat deze MIME-part asynchroon op naar een bestand met de huidige datum/tijd.
	public Task SaveAsync(string path, CancellationToken ct = default) => SaveAsync(path, DateTime.Now, ct);

	// Slaat deze MIME-part asynchroon op (met encoding) naar een bestand met de opgegeven datum/tijd.
	public async Task SaveAsync(string path, DateTime dtm, CancellationToken ct = default)
	{
		EnsureBoundariesInitialized();
		var encoding = string.IsNullOrWhiteSpace(CharSet) ? Encoding.UTF8 : Encoding.GetEncoding(CharSet);
		await File.WriteAllLinesAsync(path, _lines[_startContent.._stop], encoding, ct);
		File.SetAttributes(path, FileAttributes.ReadOnly);
		File.SetLastWriteTimeUtc(path, dtm);
		File.SetCreationTime(path, dtm);
	}

	// Leest een .eml-bestand asynchroon en retourneert een MimePart met de geparste inhoud.
	public static async Task<MimePart> ReadEmlAsync(string path)
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		var lines = await File.ReadAllLinesAsync(path);
		return new MimePart(lines) { _start = 0, _stop = lines.Length };
	}
}