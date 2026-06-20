using System.Net;
using System.Text;
using System.Text.Json;

namespace WebMVC.Ultilities
{
    public static class Translator  
    {
        public static string TranslateText(string input, string sLang = "vi", string tLang = "zh")
        {
            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sLang}&tl={tLang}&dt=t&q={Uri.EscapeDataString(input)}";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
                request.Method = "GET";

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string content = reader.ReadToEnd();
                    return ParseTranslationResult(content);
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi dịch: {ex.Message}";
            }
        }

        private static string ParseTranslationResult(string json)
        {
            try
            {
                var jsonData = JsonSerializer.Deserialize<object[]>(json);
                var translatedText = new StringBuilder();

                foreach (var sentence in JsonSerializer.Deserialize<object[]>(jsonData[0].ToString()))
                {
                    translatedText.Append(((JsonElement)sentence).EnumerateArray().First().GetString());
                }

                return translatedText.ToString();
            }
            catch
            {
                return "Lỗi khi xử lý dữ liệu dịch.";
            }
        }
    }
}
