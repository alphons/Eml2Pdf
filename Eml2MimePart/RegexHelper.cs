using System.Text.RegularExpressions;

namespace Eml2MimePart;

public partial class RegexHelper
{
	public static readonly Regex IsValue = IsValueRegex();
	public static readonly Regex Boundary = BoundaryRegex();
	public static readonly Regex CharSet = CharsetRegex();
	public static readonly Regex FileName = FileNameRegex();
	public static readonly Regex ContentId = ContentIdRegex();

	[GeneratedRegex(@"^\W")]
	private static partial Regex IsValueRegex();

	[GeneratedRegex(@"\Wboundary=""([^""]*)""")]
	private static partial Regex BoundaryRegex();

	[GeneratedRegex(@"\Wcharset=""([^""]*)""")]
	private static partial Regex CharsetRegex();

	[GeneratedRegex(@"\Wname=""([^""]+)""")]
	private static partial Regex FileNameRegex();

	[GeneratedRegex(@"<([^>]+)>")]
	private static partial Regex ContentIdRegex();
}
