using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MAUI_AI_Assistant
{
    public static class ParseHelper
    {
        public static List<string> GetImages(string xaml)
        {
            const string image = "https://placehold.co/";

            List<string> images = new List<string>();

            string[] separatingStrings = { "\n" };
            string[] lines = xaml.Split(separatingStrings, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (line.Contains(image))
                {
                    Match url = Regex.Match(line, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
                    images.Add(url.Value);
                }
            }

            return images;
        }

        public static string GetImageDescription(string image)
        {
            const string placeholder = "description=";
            int placeholderIndex = image.LastIndexOf(placeholder) + placeholder.Length;
            string description = image.Remove(0, placeholderIndex);
            description = description.Replace("%20", " ");

            return description;
        }

        public static string UpdateImages(string xaml, Dictionary<string, string> images)
        {
            string result = xaml;

            foreach (var replacement in images)
            {
                result = result.Replace(replacement.Key, replacement.Value);
            }

            return result;
        }
    }
}