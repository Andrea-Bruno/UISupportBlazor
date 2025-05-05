using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;


namespace UISupportBlazor
{
    /// <summary>
    /// Markdown Converter
    /// </summary>
    public static class MarkdownConverter
    {
        // Thread-safe storage for footnotes
        private static readonly ConcurrentDictionary<string, string> _footnotes = new();
        private static readonly object _conversionLock = new();

        /// <summary>
        /// Converts Markdown text to HTML
        /// </summary>
        /// <param name="markdown">Input Markdown text</param>
        /// <returns>HTML output</returns>
        public static string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            lock (_conversionLock)
            {
                try
                {
                    _footnotes.Clear();

                    // Conversion pipeline
                    markdown = NormalizeLineEndings(markdown);
                    markdown = ExtractFootnoteDefinitions(markdown);
                    markdown = ProcessBlockElements(markdown);
                    markdown = ProcessInlineElements(markdown);
                    markdown = AppendFootnotes(markdown);

                    return markdown;
                }
                catch (Exception ex)
                {
                    return $"<p class=\"error\">Conversion error: {EscapeHtml(ex.Message)}</p>";
                }
            }
        }

        /// <summary>
        /// Normalizes line endings to Unix-style (\n)
        /// </summary>
        private static string NormalizeLineEndings(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        /// <summary>
        /// Extracts and stores footnote definitions
        /// </summary>
        private static string ExtractFootnoteDefinitions(string markdown)
        {
            return Regex.Replace(markdown, @"^\[\^(.+?)\]:\s*(.+?)\s*$", m =>
            {
                _footnotes[m.Groups[1].Value] = m.Groups[2].Value;
                return string.Empty;
            }, RegexOptions.Multiline);
        }

        /// <summary>
        /// Processes block-level Markdown elements
        /// </summary>
        private static string ProcessBlockElements(string markdown)
        {
            // Setext-style headers (=== and ---)
            markdown = Regex.Replace(markdown, @"^(.+)\n=+\s*$", "<h1>$1</h1>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^(.+)\n-+\s*$", "<h2>$1</h2>", RegexOptions.Multiline);

            // ATX-style headers (#)
            markdown = Regex.Replace(markdown, @"^(#{1,6})\s+(.*?)\s*$", m =>
                $"<h{m.Groups[1].Value.Length}>{m.Groups[2].Value}</h{m.Groups[1].Value.Length}>",
                RegexOptions.Multiline);

            // Horizontal rules
            markdown = Regex.Replace(markdown, @"^\s*([*\-_])(\s*\1){2,}\s*$", "<hr/>", RegexOptions.Multiline);

            // Blockquotes
            markdown = Regex.Replace(markdown, @"^>\s+(.*?)$", "<blockquote>$1</blockquote>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"(?s)<blockquote>(.*?)</blockquote>", m =>
            {
                string content = Regex.Replace(m.Groups[1].Value, @"\n>\s+", "\n");
                return $"<blockquote>{content}</blockquote>";
            });

            // Lists
            markdown = ProcessLists(markdown);

            // Code blocks
            markdown = Regex.Replace(markdown, @"```(\w*)\n([^`]+?)```", m =>
            {
                string lang = m.Groups[1].Value;
                string code = m.Groups[2].Value.Trim();
                return string.IsNullOrEmpty(lang)
                    ? $"<pre><code>{EscapeHtml(code)}</code></pre>"
                    : $"<pre><code class=\"language-{lang}\">{EscapeHtml(code)}</code></pre>";
            }, RegexOptions.Multiline);

            // Tables
            markdown = ProcessTables(markdown);

            // Paragraphs
            markdown = Regex.Replace(markdown, @"(?s)((?:^(?!<[a-z]|<\/[a-z]).*\n?)+)", m =>
            {
                string content = m.Value.Trim();
                return string.IsNullOrEmpty(content) ? string.Empty : $"<p>{content}</p>";
            }, RegexOptions.Multiline);

            return markdown;
        }

        /// <summary>
        /// Processes ordered and unordered lists with proper grouping
        /// </summary>
        private static string ProcessLists(string markdown)
        {
            // Process unordered lists
            markdown = Regex.Replace(markdown, @"((?:^\s*[*+-]\s+.*(?:\n|$))+)", m =>
            {
                var items = Regex.Matches(m.Value, @"^\s*[*+-]\s+(.*)", RegexOptions.Multiline)
                                 .Cast<Match>()
                                 .Select(match => $"<li>{match.Groups[1].Value.Trim()}</li>");
                return $"<ul>\n{string.Join("\n", items)}\n</ul>";
            }, RegexOptions.Multiline);

            // Process ordered lists
            markdown = Regex.Replace(markdown, @"((?:^\s*\d+\.\s+.*(?:\n|$))+)", m =>
            {
                var items = Regex.Matches(m.Value, @"^\s*\d+\.\s+(.*)", RegexOptions.Multiline)
                                 .Cast<Match>()
                                 .Select(match => $"<li>{match.Groups[1].Value.Trim()}</li>");
                return $"<ol>\n{string.Join("\n", items)}\n</ol>";
            }, RegexOptions.Multiline);

            return markdown;
        }


        /// <summary>
        /// Processes Markdown tables with alignment support
        /// </summary>
        private static string ProcessTables(string markdown)
        {
            return Regex.Replace(markdown, @"(?m)^\|(.+)\|\s*\n\|([\-:| ]+)\|\s*\n((?:\|.*\|\s*\n?)+)", m =>
            {
                var headers = m.Groups[1].Value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var aligns = m.Groups[2].Value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var rows = m.Groups[3].Value.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var sb = new StringBuilder("<table>\n<thead>\n<tr>");

                // Process headers
                for (int i = 0; i < headers.Length; i++)
                {
                    sb.Append($"<th{GetAlignment(aligns, i)}>{EscapeHtml(headers[i])}</th>");
                }

                sb.Append("</tr>\n</thead>\n<tbody>");

                // Process rows
                foreach (string row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;

                    var cells = row.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    sb.Append("\n<tr>");

                    for (int i = 0; i < cells.Length; i++)
                    {
                        sb.Append($"<td{GetAlignment(aligns, i)}>{EscapeHtml(cells[i])}</td>");
                    }

                    sb.Append("</tr>");
                }

                sb.Append("\n</tbody>\n</table>");
                return sb.ToString();
            }, RegexOptions.Multiline);
        }

        /// <summary>
        /// Processes inline Markdown elements
        /// </summary>
        private static string ProcessInlineElements(string markdown)
        {
            // Standard and automatic links
            markdown = Regex.Replace(markdown, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");
            markdown = Regex.Replace(markdown, @"<(https?://[^>]+)>", "<a href=\"$1\">$1</a>");
            markdown = Regex.Replace(markdown, @"<([^>]+@[^>]+)>", "<a href=\"mailto:$1\">$1</a>");

            // Images
            markdown = Regex.Replace(markdown, @"!\[([^\]]*)\]\(([^)]+)\)(?:\{([^}]+)\})?", m =>
            {
                string alt = EscapeHtml(m.Groups[1].Value);
                string src = m.Groups[2].Value;
                string attrs = m.Groups.Count > 3 ? m.Groups[3].Value : "";
                return $"<img src=\"{src}\" alt=\"{alt}\"{(string.IsNullOrWhiteSpace(attrs) ? "" : " " + attrs)}/>";
            });

            // Text formatting
            markdown = Regex.Replace(markdown, @"\*\*([^*]+)\*\*", "<strong>$1</strong>");
            markdown = Regex.Replace(markdown, @"__([^_]+)__", "<strong>$1</strong>");
            markdown = Regex.Replace(markdown, @"\*([^*]+)\*", "<em>$1</em>");
            markdown = Regex.Replace(markdown, @"_([^_]+)_", "<em>$1</em>");
            markdown = Regex.Replace(markdown, @"~~([^~]+)~~", "<s>$1</s>");
            markdown = Regex.Replace(markdown, @"`([^`]+)`", "<code>$1</code>");
            markdown = Regex.Replace(markdown, @"\\([\\`*{}\[\]()#+\-.!_>~|""'])", "$1");

            // Footnote references
            markdown = Regex.Replace(markdown, @"\[\^([^\]]+)\]", m =>
            {
                string id = m.Groups[1].Value;
                return _footnotes.ContainsKey(id)
                    ? $"<sup><a href=\"#fn-{id}\">[{id}]</a></sup>"
                    : $"[^{id}]";
            });

            // Line breaks
            markdown = Regex.Replace(markdown, @" {2,}\n", "<br/>\n");

            return markdown;
        }

        /// <summary>
        /// Appends collected footnotes to the document
        /// </summary>
        private static string AppendFootnotes(string markdown)
        {
            if (_footnotes.IsEmpty) return markdown;

            var sb = new StringBuilder("\n<div class=\"footnotes\">\n<ol>");
            foreach (var note in _footnotes)
            {
                sb.Append($"\n<li id=\"fn-{note.Key}\">{note.Value} <a href=\"#fn-ref-{note.Key}\">↩</a></li>");
            }
            sb.Append("\n</ol>\n</div>");

            return markdown + sb.ToString();
        }

        /// <summary>
        /// Gets alignment style for table cells
        /// </summary>
        private static string GetAlignment(string[] aligns, int index)
        {
            if (index >= aligns.Length) return "";

            string align = aligns[index].Trim();
            return align switch
            {
                _ when align.StartsWith(":") && align.EndsWith(":") => " style=\"text-align:center\"",
                _ when align.StartsWith(":") => " style=\"text-align:left\"",
                _ when align.EndsWith(":") => " style=\"text-align:right\"",
                _ => ""
            };
        }

        /// <summary>
        /// Escapes HTML special characters
        /// </summary>
        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            return new StringBuilder(text)
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;")
                .ToString();
        }
    }
}