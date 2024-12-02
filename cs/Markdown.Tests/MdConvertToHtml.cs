using System.Collections;
using System.Diagnostics;
using FluentAssertions;

namespace Markdown.Tests;

public class MdConvertToHtml
{
    private IMd Parser = new Md();

    private static IEnumerable<TestCaseData> HeaderTestCases()
    {
        yield return new TestCaseData("# hello", "<h1>hello</h1>").SetName("HeaderWithoutNewLine");
        yield return new TestCaseData("# hel-lo\n hi", "<h1>hel-lo</h1>\n hi").SetName("HeaderWithNewLine");
        yield return new TestCaseData("# _hello_", "<h1><em>hello</em></h1>").SetName("ItalicInHeader");
    }

    private static IEnumerable<TestCaseData> SimpleTagTestCases()
    {
        yield return new TestCaseData("_hello_", "<em>hello</em>").SetName("ItalicText");
        yield return new TestCaseData("__hello__", "<strong>hello</strong>").SetName("BoldText");
    }

    private static IEnumerable<TestCaseData> NestedTagTestCases()
    {
        yield return new TestCaseData("__bold _italic_ text__", "<strong>bold <em>italic</em> text</strong>")
            .SetName("BoldWithItalicInside");
        yield return new TestCaseData("# __bold _italic_ text__", "<h1><strong>bold <em>italic</em> text</strong></h1>")
            .SetName("BoldInHeaderWithItalicInside");
        yield return new TestCaseData("# __bold _italic_ text__\n",
                "<h1><strong>bold <em>italic</em> text</strong></h1>\n")
            .SetName("BoldInHeaderWithNewLine");
    }

    private static IEnumerable<TestCaseData> SpecialCases()
    {
        yield return new TestCaseData("____", "____").SetName("DoubleUnderscore");
        yield return new TestCaseData("__", "__").SetName("SingleUnderscore");
        yield return new TestCaseData("_hi __bold__ t_", "<em>hi __bold__ t</em>").SetName("ItalicWithBoldInside");
    }

    private static IEnumerable<TestCaseData> IntersectionCases()
    {
        yield return new TestCaseData("_hi __bold t_ k__", "_hi __bold t_ k__").SetName("IntersectCase1");
        yield return new TestCaseData("__hi _bold t__ k_", "__hi _bold t__ k_").SetName("IntersectCase2");
    }

    private static IEnumerable<TestCaseData> NumericCases()
    {
        yield return new TestCaseData("hello_3231_", "hello_3231_").SetName("NumericInItalic");
        yield return new TestCaseData("outer__3231__", "outer__3231__").SetName("NumericInBold");
    }


    private static IEnumerable<TestCaseData> BackSlashCases()
    {
        yield return new TestCaseData(@"\\", @"\").SetName("BackslashEscape");
        yield return new TestCaseData(@"\__Hello__", "__Hello__").SetName("EscapedDoubleUnderscore");
        yield return new TestCaseData(@"\_Hello_", "_Hello_").SetName("EscapedUnderscore");
        yield return new TestCaseData(@"\\_Hello_", @"\<em>Hello</em>").SetName("EscapedWithItalic");
        yield return new TestCaseData(@"_Hello_ \_outerwile_", @"<em>Hello</em> _outerwile_").SetName(
            "EscapedItalicInCenter");
        yield return new TestCaseData(@"_Hello \_outerwile_", @"<em>Hello _outerwile</em>").SetName(
            "EscapedItalicBetweenItalic");
    }

    private static IEnumerable<TestCaseData> ComplexNestedTags()
    {
        yield return new TestCaseData(@"# _hello_ __how to play__ _with __my__ friend_",
                "<h1><em>hello</em> <strong>how to play</strong> <em>with __my__ friend</em></h1>")
            .SetName("ComplexHeaderWithTags");
    }

    private static IEnumerable<TestCaseData> UnderscoreEdgeCases()
    {
        yield return new TestCaseData("__ hello.__", "__ hello.__").SetName("Bold_ShouldBe_StartWhenNearSymbol");
        yield return new TestCaseData("s__ hello.__", "s__ hello.__").SetName("s__ hello.__ -> s__ hello.__");
        yield return new TestCaseData("__hello __", "__hello __").SetName("Bold_ShouldBe_EndWhenNearSymbol");
        yield return new TestCaseData("_ outer_", "_ outer_").SetName("Italic_ShouldBe_StartWhenNearSymbol");
        yield return new TestCaseData("_outer _", "_outer _").SetName("Italic_ShouldBe_EndWhenNearSymbol");
    }

    private static IEnumerable<TestCaseData> AdvancedComplexTestCases()
    {
        yield return new TestCaseData(
            @"# __This _is_ a__ _complex \_nested\_ _test__ with \#escapes__",
            "<h1><strong>This <em>is</em> a</strong> _complex _nested_ _test__ with #escapes__</h1>"
        ).SetName("ComplexNestedTagsWithEscapes");
    }

    [TestCaseSource(nameof(AdvancedComplexTestCases))]
    [TestCaseSource(nameof(HeaderTestCases))]
    [TestCaseSource(nameof(SimpleTagTestCases))]
    [TestCaseSource(nameof(NestedTagTestCases))]
    [TestCaseSource(nameof(SpecialCases))]
    [TestCaseSource(nameof(IntersectionCases))]
    [TestCaseSource(nameof(NumericCases))]
    [TestCaseSource(nameof(BackSlashCases))]
    [TestCaseSource(nameof(ComplexNestedTags))]
    [TestCaseSource(nameof(UnderscoreEdgeCases))]
    public void MdParser_Test(string input, string expected)
    {
        var result = Parser.Render(input);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void CheckUtf()
    {
        var unicodeSymbols = Enumerable.Range(0, 10000)
            .Select(char.ConvertFromUtf32).Where(x => x != "\\") // убрал его, так как это экраннирование
            .Aggregate((a, b) => a + b); // Собираем в строку


        foreach (var chunk in Chunk(unicodeSymbols, 1000))
        {
            Parser.Render(chunk).Should().Be(chunk);
        }
    }

    [Test]
    public void Algorithm_ShouldWorkInLinearTime()
    {
        var sizes = new[] { 10, 20, 40, 80, 160 };


        var timings = GetTimesTests(sizes);

        for (int i = 1; i < timings.Count; i++)
        {
            var ratio = timings[i] / timings[i - 1];

            ratio.Should()
                .BeLessThanOrEqualTo(2.5); // в два раза каждый следующий тест работает дольше (0.5 погрешность).
        }
    }

    private List<double> GetTimesTests(int[] sizes)
    {
        var timings = new List<double>();

        foreach (var size in sizes)
        {
            var input = GenerateInput(size);

            var stopwatch = Stopwatch.StartNew();
            Task.Run(() => Algorithm(input)); // чтоб в отедльном потоке была и скорость не зависела от других задач
            stopwatch.Stop();

            timings.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        return timings;
    }

    private string GenerateInput(int size)
    {
        return string.Concat(Enumerable.Repeat(@"# __This _is_ a__ _complex \_nested\_ _test__ with \#escapes__",
            size));
    }

    private void Algorithm(string input)
    {
        Parser.Render(input);
    }

    private static IEnumerable<string> Chunk(string input, int chunkSize)
    {
        for (int i = 0; i < input.Length; i += chunkSize)
        {
            yield return input.Substring(i, Math.Min(chunkSize, input.Length - i));
        }
    }
}