using System.Collections.Immutable;
using Markdown.treeVisitor;

namespace Markdown;

internal interface IConvertTree
{
    INode Convert(IImmutableList<Token> tokens);
}