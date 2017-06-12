using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Sprache;
namespace TempleteProcessor
{
    class Block
    {
        public virtual string GetTypeName()
        {
            return this.GetType().Name;
        }
        public override string ToString()
        {
            return "";
        }
        public virtual void WriteScript(TextWriter writer)
        {

        }
    }
    class DirectiveBlock : Block
    {
        public override void WriteScript(TextWriter writer)
        {

        }
    }
    class AssemblyDirectiveBlock : DirectiveBlock
    {
        public string AssemblyName { get; set; }
        public override string ToString()
        {
            return "assembly:" + AssemblyName;
        }
        public override void WriteScript(TextWriter writer)
        {
            writer.WriteLine(@"#r ""{0}""", AssemblyName);
        }
    }
    class ImportDirectiveBlock : DirectiveBlock
    {
        public string NamespaceName { get; set; }

        public override string ToString()
        {
            return "import:" + NamespaceName;
        }
        public override void WriteScript(TextWriter writer)
        {
            writer.WriteLine(@"using  {0};", NamespaceName);

        }
    }
    class OutputDirectiveBlock : DirectiveBlock
    {
        public string Extention { get; set; }

        public override string ToString()
        {
            return "output:" + Extention;
        }
        public override void WriteScript(TextWriter writer)
        {

        }
    }
    class ControlBlock : Block
    {
        public string Content { get; set; }

        public override string ToString()
        {
            return "control:" + Content;
        }
        public override void WriteScript(TextWriter writer)
        {

        }
    }
    class StandardControlBlock : ControlBlock
    {
        public override string ToString()
        {
            return "code:" + Content;
        }
        public override void WriteScript(TextWriter writer)
        {
            writer.Write(Content);

        }
    }
    class ExpressionControlBlock : ControlBlock
    {
        public override string ToString()
        {
            return "expression:" + Content;
        }
        public override void WriteScript(TextWriter writer)
        {
            writer.WriteLine("writer.Write({0});", Content);

        }
    }
    class ClassControlBlock : ControlBlock
    {
        public override string ToString()
        {
            return "class feature:" + Content;
        }
        public override void WriteScript(TextWriter writer)
        {
            writer.Write(Content);
        }
    }
    class TextBlock : Block
    {
        public List<string> TextLines { get; set; }
        public TextBlock()
        {

        }
        public TextBlock(IEnumerable<string> content)
        {
            TextLines = new List<string>();
            TextLines.AddRange(content);
        }
        public override string ToString()
        {
            string text = "";
            foreach (var line in TextLines)
            {
                text += "text:" + line.Replace("\r", "\\r").Replace("\n", "\\n");
            }
            return text;
        }

        public override void WriteScript(TextWriter writer)
        {
            foreach (var line in TextLines)
            {
                writer.WriteLine(@"writer.Write(""{0}"");", line.Replace("\r", "\\r").Replace("\n", "\\n"));
            }

        }
    }
    class TemplateParser
    {
        static Parser<IEnumerable<char>> StandardControlBlockStart = Parse.String("<#");
        static Parser<IEnumerable<char>> DirectiveBlockStart = Parse.String("<#@");
        static Parser<IEnumerable<char>> ExpressionControlBlockStart = Parse.String("<#=");
        static Parser<IEnumerable<char>> ClassControlBlockStart = Parse.String("<#+");

        static Parser<IEnumerable<char>> ControlBlockStart =
            DirectiveBlockStart
            .Or(ExpressionControlBlockStart)
            .Or(ClassControlBlockStart)
            .Or(StandardControlBlockStart)
            ;
        static Parser<IEnumerable<char>> ControlBlockEnd = Parse.String("#>");

        static Parser<IEnumerable<char>> QuotedSymbol =
            from quoteStart in Parse.Char('"')
            from symbol in Parse.AnyChar.Except(Parse.Char('"')).Many().Text()
            from quoteEnd in Parse.Char('"')
            select symbol;

