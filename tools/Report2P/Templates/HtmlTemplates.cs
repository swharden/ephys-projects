using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P
{
    public static class HtmlTemplates
    {
        //public static string TemplateFolder;
        public static string Base = string.Empty;
        public static string Header = string.Empty;
        public static string TimelineItemDetails = string.Empty;

        public static void ThrowIfNotPopulated()
        {
            if (string.IsNullOrEmpty(Base))
                throw new InvalidOperationException("Templates must be populated before use.");
        }

        public static void Populate(string TemplateFolder)
        {
            Base = File.ReadAllText(Path.Combine(TemplateFolder, "base.html"));
            Header = File.ReadAllText(Path.Combine(TemplateFolder, "header.html"));
            TimelineItemDetails = File.ReadAllText(Path.Combine(TemplateFolder, "timeline-item-details.html"));
        }

        public static string BuildPage(string title, string content)
        {
            return Base.Replace("{{TITLE}}", title).Replace("{{CONTENT}}", content);
        }
    }
}
