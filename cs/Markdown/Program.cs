using Markdown;

var list = new List<int>() { 1, 2, 3, 4 };

var k = TokenTypeExtension.TokenTypes[char.IsDigit];

Console.WriteLine(k);