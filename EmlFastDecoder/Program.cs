using Eml2MimePart;

using System.Diagnostics;

static async Task ShowPartsAsync(int Depth, MimePart part)
{
	var contentType = part["Content-Type"];

	if (contentType != string.Empty)
	{
		for (var i = 0; i < Depth; i++)
			Debug.Write('\t');
		Debug.WriteLine(contentType);
	}

	if (part["Content-Disposition"].Contains("attachment"))
	{
		if (contentType.Contains("message/rfc822"))
		{
			if (DateTime.TryParse(part.Parts[0]["Date"], out DateTime dtm))
			{
				//await part.SaveAsync($"{dtm:yyyyMMdd-HHmmss}.eml", dtm);
			}
		}
	}


	if(contentType.StartsWith("text/html"))
	{
		//await File.WriteAllTextAsync("a.html", part.TextContent, Encoding.GetEncoding( part.CharSet));
	}

	if (contentType.StartsWith("image/"))
	{
		//await File.WriteAllBytesAsync("a.png", part.BinContent);
	}

	Depth++;
	foreach (var subpart in part.Parts)
	{
		await ShowPartsAsync(Depth, subpart);
	}
}

var sw = Stopwatch.StartNew();

//var email = await FastHelper.ReadEmlAsync(@"input\20240603-135130.eml");
var email = await MimePart.ReadEmlAsync(@"input\jun-jul 2024.eml");

await ShowPartsAsync(0, email);

Debug.WriteLine(sw.ElapsedMilliseconds + "mS");

Console.Write("enter");
Console.ReadLine();
