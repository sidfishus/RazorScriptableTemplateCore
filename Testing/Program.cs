using System;
using RazorScriptableTemplateCore;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

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

        string _PrivateVariable="";

        public string GetPrivateVariable()
        {
            return _PrivateVariable;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                DateTime startTs = DateTime.Now;
                DateTime lastPrintTs = DateTime.Now;
                const int COUNT = 1;
                for (int i = 0; i < COUNT; ++i)
                {
                    using (var template = new Template<MyModel>(".", "razorTemplate.rt",
                        TemplateHelpers.GetListOfAssembliesForTypeRecursive<MyModel>())
                    )
                    //using (var template = new Template<MyModel>(".", "razorTemplate.rt",
                    //    new[]{ TemplateHelpers.GetLocationForType<MyModel>(),
                    //        TemplateHelpers.GetLocationForType<TestClasslib.Class1>() })
                    //)
                    {

                        string[] names = new string[] { "Chris", "John", "Sam" };

                        Array.ForEach(names, iterName =>
                        {
                            var model = new MyModel();
                            model.Name = iterName;
                            model.NestedClass = new TestClasslib.Class1 { Property = "Test" };
                            model.NestedClass.AggregatedProperty = new TestClasslib2.Class1 { Property = "Test classlib2" };
                            model.NestedClass.ListOfAggregatedProperty = new List<TestClasslib3.Class1>();
                            model.NestedClass.ListOfAggregatedProperty.Add(new TestClasslib3.Class1 { Property="Aggregated Property from index 0" });
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
