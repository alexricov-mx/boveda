using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace BERRecepcion.Front.Utilities
{
    public static class Helpers
    {
        public static IHtmlContent setHtmlText(this IHtmlHelper helper, string text)
        {
            TagBuilder t = new TagBuilder("div");
            t.InnerHtml.SetHtmlContent(StripImgRegex(text));
            return t;
        }

        public static string StripImgRegex(string source)
        {
            source = source.Replace("100%", "0%").Replace("width", "w").Replace("id=", "_id=").Replace("id =", "_id =");
            return Regex.Replace(source, "<img.*?>", string.Empty);
        }
    }
}
