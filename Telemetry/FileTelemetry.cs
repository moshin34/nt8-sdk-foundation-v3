using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using NT8.SDK;

namespace NT8.SDK.Telemetry
{
    /// <summary>
    /// File-based telemetry sink that writes events to disk in CSV or JSON Lines format.
    /// </summary>
    public sealed class FileTelemetry : ITelemetry
    {
        private readonly StreamWriter _writer;
        private readonly string _format;
        private readonly JavaScriptSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTelemetry"/> class.
        /// </summary>
        /// <param name="path">Path to the log file.</param>
        /// <param name="format">Output format: "csv" (default) or "jsonl".</param>
        public FileTelemetry(string path, string format = "csv")
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _format = (format ?? "csv").ToLowerInvariant();
            _writer = new StreamWriter(path, append: true) { AutoFlush = true };

            if (_format == "jsonl")
                _serializer = new JavaScriptSerializer();
        }

        /// <summary>
        /// Emits a telemetry event with the specified data.
        /// </summary>
        /// <param name="category">Event category.</param>
        /// <param name="message">Event message.</param>
        /// <param name="payload">Optional structured payload.</param>
        public void Emit(string category, string message, object payload = null)
        {
            if (_format == "jsonl")
            {
                var evt = new Dictionary<string, object>
                {
                    { "ts", DateTime.UtcNow.ToString("o") },
                    { "category", category },
                    { "message", message }
                };
                if (payload != null)
                    evt["payload"] = payload;
                string json = _serializer.Serialize(evt);
                _writer.WriteLine(json);
            }
            else
            {
                string line = string.Join(",", new[]
                {
                    DateTime.UtcNow.ToString("o"),
                    EscapeCsv(category),
                    EscapeCsv(message)
                });
                _writer.WriteLine(line);
            }
        }

        /// <inheritdoc />
        public void Emit(TelemetryEvent evt)
        {
            Emit(evt.Category, evt.Action, new { label = evt.Label, value = evt.Value });
        }

        private static string EscapeCsv(string value)
        {
            if (value == null) return string.Empty;
            bool quote = value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
            if (quote)
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}

