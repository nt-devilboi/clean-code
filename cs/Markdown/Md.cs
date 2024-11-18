using Markdown.Interfaces;

namespace Markdown;

public class Md : IMd
{
    private readonly ISyntaxAnalyzer syntaxAnalyzer;
    private readonly IParser parser;
    private readonly IVisitorTranslator translator;
    // можно даже абстрактую фабрику заюзать, а вось будут разные visitor(не в html а в xml)

    public string Render(string text)
    {
        var syntaxTree = syntaxAnalyzer.ParseText(text, parser);
        
        return syntaxAnalyzer.Translate(syntaxTree, translator);
    }
}