using FluentAssertions;

namespace Markdown.Tests;

//todo: текст header, если на второй строчки

public class Tests
{
    private MdParser parser;

    [SetUp]
    public void Setup()
    {
        parser = new MdParser();
    }

    [TestCaseSource(nameof(FirstLineTestCases))]
    public Token MdParser_ParseTags_OnFirstLine(string text)
    {
        return parser.Tokenize(text)[0];
    }


    public static IEnumerable<TestCaseData> FirstLineTestCases()
    {
        yield return new TestCaseData("# Hello")
            .Returns(new Token("Hello", TokenType.Header))
            .SetName("Header With Single Hash");
        yield return new TestCaseData("# Hello \n какой-то текст")
            .Returns(new Token("Hello", TokenType.Header))
            .SetName("Header With newLine after Space");
        yield return new TestCaseData("# Hello\n какой-то текст")
            .Returns(new Token("Hello", TokenType.Header))
            .SetName("Header With newLine without Space");
        yield return new TestCaseData("# Hello # Bye\nкакой-то текст")
            .Returns(new Token("Hello # Bye", TokenType.Header))
            .SetName("Header With newLine");
        yield return new TestCaseData("# ")
            .Returns(new Token("", TokenType.Header))
            .SetName("Header is Empty");
        yield return new TestCaseData("_Italic Text_")
            .Returns(new Token("Italic Text", TokenType.Italic))
            .SetName("Italic text");
        yield return new TestCaseData("_Italic Text")
            .Returns(new Token("Italic Text", TokenType.Italic))
            .SetName("Italic text without endTag");
        yield return new TestCaseData("__bold Text__")
            .Returns(new Token("bold Text", TokenType.Bold))
            .SetName("bold text");
        yield return new TestCaseData("__bold_Text__")
            .Returns(new Token("bold_Text", TokenType.Bold))
            .SetName("bold text with _");
        yield return new TestCaseData("__bold Text")
            .Returns(new Token("bold Text", TokenType.Bold))
            .SetName("bold text without end tag");
    }
}