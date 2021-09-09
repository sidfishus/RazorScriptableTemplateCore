using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorScriptableTemplateCore
{
    public static class Helpers
    {
        public static IList<string> GetListOfAssembliesForType<TYPE>()
        {
            var thisTypeInfo = typeof(TYPE);
            var rv = new List<string>();
            rv.Add(thisTypeInfo.Assembly.Location);

            return rv;
        }
    }
}
