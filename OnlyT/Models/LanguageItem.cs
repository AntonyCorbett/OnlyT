namespace OnlyT.Models
{
    public class LanguageItem
    {
        public LanguageItem(string languageId, string languageName)
        {
            LanguageId = languageId;
            LanguageName = languageName;
        }

        public string LanguageId { get; }

        public string LanguageName { get; }
    }
}
