namespace Eml2Pdf;

public class EmailMessage
{
	public List<(string Name, string Value)> Headers { get; set; } = [];
	public List<MimePart2> Parts { get; set; } = [];
	public List<Attachment> Attachments { get; set; } = [];

	// Optionele convenience properties voor veelgebruikte headers
	public string From => Headers.FirstOrDefault(h => h.Name.Equals("from", StringComparison.CurrentCultureIgnoreCase)).Value;
	public string To => Headers.FirstOrDefault(h => h.Name.Equals("to", StringComparison.CurrentCultureIgnoreCase)).Value;
	public string Subject => Headers.FirstOrDefault(h => h.Name.Equals("subject", StringComparison.CurrentCultureIgnoreCase)).Value;
	public DateTime? Date => DateTime.TryParse(Headers.FirstOrDefault(h => h.Name.Equals("date", StringComparison.CurrentCultureIgnoreCase)).Value, out DateTime date) ? date : (DateTime?)null;
	public string MessageId => Headers.FirstOrDefault(h => h.Name.Equals("message-id", StringComparison.CurrentCultureIgnoreCase)).Value;
}

public class MimePart2
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

