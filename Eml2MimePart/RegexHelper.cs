using System.Text.RegularExpressions;

namespace Eml2MimePart;

public partial class RegexHelper
{
	public static readonly Regex IsValue = IsValueRegex();
	public static readonly Regex Boundary = BoundaryRegex();
	public static readonly Regex CharSet = CharsetRegex();

	[GeneratedRegex(@"^\W")]
	private static partial Regex IsValueRegex();

	[GeneratedRegex(@"\Wboundary=""([^""]*)""")]
	private static partial Regex BoundaryRegex();

	[GeneratedRegex(@"\Wcharset=""([^""]*)""")]
	private static partial Regex CharsetRegex();
}
