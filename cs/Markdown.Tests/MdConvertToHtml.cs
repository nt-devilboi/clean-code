using FluentAssertions;
namespace Markdown.Tests;

public class MdConvertToHtml
{
    public IMd Parser = new Md();

        // нету 5 и 6 и один тест сейчас не проходит
    [TestCase("# hello", "<h1>hello</h1>", TestName = "HeaderWithoutNewLine")]
    [TestCase("# hello\n hi", "<h1>hello</h1>\n hi", TestName = "HeaderWithNewLine")]
    [TestCase("# _hello_", "<h1><em>hello</em></h1>", TestName = "italicInHeader")]
    [TestCase("_hello_", "<em>hello</em>")]
    [TestCase("# hel_lo", "<h1>hel_lo</h1>")]
    [TestCase("__hello__", "<strong>hello</strong>")]
    [TestCase("__bold _italic_ text__", "<strong>bold <em>italic</em> text</strong>")]
    [TestCase("# __bold _italic_ text__", "<h1><strong>bold <em>italic</em> text</strong></h1>")] // условие 1
    [TestCase("# __bold _italic_ text__\n", "<h1><strong>bold <em>italic</em> text</strong></h1>\n")]
    [TestCase("____", "____")] // условие 10
    [TestCase("__", "__")] // условие 10
    [TestCase("_hi __bold__ t_", "<em>hi __bold__ t</em>")] // условие 2
    [TestCase("_hi __bold t_ k__", "_hi __bold t_ k__", TestName = "intersect")]  // условие 9
    [TestCase("_ outer_", "_ outer_", TestName = "_ outer_ -> _ outer_")] // условие 7
    [TestCase("_outer _", "_outer _", TestName = "_outer _ -> _outer _")] // условие 8
    [TestCase("_outer\n_", "_outer\n_", TestName = "spaceAfterItalicSymbol")] // условие 9
    [TestCase("ou_t_er", "ou<em>t</em>er", TestName = "ou_t_er -> ou<em>t</em>er")] // условие 4
    [TestCase("_3231_", "_3231_", TestName = "_3231_ -> _3231_")] // условие 3
    [TestCase("__3231__", "__3231__", TestName = "__3231__ -> __3231__")] // условие 3
    [TestCase(@"\\", @"\", TestName = @"\\ -> \")] 
    [TestCase(@"\__Hello__", "__Hello__", TestName = @"\__Hello__ -> __Hello__")] 
    [TestCase(@"\_Hello_", "_Hello_", TestName = @"\_Hello_ -> _Hello_")] 
    [TestCase(@"\\_Hello_", @"\<em>Hello</em>", TestName = @"\\_Hello_ -> \<em>Hello</em>")] 
    public void MdParser_Render_ShouldProduceCorrectHtml(string input, string expectedOutput)
    {
        Parser.Render(input).Should().Be(expectedOutput);
    }
}