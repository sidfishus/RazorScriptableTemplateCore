using System;
using System.Web.Razor;

// Credits to https://www.codemag.com/article/1103081/Leveraging-Razor-Templates-Outside-of-ASP.NET-They%E2%80%99re-Not-Just-for-HTML-Anymore!
namespace RazorScriptableTemplate
{

    //sidtodo change these?
    //sidtodo why is it abstract?
    //sidtodo name that applies it's the default type.
    abstract class OrderInfoTemplateBase
    {
        public abstract void Execute();

        public virtual void Write(object value)
        { /* TODO: Write value */ }

        public virtual void WriteLiteral(object value)
        { /* TODO: Write literal */ }
    }


    //sidtodo make it abstract?
    public static class Template
    {
        public static void ExecuteFromString(string templateText)
        {
            // The razor template syntax is C#.
            // I considered making this a parameter but decided against it, I have no interest in using VB .NET syntax.
            var language = new CSharpRazorCodeLanguage();
            //sidtodo change these.
            var host = new RazorEngineHost(language)
            {
                DefaultBaseClass = "OrderInfoTemplateBase",
                DefaultClassName = "OrderInfoTemplate",
                DefaultNamespace = "CompiledRazorTemplates",
            };

            // By default include the system namespace
            host.NamespaceImports.Add("System");

            RazorTemplateEngine engine = new RazorTemplateEngine(host);
            GeneratorResults razorResult = engine.GenerateCode(templateText);

        }
    }
}