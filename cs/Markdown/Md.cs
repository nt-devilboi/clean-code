using Markdown.Interfaces;

namespace Markdown;
// можно даже абстрактую фабрику заюзать, а вось будут разные visitor(не в html а в xml)

public class Md : IMd
{
    private readonly ILexer lexer = new MdParser();
    private readonly IParser Parser = new MdParser();
    public string Render(string text)
    {
        var tokens = lexer.Tokenize(text);
        return Parser.ToHtml(tokens);
    }
}