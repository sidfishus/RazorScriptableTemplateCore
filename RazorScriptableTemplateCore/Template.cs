using System;
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

// Credits to Simon Mourier: https://stackoverflow.com/questions/38247080/using-razor-outside-of-mvc-in-net-core
// Most of this is his. I have adapted it so it can be used as a library, and also improved the way it emits and
// then loads the assembly by using a stream instead of a physical file.
namespace RazorScriptableTemplateCore
{
    // Useage for example:
    // var template=new Template<YourModel>("C:\\temp", "yourRazorTemplate.temp",
    //  TemplateHelpers.GetListOfAssembliesForType<YourModel>());
    // var yourModel=new YourModel();
    // ...
    // var output=template.Execute(yourModel);
    //
    // The template file must contain '@inherits RazorScriptableTemplateCore.TemplateWriter<Testing.MyModel>'
    // at the top otherwise it will not work.
    public class Template<MODEL> : IDisposable
    {
        TemplateWriter<MODEL> _Writer;
        AssemblyLoadContext _AssemblyLoadContext;

        public Template(string templatePath,
            string templateFilename,
            IEnumerable<string> dynamicAssemblies)
        {
            
            // Compile and emit to a stream
            using (var memoryStream = new MemoryStream())
            {
                var compilation = PrepareRazorCompile(templatePath, templateFilename, dynamicAssemblies);
                var result = compilation.Emit(memoryStream);
                if (!result.Success)
                {
                    throw new Exception(string.Join(Environment.NewLine, result.Diagnostics));
                }
                // Reset the position - important otherwise loading the assembly from a stream will fail.
                memoryStream.Position = 0;

                // We load the assembly in a special way. The 'true' parameter specifies that the assembly can be
                // unloaded. See the 'Dispose' method for more details.
                _AssemblyLoadContext = new AssemblyLoadContext(null, true /* This is the key bit */);
                var assembly = _AssemblyLoadContext.LoadFromStream(memoryStream);

                // the generated type is defined in our custom namespace which we specified in 'builder.SetNamespace'.
                // "Template" is the type name that razor uses by default.
                _Writer = (TemplateWriter<MODEL>)Activator.CreateInstance(assembly.GetType("RazorTemplate.Template"));
            }
        }

        private static Compilation PrepareRazorCompile(string templatePath,string templateFilename,
            IEnumerable<string> dynamicAssemblies)
        {
            var fs = RazorProjectFileSystem.Create(templatePath);

            // customize the default engine a little bit
            var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs, (builder) =>
            {
                // in .NET core 3.1, compatibility has been broken (again), and this is not needed anymore...
                // InheritsDirective.Register(builder);
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

			var uniqueAssemblies = new HashSet<string>
			{
				typeof(object).Assembly.Location, // include corlib
                typeof(RazorCompiledItemAttribute).Assembly.Location, // include Microsoft.AspNetCore.Razor.Runtime
                Assembly.GetExecutingAssembly().Location, // this DLL (that contains the MyTemplate base class)

                // for some reason on .NET core, I need to add this... this is not needed with .NET framework
                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll"),

                // as found out by @Isantipov, for some other reason on .NET Core for Mac and Linux, we need to add this... this is not needed with .NET framework
                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll")
			};

            foreach (var assembly in dynamicAssemblies)
			{
                uniqueAssemblies.Add(assembly);
            }

            var metaDataReferenceList = uniqueAssemblies.Select(assembly =>
            {
                return MetadataReference.CreateFromFile(assembly);
            });

            var assemblyFn = Path.GetRandomFileName();

            // define the dll
            return CSharpCompilation.Create(assemblyFn, new[] { tree }, metaDataReferenceList,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        public string Execute(MODEL model)
        {
            return _Writer.ExecuteInternal(model);
        }

        void IDisposable.Dispose()
        {
            // This helps the generated assembly be unloaded and free up the resources.
            // It's a work around for the fact that you can't
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
