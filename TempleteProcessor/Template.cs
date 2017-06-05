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
        private string _templateFilename;


        public Template(string templateFilename)
        {
            _templateFilename = templateFilename;
        }


    }
}
