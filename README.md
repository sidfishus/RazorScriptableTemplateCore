# RazorScriptableTemplateCore

Big shout outs to Simon Mourier for this: https://stackoverflow.com/questions/38247080/using-razor-outside-of-mvc-in-net-core.

I have taken Simon's work to leverage the Razor SDK to create templates outside of web pages and made it in to a library so you can do this in .NET5:

```
using (var template = new Template<MyModel>(".", "razorTemplate.rt",
  new[]{ TemplateHelpers.GetLocationForType<MyModel>(),
    TemplateHelpers.GetLocationForType<TestClasslib.Class1>() })
)
{
  var model = new MyModel();
  model.Name = iterName;
  model.NestedClass = new TestClasslib.Class1 { Property = "Test" };
  var output = template.Execute(model);
  Console.WriteLine(output);
}
                            
```

I'm still working on adding the ability to get the list of assemblies that a given type uses. For example when you have a model class that has a nested type which is in another assembly, add the ability to automatically get from the parent type to the nested/child assembly.
