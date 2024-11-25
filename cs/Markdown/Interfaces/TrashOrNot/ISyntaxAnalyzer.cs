using Markdown.Interfaces;

namespace Markdown;


// по сути, это будет паттерн фасад. внутри него будет скрыта логика взаимодейсвите других интерфейсов (сущностей).
// но как будто он не очень нужен по ощущению. но как будто удобный 
public interface ISyntaxAnalyzer
{
    IElementNode ParseText(string text, IParser parser);

    string Translate(IElementNode root,IVisitorTranslator translator);
}