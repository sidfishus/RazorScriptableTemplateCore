using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorScriptableTemplateCore
{
    public abstract class TemplateWriter<MODEL>
    {

        internal readonly StringBuilder _Output = new StringBuilder();

        public MODEL Model
        {
            get;
            internal set;
        }

        protected void WriteLiteral(string literal)
        {
            _Output.Append(literal);
        }

        protected void Write(object obj)
        {
            _Output.Append(obj);
        }

        public abstract Task ExecuteAsync();

        internal string ExecuteInternal(MODEL model)
        {
            Model = model;
            _Output.Clear();

            // Lies! this does not execute asynchronously!
            ExecuteAsync();

            return _Output.ToString();
        }
    }
}
