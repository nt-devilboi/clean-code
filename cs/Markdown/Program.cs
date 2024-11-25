// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;

var list = new List<int>() { 1, 2, 3, 4 };

var imut = ImmutableList.CreateRange(list);
var readOnly = list.AsReadOnly();
list[2] = 10;

Console.WriteLine(imut[2]);
// будет 10, хоть он и реад онли, это всё потому, что он работает с тем же самым листом, который был изначально, но запрещает менять его. но никто не мешает менять изначальный лист. 
// мы как бы в данном случае положили массив под стекло из него нельзя никак менять сам массив.
// тогда как в случае c token мы полностью передивыем все данные из массив в новый массив, который уже никак нельзя менять, не добавлять новые элементы не менять данные внутри тех элементов, которыех хранятся внутри этого массива.
Console.WriteLine(readOnly[2]);

imut = imut.Add(3);