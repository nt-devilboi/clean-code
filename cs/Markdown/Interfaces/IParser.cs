namespace Markdown.Interfaces;

public interface IParser
{
    IReadOnlyList<Token> Tokenize(string text);

    IElementNode GenerateTree(IReadOnlyList<Token> tokens);
}