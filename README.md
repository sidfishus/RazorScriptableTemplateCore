# RazorScriptableTemplateCore

Big shout outs to Simon Mourier for this: https://stackoverflow.com/questions/38247080/using-razor-outside-of-mvc-in-net-core.

I have taken Simon's work to leverage the Razor SDK to create templates outside of web pages and made it in to a library so you can do this in .NET5:

```
using (var template = new Template<MyModel>(".", "razorTemplate.rt",
  TemplateHelpers.GetListOfAssembliesForTypeRecursive<MyModel>())
)
{
  var model = new MyModel();
  model.Name = iterName;
  model.NestedClass =
    new TestClasslib.Class1 { Property = "Test" };
  model.NestedClass.AggregatedProperty =
    new TestClasslib2.Class1 { Property = "Test classlib2" };
  model.NestedClass.ListOfAggregatedProperty = new List<TestClasslib3.Class1>();
  model.NestedClass.ListOfAggregatedProperty.Add(
    new TestClasslib3.Class1 { Property="Aggregated Property from index 0" }
  );
  var output = template.Execute(model);
  Console.WriteLine(output);
}
                            
```

I've improved the way it emits and then loads the dynamically generated assembly by using a stream instead of a physical file, and found a way to unload the assemblies once the templates they were generated for via Dispose.
