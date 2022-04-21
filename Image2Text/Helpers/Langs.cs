using IronOcr;
using System.Collections.Generic;

namespace Image2Text.Helpers
{
    public static class Langs
    {
        public static Dictionary<string, OcrLanguage> Dict = new Dictionary<string, OcrLanguage>
        {
            { "en", OcrLanguage.EnglishBest },
            { "ru", OcrLanguage.RussianBest }
        };
    }
}
