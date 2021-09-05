using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// Generic types will have the same guid even if the type parameter is different, which is the behaviour we want.
// Once we have collected the composed types for e.g. a IList<int>, we don't want to run the same algorithm
// for a IList<string>.
using UniqueTypeSet = System.Collections.Generic.HashSet<System.Guid>;
using UniqueStringSet = System.Collections.Generic.HashSet<string>;

namespace RazorScriptableTemplateCore
{

    public static class TemplateHelpers
    {
        // This works for:
        //  1. nested / containing types referenced in another assembly
        //  2. Generic properties where the type of the generic is another assembly
        // I can't think of any more examples.
        public static IList<string> GetListOfAssembliesForTypeRecursive<TYPE>()
        {
            // Types that have already had this work done for them. You don't want to build a list from the same type
            // multiple times.
            var typesDone = new UniqueTypeSet();

            // Unique list of assemblies
            var assembliesAdded = new UniqueStringSet();

            var thisType = typeof(TYPE);
            GetListOfAssembliesForTypeRecursive(thisType, typesDone, assembliesAdded);

            return assembliesAdded.ToList();
        }

        public static string GetLocationForType<TYPE>()
        {
            var thisType = typeof(TYPE);
            return thisType.Assembly.Location;
        }

        //////////////////////// ~Public
        static void GetListOfAssembliesForTypeRecursive(Type type, UniqueTypeSet typesDone,
            UniqueStringSet assembliesAdded)
        {
            typesDone.Add(type.GUID);
            assembliesAdded.Add(type.Assembly.Location);

            var thisTypeInfo = type.GetTypeInfo();

            // Add the contained/child property types
            GetListOfAssembliesForTypeList(
                thisTypeInfo.DeclaredFields, item => item.FieldType, typesDone, assembliesAdded
            );

            // Do the generic type arguments as well
            GetListOfAssembliesForTypeList(
                thisTypeInfo.GenericTypeArguments, item => item, typesDone, assembliesAdded
            );
        }

        // Helper for GetListOfAssembliesForTypeRecursive
        static void GetListOfAssembliesForTypeList<LITYPE>(IEnumerable<LITYPE> list, Func<LITYPE, Type> GetTypeFromItem,
            UniqueTypeSet typesDone, UniqueStringSet assembliesAdded)
        {
            foreach (var item in list)
            {
                var type = GetTypeFromItem(item);
                // Check we havn't already done the type
                if (!typesDone.Contains(type.GUID))
                {
                    GetListOfAssembliesForTypeRecursive(type, typesDone, assembliesAdded);
                }
            }
        }
    }
}
