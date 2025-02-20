using Eml2PdfHelper;
using System.Text;

namespace Eml2Pdf;

class Program
{
	static void Main(string[] args)
	{
		try
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			string emlPath = "input.eml";
			string pdfPath = "output.pdf";

			var email = Helper.ParseMultipartEmlAsync(emlPath);

			Helper.CreatePdf(email, pdfPath);

			Console.WriteLine("Conversie voltooid!");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Fout: {ex.Message}");
		}
	}


}