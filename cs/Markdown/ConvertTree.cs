using System.Collections.Immutable;
using Markdown.treeVisitor;

namespace Markdown;

internal class ConvertTree : IConvertTree
{
    public INode Convert(IImmutableList<Token> tokens)
    {
        var tree = CreateAstTree(tokens);


        return tree;
    }

    private INode CreateAstTree(IImmutableList<Token> tokens)
    {
        var root = new LineNode();
        for (var i = 0; i < tokens.Count; i++)
        {
            if (!tokens[i].IsTag)
            {
                root.InnerNode.Add(new TextNode(tokens[i].Value));
            }

            if (tokens[i].Type is TokenType.Header)
            {
                var header = CreateHeader(tokens, ref i);
                root.NextNode = header;
            }
        }


        return root;
    }

    private HeaderNode CreateHeader(IImmutableList<Token> tokens, ref int j)
    {
        var header = new HeaderNode();
        for (j++; j < tokens.Count; j++)
        {
            if (tokens[j].Type is TokenType.Word)
            {
                header.InnerNode.Add(new TextNode(tokens[j].Value));
            }

            if (tokens[j].Type is TokenType.WhiteSpace)
            {
                header.InnerNode.Add(new TextNode(tokens[j].Value));
            }


            if (tokens[j].Type is TokenType.NewLine)
            {
                header.NextNode = CreateNewLIne(tokens, ref j);
                break;
            }
        }

        return header;
    }

    private INode CreateNewLIne(IImmutableList<Token> tokens, ref int j)
    {
        var newLine = new NewLineNode();

        for (j++; j < tokens.Count; j++)
        {
            if (!tokens[j].IsTag)
            {
                newLine.InnerNode.Add(new TextNode(tokens[j].Value));
            }
        }


        return newLine;
    }
}