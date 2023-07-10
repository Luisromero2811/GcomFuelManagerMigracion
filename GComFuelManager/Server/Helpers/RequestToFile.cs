using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json;

namespace GComFuelManager.Server.Helpers
{
    public class RequestToFile
    {
        public void GenerateFile(string value, string name, string foldername)
        {
            if (!Directory.Exists("Files"))
                Directory.CreateDirectory("Files");

            var path = string.IsNullOrEmpty(foldername) ? "default" : foldername;

            var pathFile = Directory.Exists($"Files/{path}");

            if (!pathFile)
                Directory.CreateDirectory($"Files/{path}");

            JsonDocument jsonDoc = JsonDocument.Parse(value);

            using (FileStream file = File.Create($"Files/{path}/{name}"))
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true }))
                {
                    jsonDoc.WriteTo(writer);
                }
            }
        }
    }
}
