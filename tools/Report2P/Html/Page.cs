using System.Text;

namespace Report2P.Html;

public class Page
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;

    public readonly StringBuilder Content = new();

    public Page()
    {

    }

    public void Save(string path)
    {
        path = Path.GetFullPath(path);
        HtmlTemplates.ThrowIfNotPopulated();

        StringBuilder sb = new();
        sb.Append("<div class='border bg-light shadow p-3'>");

        sb.Append($"<h1>{Title}</h1>");
        sb.Append($"<div><code>{Subtitle}</code></div>");

        sb.Append("</div>");

        string html = HtmlTemplates.BuildPage(Title, sb.ToString());
        File.WriteAllText(path, html);
        Console.WriteLine($"  wrote: {path}");
    }
}
