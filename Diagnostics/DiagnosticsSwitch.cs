using System;
using System.Collections.Generic;
using NT8.SDK;

namespace NT8.SDK.Diagnostics
{
    /// <summary>
    /// Central toggle and allowlist filter for diagnostics and telemetry emission.
    /// </summary>
    public sealed class DiagnosticsSwitch
    {
        private readonly HashSet<string> _tagAllow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _catAllow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _useTagAllow;
        private bool _useCatAllow;

        /// <summary>
        /// Gets or sets whether diagnostics and telemetry are globally enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Sets the allowlist of diagnostic tags. Null or empty clears the allowlist.
        /// </summary>
        /// <param name="tags">Tags allowed for capture.</param>
        public void SetDiagnosticTagAllowlist(string[] tags)
        {
            _tagAllow.Clear();
            if (tags == null || tags.Length == 0)
            {
                _useTagAllow = false;
                return;
            }
            _useTagAllow = true;
            for (int i = 0; i < tags.Length; i++)
            {
                var t = tags[i];
                if (!string.IsNullOrEmpty(t)) _tagAllow.Add(t);
            }
        }

        /// <summary>
        /// Sets the allowlist of telemetry categories. Null or empty clears the allowlist.
        /// </summary>
        /// <param name="categories">Categories allowed for emission.</param>
        public void SetTelemetryCategoryAllowlist(string[] categories)
        {
            _catAllow.Clear();
            if (categories == null || categories.Length == 0)
            {
                _useCatAllow = false;
                return;
            }
            _useCatAllow = true;
            for (int i = 0; i < categories.Length; i++)
            {
                var c = categories[i];
                if (!string.IsNullOrEmpty(c)) _catAllow.Add(c);
            }
        }

        /// <summary>
        /// Determines whether diagnostics should be captured for the specified tag.
        /// </summary>
        /// <param name="tag">Diagnostic tag.</param>
        /// <returns>True if enabled and the tag is allowed.</returns>
        public bool ShouldCapture(string tag)
        {
            if (!Enabled) return false;
            if (!_useTagAllow) return true;
            return _tagAllow.Contains(tag ?? string.Empty);
        }

        /// <summary>
        /// Determines whether telemetry should be emitted for the specified event.
        /// </summary>
        /// <param name="evt">Telemetry event.</param>
        /// <returns>True if enabled and the event category is allowed.</returns>
        public bool ShouldEmit(TelemetryEvent evt)
        {
            if (!Enabled) return false;
            if (!_useCatAllow) return true;
            return _catAllow.Contains(evt.Category ?? string.Empty);
        }

#if DEBUG
        internal static class DiagnosticsSwitchTests
        {
            internal static void Smoke()
            {
                var sw = new DiagnosticsSwitch();
                System.Diagnostics.Debug.Assert(sw.ShouldCapture("foo"));
                sw.SetDiagnosticTagAllowlist(new string[] { "bar" });
                System.Diagnostics.Debug.Assert(sw.ShouldCapture("bar"));
                System.Diagnostics.Debug.Assert(!sw.ShouldCapture("foo"));
                sw.SetTelemetryCategoryAllowlist(new string[] { "cat" });
                System.Diagnostics.Debug.Assert(sw.ShouldEmit(new TelemetryEvent("cat", "", "", "")));
                System.Diagnostics.Debug.Assert(!sw.ShouldEmit(new TelemetryEvent("dog", "", "", "")));
            }
        }
#endif
    }
}

