using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorScriptableTemplateCore
{
    public static class TemplateHelpers
    {
        //sidtodo: This is unfinished.
        public static IList<string> GetListOfAssembliesForType<TYPE>()
        {
            var rv = new List<string>();
            rv.Add(GetLocationForType<TYPE>());

            return rv;
        }

        public static string GetLocationForType<TYPE>()
        {
            var thisTypeInfo = typeof(TYPE);
            return thisTypeInfo.Assembly.Location;
        }
    }
}
