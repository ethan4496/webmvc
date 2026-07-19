using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WebMVC.Entities;

namespace WebMVC.Models.Responses
{
    public class PostResponse : Post
    {
        public List<CategoryResponse> Categories { get; set; } = new();

        public string AuthorName { get; set; }

        public int ReadingTimeMinutes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Content))
                {
                    return 1;
                }

                var plainText = Regex.Replace(Content, "<.*?>", " ");
                var wordCount = plainText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
                return Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
            }
        }

        public List<PostTocItem> TableOfContents
        {
            get
            {
                var items = new List<PostTocItem>();
                if (string.IsNullOrWhiteSpace(Content))
                {
                    return items;
                }

                var matches = Regex.Matches(Content, "<h[23]\\b([^>]*)>(.*?)</h[23]>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match match in matches)
                {
                    var label = Regex.Replace(match.Groups[2].Value, "<.*?>", "").Trim();
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    var idAttr = Regex.Match(match.Groups[1].Value, "id=[\"']([^\"']*)[\"']", RegexOptions.IgnoreCase);
                    var id = idAttr.Success ? idAttr.Groups[1].Value : Slugify(label);

                    items.Add(new PostTocItem { Id = id, Label = label });
                }

                return items;
            }
        }

        private static string Slugify(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var result = sb.ToString().Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd').Replace('Đ', 'D')
                .ToLowerInvariant();
            result = Regex.Replace(result, "[^a-z0-9\\s-]", "");
            result = Regex.Replace(result, "\\s+", "-");
            result = Regex.Replace(result, "-+", "-");
            return result.Trim('-');
        }
    }
}
