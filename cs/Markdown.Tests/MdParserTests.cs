using FluentAssertions;
using Markdown.Interfaces;

namespace Markdown.Tests;

//todo: текст header, если на второй строчки

public class MdParserTests
{
    private ILexer lexer;

    [SetUp]
    public void Setup()
    {
        lexer = new MdParser();
    }

    [TestCase("# hello", "# ", "hello", "")]
    public void MdParser_ParseTag_HeaderWithText(string text, string tokenHeaderStart, string tokenText,
        string tokenHeaderEnd)
    {
        var tokens = lexer.Tokenize(text);

        tokens[0].Should().Be(new Token(tokenHeaderStart, TokenType.Header)
            { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token(tokenText, TokenType.Word) { StartIndex = 2 });
    }


    [TestCase("# hello\n", "# ", "hello", "\n")]
    public void MdParser_ParseTag_HeaderWithTextWithNewLine(string text,
        string tokenHeaderStart,
        string tokenText,
        string tokenHeaderEnd
    )
    {
        var tokens = lexer.Tokenize(text);

        tokens[0].Should().Be(new Token(tokenHeaderStart, TokenType.Header)
            { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token(tokenText, TokenType.Word) { StartIndex = 2 });
        tokens[2].Should().Be(new Token(tokenHeaderEnd, TokenType.NewLine)
            { StartIndex = 7, IsTag = true });
    }

    [Test]
    public void MdParser_ParseTag_HeaderWithTextWithNewLineAndText()
    {
        var tokens = lexer.Tokenize("# hello\n hi");

        tokens[0].Should().Be(new Token("# ", TokenType.Header)
            { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token("hello", TokenType.Word) { StartIndex = 2 });
        tokens[2].Should().Be(new Token("\n", TokenType.NewLine)
            { StartIndex = 7, IsTag = true });
        tokens[3].Should().Be(new Token("hi", TokenType.Word)
            { StartIndex = 9, IsTag = false });
    }

    [Test]
    public void MdParser_ParseTag_BoldText()
    {
        var text = "__bold text__";

        var token = lexer.Tokenize(text);


        token[0].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 0, IsTag = true });
        token[1].Should().Be(new Token("bold", TokenType.Word) { StartIndex = 2 });
        token[2].Should().Be(new Token("text", TokenType.Word) { StartIndex = 7 });
        token[3].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 11, IsTag = true });
    }

    [Test]
    public void MdParser_ParseTag_DigitWithItalic() // 3 условие спецификаций
    {
        var text = "_1234_";

        var token = lexer.Tokenize(text);


        token[0].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 0 });
        token[1].Should().Be(new Token("1234", TokenType.Digit) { StartIndex = 1 });
        token[2].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 5});
    }

    [Test]
    public void MdParser_ParseTag_ItalicInWord() // 4 условие спецификаций
    {
        var text = "ou_ter_wild";

        var token = lexer.Tokenize(text);


        token[0].Should().Be(new Token("ou", TokenType.Word) { StartIndex = 0 });
        token[1].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 2, IsTag = true});
        token[2].Should().Be(new Token("ter", TokenType.Word) { StartIndex = 3});
        token[3].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 6, IsTag = true});
        token[4].Should().Be(new Token("wild", TokenType.Word) { StartIndex = 7});
    }

    [Test]
    public void MdParser_SingleItalicTag_ShouldBe_Text()
    {
        var text = "he_llo";

        var token = lexer.Tokenize(text);

        token[0].Should().Be(new Token("he", TokenType.Word) { StartIndex = 0 });
        token[1].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 2 });
        token[2].Should().Be(new Token("llo", TokenType.Word) { StartIndex = 3 });
    }

    [Test]
    public void MdParser_ParseTag_ItalicText()
    {
        var text = "_italic text_";

        var token = lexer.Tokenize(text);

        token[0].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 0, IsTag = true });
        token[1].Should().Be(new Token("italic", TokenType.Word) { StartIndex = 1 });
        token[2].Should().Be(new Token("text", TokenType.Word) { StartIndex = 8 });
        token[3].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 12, IsTag = true });
    }


    [Test]
    public void MdParser_ParseTags_ItalicInBold()
    {
        var text = "__bold _italic_ text__";

        var tokens = lexer.Tokenize(text);

        tokens[0].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token("bold", TokenType.Word) { StartIndex = 2 });
        tokens[2].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 7, IsTag = true });
        tokens[3].Should().Be(new Token("italic", TokenType.Word) { StartIndex = 8 });
        tokens[4].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 14, IsTag = true });
        tokens[5].Should().Be(new Token("text", TokenType.Word) { StartIndex = 16 });
        tokens[6].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 20, IsTag = true });
    }

    [Test]
    public void MdParser_ParseTags_ItalicInBold_WithComplexText()
    {
        var text = "_hi __bold t_ k__";

        var tokens = lexer.Tokenize(text);

        tokens[0].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 0 });
        tokens[1].Should().Be(new Token("hi", TokenType.Word) { StartIndex = 1 });
        tokens[2].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 4 });
        tokens[3].Should().Be(new Token("bold", TokenType.Word) { StartIndex = 6 });
        tokens[4].Should().Be(new Token("t", TokenType.Word) { StartIndex = 11 });
        tokens[5].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 12 });
        tokens[6].Should().Be(new Token("k", TokenType.Word) { StartIndex = 14 });
        tokens[7].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 15 });
    }

    [Test]
    public void MdParser_Lexer_IntersectBoldItalic()
    {
        var text = "__hel_";
    }

    [Test]
    public void MdParser_ParseTags_BoldWithStartItalic()
    {
        var text = "__bold_text__";

        var tokens = lexer.Tokenize(text);

        tokens[0].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token("bold", TokenType.Word) { StartIndex = 2 });
        tokens[2].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 6 });
        tokens[3].Should().Be(new Token("text", TokenType.Word) { StartIndex = 7 });
        tokens[4].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 11, IsTag = true });
    }

    [Test]
    public void MdParser_ParseTags_BoldInItalic()
    {
        var text = "_italic __bold__ text_"; // тест не проходит, ибо я пока не очень понимаю, как считывать, кто во что вложен.

        var tokens = lexer.Tokenize(text);

        tokens[0].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token("italic", TokenType.Word) { StartIndex = 1 });
        tokens[2].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 8 });
        tokens[3].Should().Be(new Token("bold", TokenType.Word) { StartIndex = 10 });
        tokens[4].Should().Be(new Token("__", TokenType.Bold) { StartIndex = 14 });
        tokens[5].Should().Be(new Token("text", TokenType.Word) { StartIndex = 17 });
        tokens[6].Should().Be(new Token("_", TokenType.Italic) { StartIndex = 21, IsTag = true });
    }

    [Test]
    public void MdParser_ParseTags_BoldIn()
    {
    }

    [TestCaseSource(nameof(FirstLineTestCases))]
    public void MdParser_ParseTags_OnFirstLine(string text, Token expectedToken)
    {
        lexer.Tokenize(text)[0].Should().Be(expectedToken);
    }


    public static IEnumerable<TestCaseData> FirstLineTestCases()
    {
        yield return new TestCaseData("# ",
                new Token("# ", TokenType.Header) { StartIndex = 0, IsTag = true })
            .SetName("Header With Single Hash");

        yield return new TestCaseData("_ ", new Token("_", TokenType.Italic) { StartIndex = 0 })
            .SetName("Header With newLine after Space");

        yield return new TestCaseData("__", new Token("__", TokenType.Bold) { StartIndex = 0 })
            .SetName("Header With newLine without Space");

        yield return new TestCaseData("OuterWild", new Token("OuterWild", TokenType.Word) { StartIndex = 0 })
            .SetName("Header With newLine without Space");

        yield return new TestCaseData("1312", new Token("1312", TokenType.Digit) { StartIndex = 0 })
            .SetName("Header With newLine without Space");
    }
}