        static Parser<Block> AssemblyDirectiveContent =
            from identifier in Parse.String("assembly")
            from delimiter in Parse.WhiteSpace.Many()
            from prefix in Parse.String("name")
            from assign in Parse.String("=")
            from name in QuotedSymbol.Text()
            select new AssemblyDirectiveBlock() { AssemblyName = name };

        static Parser<Block> OutputDirectiveContent =
            from identifier in Parse.String("output")
            from delimiter in Parse.WhiteSpace.Many()
            from prefix in Parse.String("extension")
            from assign in Parse.String("=")
            from name in QuotedSymbol.Text()
            select new OutputDirectiveBlock() { Extention = name };

        static Parser<Block> ImportDirectiveContent =
            from identifier in Parse.String("import")
            from delimiter in Parse.WhiteSpace.Many()
            from prefix in Parse.String("namespace")
            from assign in Parse.String("=")
            from name in QuotedSymbol.Text()
            select new ImportDirectiveBlock() { NamespaceName = name };
        static Parser<Block> DirectiveBlockContent =
            AssemblyDirectiveContent
            .Or(OutputDirectiveContent)
            .Or(ImportDirectiveContent)
            ;

        static Parser<Block> DirectiveBlock =
            from blockStart in DirectiveBlockStart
            from space in Parse.WhiteSpace.Many()
            from content in DirectiveBlockContent
            from space2 in Parse.WhiteSpace.Many()
            from blockEnd in ControlBlockEnd
            from trailing in Parse.WhiteSpace.Many()
            select content;

        static Parser<string> ControlBlockContent = Parse.AnyChar.Except(ControlBlockEnd).Many().Text();
        static Parser<Block> StandardControlBlock =
            from blockStart in StandardControlBlockStart
            from blockContent in ControlBlockContent
            from blockEnd in ControlBlockEnd
            select new StandardControlBlock() { Content = blockContent };
        static Parser<Block> ClassControlBlock =
            from blockStart in ClassControlBlockStart
            from blockContent in ControlBlockContent
            from blockEnd in ControlBlockEnd
            select new ClassControlBlock() { Content = blockContent };
        static Parser<Block> ExpressionControlBlock =
            from blockStart in ExpressionControlBlockStart
            from blockContent in ControlBlockContent
            from blockEnd in ControlBlockEnd
            select new ExpressionControlBlock() { Content = blockContent };

        static Parser<Block> ControlBlock =
            ExpressionControlBlock
            .Or(DirectiveBlock)
            .Or(ClassControlBlock)
            .Or(StandardControlBlock)
            ;
        static Parser<string> TextLineEndWithBlockStart = Parse.AnyChar.Except(ControlBlockStart).Many().Text();
        static Parser<string> TextLineEndWithNewLine = Parse.AnyChar.Except(ControlBlockStart).Except(Parse.LineTerminator).Many().Text().Select(x => x + Environment.NewLine);
        static Parser<IEnumerable<string>> TextContent =
            TextLineEndWithBlockStart.Or(TextLineEndWithNewLine).Many();

        static Parser<TextBlock> TextBlock =
            from content in TextContent
            select new TextBlock(content);

        static Parser<IEnumerable<Block>> Template = ControlBlock
            .Or(TextBlock)
            .Many()
        ;
        public Template ParseTemplate(string templateString)
        {
            var blocks = Template.Parse(templateString);

            using (var writer = new StringWriter())
            {
                foreach (var block in blocks.Where(x => x is DirectiveBlock))
                {
                    block.WriteScript(writer);
                }
                writer.WriteLine("using(var writer = new System.IO.StringWriter()){");
                foreach (var block in blocks.Where(x => !(x is DirectiveBlock)))
                {
                    block.WriteScript(writer);
                }
                writer.WriteLine("return writer.ToString();");
                writer.WriteLine("}");

                var template = new Template(writer.ToString());
                return template;
            }


        }
    }
}
