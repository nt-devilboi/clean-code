using System.Collections.Immutable;
using Markdown.treeVisitor;

namespace Markdown.Converter;

internal class ConvertTree : IConvertTree
{
    public INode Convert(IImmutableList<Token> tokens)
    {
        var start = -1;
        var tree = CreateNode(tokens, new LineNode(), false, ref start);


        return tree;
    }

    private TNode CreateNode<TNode>(IImmutableList<Token> tokens, TNode node, bool canIncludeHardTag, ref int j)
        where TNode : INode
    {
        for (j++; j < tokens.Count; j++)
        {
            if (!tokens[j].IsTag)
            {
                node.InnerNode.Add(new TextNode(tokens[j].Value));
            }

            else if (tokens[j].Type is TokenType.Header)
            {
                var header = CreateNode(tokens, new HeaderNode(), true, ref j);
                node.NextNode = header;
            }

            else if (tokens[j].Type is TokenType.Italic)
            {
                if (node is ItalicNode)
                {// canInclude... работает не самым правильным обрзаом
                    if (canIncludeHardTag) node.NextNode = CreateNode(tokens, new LineNode(), false, ref j);

                    break;
                }

                node.InnerNode.Add(CreateNode(tokens, new ItalicNode(), false, ref j));
            }

            else if (tokens[j].Type is TokenType.Bold)
            {
                if (node is BoldNode)
                {
                    node.NextNode = CreateNode(tokens, new LineNode(), false, ref j);
                    
                    break;
                }

                if (node is HeaderNode)
                {// мне нравится как здесь происходит жанглирование экзеплярами node, внутри который хранится информация.
                    node.InnerNode.Add(CreateNode(tokens, new BoldNode(), true, ref j)); 
                }

                else
                {
                    var bold = CreateNode(tokens, new BoldNode(), true, ref j);

                    node.NextNode = bold;
                }
            }

            else if (tokens[j].Type is TokenType.NewLine)
            {
                if (node is HeaderNode)
                {
                    node.NextNode = CreateNode(tokens, new NewLineNode(), false, ref j);
                    break;
                }
            }
        }

        return node;
    }

    
    // todo: всё ниже убрать, когда выше будет работать
    private LineNode CreateLineNode(IImmutableList<Token> tokens, ref int j)
    {
        var root = new LineNode();
        for (j++; j < tokens.Count; j++)
        {
            if (!tokens[j].IsTag)
            {
                root.InnerNode.Add(new TextNode(tokens[j].Value));
            }

            else if (tokens[j].Type is TokenType.Header)
            {
                var header = CreateHeader(tokens, ref j);
                root.NextNode = header;
            }

            else if (tokens[j].Type is TokenType.Italic)
            {
                var italic = CreateItalicNode(tokens, false, ref j);
                root.NextNode = italic;
            }

            else if (tokens[j].Type is TokenType.Bold)
            {
                var bold = CreateBoldNode(tokens, false, ref j);
                root.NextNode = bold;
            }
        }


        return root;
    }

    private BoldNode CreateBoldNode(IImmutableList<Token> tokens, bool isInner, ref int j)
    {
        var bold = new BoldNode();

        for (j++; j < tokens.Count; j++)
        {
            if (!tokens[j].IsTag)
            {
                bold.InnerNode.Add(new TextNode(tokens[j].Value));
            }

            else if (tokens[j].Type is TokenType.Italic)
            {
                bold.InnerNode.Add(CreateItalicNode(tokens, true, ref j));
            }

            else if (tokens[j].Type is TokenType.Bold)
            {
                if (!isInner) bold.NextNode = CreateLineNode(tokens, ref j);
                break;
            }
        }


        return bold;
    }

    private HeaderNode CreateHeader(IImmutableList<Token> tokens, ref int j)
    {
        var header = new HeaderNode();
        for (j++; j < tokens.Count; j++)
        {
            if (!tokens[j].IsTag)
            {
                header.InnerNode.Add(new TextNode(tokens[j].Value));
            }

            else if (tokens[j].Type is TokenType.NewLine)
            {
                header.NextNode = CreateNewLIne(tokens, ref j);
                break;
            }

            else if (tokens[j].Type is TokenType.Bold)
            {
                header.InnerNode.Add(CreateBoldNode(tokens, true, ref j));
            }

            else if (tokens[j].Type is TokenType.Italic)
            {
                header.InnerNode.Add(CreateItalicNode(tokens, true, ref j));
            }
        }

        return header;
    }

    private ItalicNode CreateItalicNode(IImmutableList<Token> tokens, bool isInner, ref int j)
    {
        var italicNode = new ItalicNode();
        for (j++; j < tokens.Count; j++)
        {
            if (!tokens[j].IsTag)
            {
                italicNode.InnerNode.Add(new TextNode(tokens[j].Value));
            }

            else if (tokens[j].Type is TokenType.Italic)
            {
                if (!isInner) italicNode.NextNode = CreateLineNode(tokens, ref j);

                break;
            }
        }

        return italicNode;
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