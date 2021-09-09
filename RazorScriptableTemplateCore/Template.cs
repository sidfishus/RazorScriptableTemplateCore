﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.AspNetCore.Razor.Hosting;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

//sidtodo credits: https://stackoverflow.com/questions/38247080/using-razor-outside-of-mvc-in-net-core
namespace RazorScriptableTemplateCore
{
    public class Template<MODEL> : IDisposable
    {
        TemplateWriter<MODEL> _Writer;
        AssemblyLoadContext _AssemblyLoadContext;

        public Template(string templatePath,
            string templateFilename,
            IEnumerable<string> dynamicAssemblies)
        {
            var fs = RazorProjectFileSystem.Create(templatePath);

            // customize the default engine a little bit
            var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs, (builder) =>
            {
                // InheritsDirective.Register(builder); // in .NET core 3.1, compatibility has been broken (again), and this is not needed anymore...
                builder.SetNamespace("RazorTemplate"); // define a namespace for the Template class
            });

            // Loads the template.

            var item = fs.GetItem(templateFilename, null);

            // parse and generate C# code
            var codeDocument = engine.Process(item);
            var cs = codeDocument.GetCSharpDocument();

            // outputs it on the console
            //Console.WriteLine(cs.GeneratedCode);

            // now, use roslyn, parse the C# code
            var tree = CSharpSyntaxTree.ParseText(cs.GeneratedCode);

            var fixedReferences = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // include corlib
                MetadataReference.CreateFromFile(typeof(RazorCompiledItemAttribute).Assembly.Location), // include Microsoft.AspNetCore.Razor.Runtime
                MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location), // this DLL (that contains the MyTemplate base class)

                // for some reason on .NET core, I need to add this... this is not needed with .NET framework
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),

                // as found out by @Isantipov, for some other reason on .NET Core for Mac and Linux, we need to add this... this is not needed with .NET framework
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll"))
            };

            var fullAssemblyList = new List<PortableExecutableReference>();
            fullAssemblyList.AddRange(fixedReferences);
            fullAssemblyList.AddRange(dynamicAssemblies.Select(iterLocation =>
            {
                return MetadataReference.CreateFromFile(iterLocation);
            }));

            var assemblyFn = Path.GetRandomFileName();

            // define the dll
            var compilation = CSharpCompilation.Create(assemblyFn, new[] { tree },fullAssemblyList,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // Compile and emit to a stream
            var memoryStream = new MemoryStream();
            var result = compilation.Emit(memoryStream);
            if (!result.Success)
            {
                throw new Exception(string.Join(Environment.NewLine, result.Diagnostics));
            }
            // Reset the position - important otherwise loading the assembly from a stream will cause it to break.
            memoryStream.Position = 0;

            _AssemblyLoadContext = new AssemblyLoadContext(null, true);
            var assembly = _AssemblyLoadContext.LoadFromStream(memoryStream);

            // Not needed
            memoryStream.Dispose();

            // the generated type is defined in our custom namespace, as we asked. "Template" is the type name that razor uses by default.
            _Writer = (TemplateWriter<MODEL>)Activator.CreateInstance(assembly.GetType("RazorTemplate.Template"));
        }

        public string Execute(MODEL model)
        {
            return _Writer.ExecuteInternal(model);
        }

        void IDisposable.Dispose()
        {
            // This helps the generated assembly be unloaded. It's a work around for the fact that you can't
            // unload assemblies directly in .NET 5.0 because secondary app domains are not supported:
            // https://stackoverflow.com/questions/27266907/no-appdomains-in-net-core-why
            _Writer = null;
            if (_AssemblyLoadContext != null)
            {
                _AssemblyLoadContext.Unload();
                _AssemblyLoadContext = null;
            }
            //
        }
    }

}