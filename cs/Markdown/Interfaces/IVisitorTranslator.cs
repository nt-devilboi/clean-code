namespace Markdown;

// как будто никто не мешает нам сделать два визитора, один будет из node делать string в другом формате(трансляция). а другой может просто из один node делать другие node другого формата. потенциально на будущие пусть будет, но в этой задаче такой цели такой нету
public interface IVisitorTranslator
{
    string Translate(ElementNodeNewLine node);
}