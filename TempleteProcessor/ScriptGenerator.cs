using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TempleteProcessor
{
    public class ScriptGenerator
    {
        enum Token{
            /// <summary>
            /// normal text
            /// </summary>
            TextBlock,
            /// <summary>
            /// <#@ import namespace="" #>
            /// </summary>
            Directives,
            /// <summary>
            /// <# C# code #>
            /// </summary>
            CodeBlock,
            /// <summary>
            /// <#= expression #>
            /// </summary>
            ExpressionBlock,
            /// <summary>
            /// <#+ expression #>
            /// </summary>
            ClassFeatureBlock
        }
        int currentChar;
        System.IO.StreamReader reader;
        StringWriter scriptWriter;
        StringBuilder buffer;
        public string Generate(string templateFilename)
        {
            scriptWriter = new StringWriter();
            buffer = new StringBuilder();
            using(reader = new System.IO.StreamReader(new FileStream(templateFilename, FileMode.Open, FileAccess.Read)))
            {
                while(!reader.EndOfStream)
                {
                    var token = GetToken();
                    switch(token){
                        case Token.TextBlock:
                            buffer.Append((char)currentChar);
                            if(currentChar == '\n'){
                                FlushTextBuffer();
                            }
                            break;
                        case Token.Directives:
                            if(buffer.Length > 0){
                                FlushTextBuffer();
                            }
                            ReadDirectives();
                            break;
                        case Token.ExpressionBlock:
                            if (buffer.Length > 0)
                            {
                                FlushTextBuffer();
                            }
                            ReadExpressionBlock();
                            break;
                        case Token.ClassFeatureBlock:
                            if (buffer.Length > 0)
                            {
                                FlushTextBuffer();
                            }
                            ReadClassFeatureBlock();
                            break;
                        case Token.CodeBlock:
                            if (buffer.Length > 0)
                            {
                                FlushTextBuffer();
                            }
                            ReadCodeBlock();
                            break;
                    }
                }
            }

            return scriptWriter.ToString();
        }

        Token GetToken(){
            currentChar = reader.Read();
            if (currentChar == '<')
            {
                var c = reader.Peek();
                if(c == '#'){
                    reader.Read();
                    c = reader.Peek();
                    if(c == '@'){
                        reader.Read();
                        reader.Read();
                        return Token.Directives;
                    }
                    if (c == '=')
                    {
                        reader.Read();
                        reader.Read();
                        return Token.ExpressionBlock;

                    }
                    if (c == '+')
                    {
                        reader.Read();
                        reader.Read();
                        return Token.ClassFeatureBlock;
                    }
                    if (c == ' ' || c == '\r' || c == '\n' || c == '\t')
                    {
                        if(c == ' '){
                            reader.Read();
                        }
                        return Token.CodeBlock;
                    }
                }
            }
            return Token.TextBlock;
        }
        void FlushTextBuffer(){
            WriteIndent();
            scriptWriter.WriteLine(@"System.Console.Write(""{0}"");", buffer.Replace("\r","\\r").Replace("\n","\\n") .ToString());
            buffer.Clear();
        }
        void ReadDirectives(){
            scriptWriter.Write(@"Directive ");

            var endOfDirective = false;
            buffer.Clear();            
            while(!reader.EndOfStream){
                if(endOfDirective){
                    var c = reader.Peek();
                    if (c != ' ' && c != '\r' && c != '\n')
                    {
                        break;
                    }
                    reader.Read();
                    continue;
                }
                if(ReadBlock()){
                    if(currentChar == '\n'){
                        scriptWriter.WriteLine(buffer.ToString());
                        buffer.Clear();
                    }
                }
                else{
                    endOfDirective = true;
                }
            }
            if(buffer.Length > 0){
                scriptWriter.WriteLine(buffer);
                buffer.Clear();
            }

        }
        void ReadCodeBlock()
        {
            WriteIndent();
            scriptWriter.WriteLine(@"// Code");

            buffer.Clear();
            while (!reader.EndOfStream)
            {
                if (ReadBlock())
                {
                    if (currentChar == '\n')
                    {
                        scriptWriter.Write(buffer);
                        buffer.Clear();
                        continue;
                    }
                }
                else
                {
                    break;
                }
            }
            if (buffer.Length > 0)
            {
                scriptWriter.WriteLine(buffer);
                buffer.Clear();
            }

        }
        void ReadExpressionBlock()
        {
            WriteIndent();
            scriptWriter.WriteLine(@"// Expression");

            buffer.Clear();
            while (!reader.EndOfStream)
            {
                if (ReadBlock())
                {
                }
                else
                {
                    break;
                }
            }
            if (buffer.Length > 0)
            {
                WriteIndent();
                scriptWriter.WriteLine("System.Console.Write({0})", buffer);
                buffer.Clear();
            }

        }
        void ReadClassFeatureBlock()
        {
            scriptWriter.WriteLine(@"// Class Feature");

            buffer.Clear();
            while (!reader.EndOfStream)
            {
                if (ReadBlock())
                {
                    if (currentChar == '\n')
                    {
                        scriptWriter.WriteLine("System.Console.WriteLine({0})", buffer);
                        buffer.Clear();
                    }
                }
                else
                {
                    break;
                }
            }
            if (buffer.Length > 0)
            {
                scriptWriter.WriteLine("System.Console.WriteLine({0})", buffer);
                buffer.Clear();
            }
        }
        bool ReadBlock(){
            currentChar = reader.Peek();
            if (currentChar == '#')
            {
                reader.Read();
                var c2 = reader.Peek();
                if (c2 == '>')
                {
                    reader.Read();
                    return false;
                }
                buffer.Append((char)currentChar);
                currentChar = reader.Read();
                buffer.Append((char)currentChar);
                return true;
            }
            buffer.Append((char)currentChar);
            reader.Read();
            return true;
        }
        void WriteIndent(){
            scriptWriter.Write(new string(' ', 4));
        }
    }
}
