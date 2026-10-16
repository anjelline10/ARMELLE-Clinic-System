using System.Globalization;
using System.Text;

namespace Armelle.Services;
// Small dependency-free, single-page PDF generator for printable text records.
public static class SimplePdfDocument
{
    public static byte[] Create(string title, IEnumerable<string> lines)
    {
        static string Safe(string s) => new string((s ?? "").Select(c => c >= 32 && c <= 126 ? c : '?').ToArray()).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var commands = new StringBuilder("BT /F1 17 Tf 48 785 Td (" + Safe(title) + ") Tj ET\n");
        int y = 755;
        foreach (var line in lines.Take(43))
        {
            commands.Append("BT /F1 10 Tf 48 ").Append(y).Append(" Td (").Append(Safe(line.Length > 105 ? line[..105] : line)).Append(") Tj ET\n");
            y -= 16;
        }
        var stream = Encoding.ASCII.GetBytes(commands.ToString());
        var objects = new[] {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            "<< /Length " + stream.Length.ToString(CultureInfo.InvariantCulture) + " >>\nstream\n" + Encoding.ASCII.GetString(stream) + "endstream"
        };
        using var ms = new MemoryStream();
        void Write(string s) { var bytes=Encoding.ASCII.GetBytes(s); ms.Write(bytes,0,bytes.Length); }
        Write("%PDF-1.4\n"); var offsets=new List<long>{0};
        for(int i=0;i<objects.Length;i++){offsets.Add(ms.Position);Write($"{i+1} 0 obj\n{objects[i]}\nendobj\n");}
        var xref=ms.Position;Write($"xref\n0 {objects.Length+1}\n0000000000 65535 f \n");
        foreach(var offset in offsets.Skip(1)) Write($"{offset:0000000000} 00000 n \n");
        Write($"trailer\n<< /Size {objects.Length+1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return ms.ToArray();
    }
}
