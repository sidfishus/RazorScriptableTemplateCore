using System;
using System.Collections.Generic;

namespace TestClasslib
{
    public class Class1
    {
        public string Property
        {
            get;
            set;
        }

        // This is in another DLL..
        public TestClasslib2.Class1 AggregatedProperty
        {
            get;
            set;
        }

        // This is in another DLL..
        public IList<TestClasslib3.Class1> ListOfAggregatedProperty
        {
            get;
            set;
        }

        public IList<int> ListOfInt
        {
            get;
            set;
        }
    }

    public class Class2<T>
	{
        public T Property
        {
            get;
            set;
        }
	}
}
