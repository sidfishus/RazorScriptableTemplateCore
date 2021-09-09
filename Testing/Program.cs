﻿using System;
using RazorScriptableTemplateCore;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace Testing
{

    public class MyModel
    {
        public string Name
        {
            get;
            set;
        }

        public TestClasslib.Class1 NestedClass
        {
            get;
            set;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {


            //sidtodo remove
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                Console.WriteLine(type.Name);
            }


            //foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            //{
            //    Assembly assembly = Assembly.Load(assemblyName);
            //    foreach (var type in assembly.GetTypes())
            //    {
            //        Console.WriteLine(type.Name);
            //    }
            //}

            try
            {
                DateTime startTs = DateTime.Now;
                DateTime lastPrintTs = DateTime.Now;
                const int COUNT = 1;
                for (int i = 0; i < COUNT; ++i)
                {
                    //TODO: auto get the list of associated assemblies
                    //using (var template = new Template<MyModel>(".", "razorTemplate.rt", Helpers.GetListOfAssembliesForType<MyModel>()))
                    using (var template = new Template<MyModel>(".", "razorTemplate.rt",
                        new[]{ TemplateHelpers.GetLocationForType<MyModel>(),
                            TemplateHelpers.GetLocationForType<TestClasslib.Class1>() })
                    )
                    {

                        string[] names = new string[] { "Chris", "John", "Sam" };

                        Array.ForEach(names, iterName =>
                        {
                            var model = new MyModel();
                            model.Name = iterName;
                            model.NestedClass = new TestClasslib.Class1 { Property = "Test" };
                            var output = template.Execute(model);
                            Console.WriteLine(output);
                        });
                    }

                    var now = DateTime.Now;
                    if((now-lastPrintTs).TotalMilliseconds > 3000)
                    {
                        lastPrintTs = now;
                        Console.WriteLine($"i={i}, seconds elapsed={(now- startTs).TotalSeconds}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadLine();
        }
    }
}
