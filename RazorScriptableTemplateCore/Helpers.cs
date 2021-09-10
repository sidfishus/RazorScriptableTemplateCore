using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorScriptableTemplateCore
{

    //sidtodo this needs to be recursive!! example if TestClasslib has types from a secondary class library

    public static class TemplateHelpers
    {
        //sidtodo: This is unfinished.
        public static IList<string> GetListOfAssembliesForTypeRecursive<TYPE>()
        {
            var rv = new List<string>();
            //sidtodo this needs to be unique.

            var thisType = typeof(TYPE);
            rv.Add(thisType.Assembly.Location);

            var thisTypeInfo = thisType.GetTypeInfo();

            // Add the contained/child property types
            foreach(var field in thisTypeInfo.DeclaredFields)
            {
                rv.Add(field.FieldType.Assembly.Location);
            }

            return rv;
        }

        public static string GetLocationForType<TYPE>()
        {
            var thisType = typeof(TYPE);
            return thisType.Assembly.Location;
        }
    }
}
