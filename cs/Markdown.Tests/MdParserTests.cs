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

    [Test]
    public void MdParser_ParseTag_HeaderWithText()
    {
        var tokens = lexer.Tokenize("# hello");

        tokens[0].Should().Be(new Token("# ", TokenType.Header) { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token("hello", TokenType.Word) { StartIndex = 2 });
    }


    [Test]
    public void MdParser_ParseTag_HeaderWithTextWithNewLine()
    {
        var tokens = lexer.Tokenize("# hello\n");

        tokens[0].Should().Be(new Token("# ", TokenType.Header)
            { StartIndex = 0, IsTag = true });
        tokens[1].Should().Be(new Token("hello", TokenType.Word) { StartIndex = 2 });
        tokens[2].Should().Be(new Token("\n", TokenType.NewLine)
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

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo([
            new Token("_", TokenType.Italic) { StartIndex = 0 },
            new Token("1234", TokenType.Digit) { StartIndex = 1 },
            new Token("_", TokenType.Italic) { StartIndex = 5 }
        ]);
    }

    [Test]
    public void MdParser_ParseTag_ItalicInWord()
    {
        var text = "ou_ter_wild";

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo([
            new Token("ou", TokenType.Word) { StartIndex = 0 },
            new Token("_", TokenType.Italic) { StartIndex = 2, IsTag = true },
            new Token("ter", TokenType.Word) { StartIndex = 3 },
            new Token("_", TokenType.Italic) { StartIndex = 6, IsTag = true },
            new Token("wild", TokenType.Word) { StartIndex = 7 }
        ]);
    }


    [Test]
    public void MdParser_SingleItalicTag_ShouldBe_Text()
    {
        var text = "he_llo";

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo([
            new Token("he", TokenType.Word) { StartIndex = 0 },
            new Token("_", TokenType.Italic) { StartIndex = 2, IsTag = false },
            new Token("llo", TokenType.Word) { StartIndex = 3 }
        ]);
    }

    [Test]
    public void MdParser_ParseTag_ItalicText()
    {
        var text = "_italic text_";

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo([
            new Token("_", TokenType.Italic) { StartIndex = 0, IsTag = true },
            new Token("italic", TokenType.Word) { StartIndex = 1 },
            new Token("text", TokenType.Word) { StartIndex = 8 },
            new Token("_", TokenType.Italic) { StartIndex = 12, IsTag = true }
        ]);
    }


    [Test]
    public void MdParser_ParseTags_ItalicInsideBold()
    {
        var text = "__bold _italic_ text__";

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo([
            new Token("__", TokenType.Bold) { StartIndex = 0, IsTag = true },
            new Token("bold", TokenType.Word) { StartIndex = 2 },
            new Token("_", TokenType.Italic) { StartIndex = 7, IsTag = true },
            new Token("italic", TokenType.Word) { StartIndex = 8 },
            new Token("_", TokenType.Italic) { StartIndex = 14, IsTag = true },
            new Token("text", TokenType.Word) { StartIndex = 16 },
            new Token("__", TokenType.Bold) { StartIndex = 20, IsTag = true }
        ]);
    }


    [Test]
    public void MdParser_ParseTags_ItalicInBold_WithComplexText()
    {
        var text = "_hi __bold t_ k__";

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo([
            new Token("_", TokenType.Italic) { StartIndex = 0 },
            new Token("hi", TokenType.Word) { StartIndex = 1 },
            new Token("__", TokenType.Bold) { StartIndex = 4 },
            new Token("bold", TokenType.Word) { StartIndex = 6 },
            new Token("t", TokenType.Word) { StartIndex = 11 },
            new Token("_", TokenType.Italic) { StartIndex = 12 },
            new Token("k", TokenType.Word) { StartIndex = 14 },
            new Token("__", TokenType.Bold) { StartIndex = 15 }
        ]);
    }


    [Test]
    public void MdParser_ParseTags_BoldWithStartItalic()
    {
        var text = "__bold_text__";

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo([
            new Token("__", TokenType.Bold) { StartIndex = 0, IsTag = true },
            new Token("bold", TokenType.Word) { StartIndex = 2 },
            new Token("_", TokenType.Italic) { StartIndex = 6 },
            new Token("text", TokenType.Word) { StartIndex = 7 },
            new Token("__", TokenType.Bold) { StartIndex = 11, IsTag = true }
        ]);
    }


    [Test]
    public void MdParser_ParseTags_BoldInItalic()
    {
        var text = "_italic __bold__ text_"; // тест проверяет вложенные теги

        var tokens = lexer.Tokenize(text);

        tokens.Should().BeEquivalentTo(new[]
        {
            new Token("_", TokenType.Italic) { StartIndex = 0, IsTag = true },
            new Token("italic", TokenType.Word) { StartIndex = 1 },
            new Token("__", TokenType.Bold) { StartIndex = 8 },
            new Token("bold", TokenType.Word) { StartIndex = 10 },
            new Token("__", TokenType.Bold) { StartIndex = 14 },
            new Token("text", TokenType.Word) { StartIndex = 17 },
            new Token("_", TokenType.Italic) { StartIndex = 21, IsTag = true }
        });
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