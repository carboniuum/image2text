using IronOcr;
using System.Collections.Generic;

namespace Image2Text.Helpers
{
    public static class Languages
    {
        public static Dictionary<string, OcrLanguage> GetLanguages() => new Dictionary<string, OcrLanguage>
        {
            { "en", OcrLanguage.EnglishBest },
            { "ru", OcrLanguage.RussianBest },
            { "jp", OcrLanguage.JapaneseAlphabetBest }
        };
    }
}
