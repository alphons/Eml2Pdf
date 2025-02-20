namespace Eml2Pdf;

public class EmailMessage
{
	public List<(string Name, string Value)> Headers { get; set; } = new List<(string, string)>();
	public List<MimePart> Parts { get; set; } = new List<MimePart>();
	public List<Attachment> Attachments { get; set; } = new List<Attachment>();

	// Optionele convenience properties voor veelgebruikte headers
	public string From => Headers.FirstOrDefault(h => h.Name.ToLower() == "from").Value;
	public string To => Headers.FirstOrDefault(h => h.Name.ToLower() == "to").Value;
	public string Subject => Headers.FirstOrDefault(h => h.Name.ToLower() == "subject").Value;
	public DateTime? Date => DateTime.TryParse(Headers.FirstOrDefault(h => h.Name.ToLower() == "date").Value, out DateTime date) ? date : (DateTime?)null;
	public string MessageId => Headers.FirstOrDefault(h => h.Name.ToLower() == "message-id").Value;
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

