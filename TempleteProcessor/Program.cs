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
            NewMethod();
        }

        private static void NewMethod1()
        {
            var generator = new ScriptGenerator();
            var script = generator.Generate("template1.txt");
            Console.WriteLine(script);
            Console.ReadLine();
        }

        private static void NewMethod()
        {
            var tempalte = File.ReadAllText("TextFile1.txt");
            var option = ScriptOptions.Default.AddImports("System");

            var script = CSharpScript.Create(tempalte, option, typeof(GlobalParams));

            var t =script.RunAsync(new GlobalParams { X = 10, Y = 2 });
            t.Wait();
            var returnvalue = t.Result.ReturnValue;
            Console.WriteLine(returnvalue);
            Console.ReadLine();
        }
    }

    public class GlobalParams
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}