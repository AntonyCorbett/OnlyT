﻿namespace OnlyT.Utils
{
    using System.Collections.Generic;
    using System.IO;

    public static class StringUtils
    {
        public static IEnumerable<string> SplitIntoLines(this string text, bool ignoreEmptyLines = true)
        {
            var result = new List<string>();
            using StringReader sr = new(text);

            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (ignoreEmptyLines || !string.IsNullOrWhiteSpace(line))
                {
                    result.Add(line);
                }
            }

            return result;
        }
    }
}
