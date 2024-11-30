namespace Markdown;

public class HtmlMdConverter : TokenMdConverter
{
    protected override string ItalicOpen() => "<em>";

    protected override string ItalicClose() => "</em>";

    protected override string BoldOpen() => "<strong>";
    
    protected override string BoldClose() => "</strong>";
    
    protected override string HeaderOpen() => "<h1>";
    
    protected override string HeaderClose() => "</h1>";
}