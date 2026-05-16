using System;
using System.Text;
using System.Text.Json;

namespace QuickSheetB64;

class Program
{
    static void Main()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                string type = root.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";

                if (type == "init")
                {
                    var resp = new { type = "register", name = "quicksheet-b64", version = "1.0.0", prefix = "b64" };
                    Console.WriteLine(JsonSerializer.Serialize(resp));
                    Console.Out.Flush();
                }
                else if (type == "activate")
                {
                    string id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
                    string param = "";
                    if (root.TryGetProperty("params", out var paramsEl) && paramsEl.ValueKind == JsonValueKind.Array)
                    {
                        var arr = paramsEl.EnumerateArray();
                        if (arr.MoveNext()) param = arr.Current.GetString() ?? "";
                    }

                    // Use 0-based relative coordinates — host adds anchor offset
                    var cells = Process(param.Trim(), 0, 0);
                    var response = new { type = "write", id, cells };
                    Console.WriteLine(JsonSerializer.Serialize(response));
                    Console.Out.Flush();
                }
            }
            catch { }
        }
    }

    static List<object> Process(string input, int r, int c)
    {
        var cells = new List<object>();

        if (string.IsNullOrEmpty(input))
        {
            cells.Add(new { r, c = c + 1, v = "Usage: b64: <text>  or  b64: decode <base64string>" });
            return cells;
        }

        // Check for explicit mode prefix
        bool decodeMode = false;
        string data = input;

        if (input.StartsWith("decode ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("dec ", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("d ", StringComparison.OrdinalIgnoreCase))
        {
            decodeMode = true;
            int spaceIdx = input.IndexOf(' ');
            data = input[(spaceIdx + 1)..].Trim();
        }
        else if (input.StartsWith("encode ", StringComparison.OrdinalIgnoreCase) ||
                 input.StartsWith("enc ", StringComparison.OrdinalIgnoreCase) ||
                 input.StartsWith("e ", StringComparison.OrdinalIgnoreCase))
        {
            decodeMode = false;
            int spaceIdx = input.IndexOf(' ');
            data = input[(spaceIdx + 1)..].Trim();
        }
        else
        {
            // Auto-detect: try to decode first
            decodeMode = IsLikelyBase64(data);
        }

        try
        {
            if (decodeMode)
            {
                // Pad if needed
                string padded = data;
                int mod = padded.Length % 4;
                if (mod > 0) padded += new string('=', 4 - mod);

                byte[] bytes = Convert.FromBase64String(padded);
                string decoded = Encoding.UTF8.GetString(bytes);

                cells.Add(new { r, c, v = "🔓 DECODED" });
                // Split on newlines for multi-line output
                string[] lines = decoded.Split('\n');
                for (int i = 0; i < Math.Min(lines.Length, 20); i++)
                {
                    string line = lines[i].TrimEnd('\r');
                    if (line.Length > 200) line = line[..197] + "...";
                    cells.Add(new { r = r + i, c = c + 1, v = line });
                }
                cells.Add(new { r = r + Math.Min(lines.Length, 20), c = c + 1, v = $"({bytes.Length} bytes)" });
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                string encoded = Convert.ToBase64String(bytes);

                cells.Add(new { r, c, v = "🔒 ENCODED" });
                // Split long base64 into 76-char lines (MIME standard)
                int row = 0;
                for (int i = 0; i < encoded.Length; i += 76)
                {
                    string chunk = encoded.Substring(i, Math.Min(76, encoded.Length - i));
                    cells.Add(new { r = r + row, c = c + 1, v = chunk });
                    row++;
                    if (row >= 20) break;
                }
                cells.Add(new { r = r + row, c = c + 1, v = $"({bytes.Length} bytes → {encoded.Length} chars)" });
            }
        }
        catch (FormatException)
        {
            cells.Add(new { r, c = c + 1, v = "⚠ Invalid base64 input" });
        }
        catch (Exception ex)
        {
            cells.Add(new { r, c = c + 1, v = $"⚠ {ex.Message}" });
        }

        return cells;
    }

    static bool IsLikelyBase64(string s)
    {
        if (s.Length < 4) return false;
        // Base64 chars: A-Z, a-z, 0-9, +, /, =
        // If it contains spaces or non-base64 chars, it's probably plain text
        foreach (char ch in s)
        {
            if (!char.IsLetterOrDigit(ch) && ch != '+' && ch != '/' && ch != '=' && ch != '\n' && ch != '\r')
                return false;
        }
        // Try to decode
        try
        {
            string padded = s;
            int mod = padded.Length % 4;
            if (mod > 0) padded += new string('=', 4 - mod);
            Convert.FromBase64String(padded);
            return true;
        }
        catch { return false; }
    }
}
