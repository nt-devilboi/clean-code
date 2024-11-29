using System.Collections.Immutable;

namespace Markdown.Interfaces;

public interface IParser
{
    string Html(IImmutableList<Token> tokens); // нужно будет обобщить, до вида разметки.
}