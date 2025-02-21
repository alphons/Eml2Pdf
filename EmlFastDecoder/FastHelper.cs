using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace EmlFastDecoder;

public class FastHelper
{
	private static string[] lines = [];

	private static Regex IsValue = new(@"^\W");

	private static readonly Regex NV = new(@"([^\s;=]+)=([^;]*?)(?=\s*?(?:;|$))");

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


	public static async Task DoitAsync(string path)
	{
		var sw = Stopwatch.StartNew();

		lines = await File.ReadAllLinesAsync(path);

		var header = new Header()
		{
			Start = 0
		};
		for (int i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			if (header.Stop < 0 && line == string.Empty)
				header.Stop = i;

		}
		Debug.WriteLine(sw.ElapsedMilliseconds + "mS");

		foreach (var item in header.List)
		{
			Debug.WriteLine($"{item.Name} = {item.Value}");
			Debug.WriteLine($"=======================");
			if (item.Properties.Count > 0)
			{
				foreach (var prop in item.Properties)
				{
					Debug.WriteLine($"{prop.Name} = {prop.Value}");
				}
				Debug.WriteLine($"=======================");
			}
		}


	}

}
