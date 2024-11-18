// See https://aka.ms/new-console-template for more information

using Markdown;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var parser = new MdParser();

var tokens = parser.Tokenize("# Hello");

foreach (var token in tokens)
{
    Console.WriteLine(token.Value);
}