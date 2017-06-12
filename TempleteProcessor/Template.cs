using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TempleteProcessor
{
    public class Template
    {
        string scriptCode;
        public Template(string code)
        {
            scriptCode = code;

        }

        public string Evaluate(object parameter = null)
        {
            Script<string> script = null;

            if (parameter == null)
            {
                script = CSharpScript.Create<string>(scriptCode);
                var task = script.RunAsync();
                task.Wait();
                return task.Result.ReturnValue.ToString();
            }
            else
            {
                script = CSharpScript.Create<string>(scriptCode, ScriptOptions.Default, parameter.GetType());
                var task = script.RunAsync(parameter);
                task.Wait();
                return task.Result.ReturnValue.ToString();
            }

        }
    }
}
