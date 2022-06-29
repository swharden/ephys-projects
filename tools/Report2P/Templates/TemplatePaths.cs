using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P.Templates
{
    public static class TemplatePaths
    {
        public static string GetTemplateFolder()
        {
            string exePath = System.Reflection.Assembly.GetEntryAssembly()?.Location
                ?? throw new InvalidOperationException();

            string appFolder = Path.GetDirectoryName(exePath)
                ?? throw new InvalidOperationException();

            string templateFolder = Path.Combine(appFolder, "Templates");

            if (!Directory.Exists(templateFolder))
                throw new DirectoryNotFoundException(templateFolder);

            return templateFolder;
        }
    }
}
