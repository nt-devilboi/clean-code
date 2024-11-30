using Markdown.Interfaces;

namespace Markdown;

public class Md : IMd
{
    private readonly ILexer lexer = new MdParser(); // так называемые неявные зависимости
    private readonly TokenMdConverter mdConverter = new HtmlMdConverter();
    public string Render(string text)
    {
        var tokens = lexer.Tokenize(text);
        return mdConverter.Convert(tokens);
    }
}