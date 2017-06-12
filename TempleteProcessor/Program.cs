using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TempleteProcessor
{
    class Program
    {

        static void Main(string[] args)
        {
            var parser = new TemplateParser();
            var template = parser.ParseTemplate(File.ReadAllText("template1.txt"));
            var content = template.Evaluate(new GlobalParams() { X = 101, Y = 201});
            Console.WriteLine(content);

            Console.ReadLine();
        }

    }

    public class GlobalParams
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}