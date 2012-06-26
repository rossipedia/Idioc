using System;

namespace Idioc
{
    class TypeNotConstructableException : Exception
    {
        public TypeNotConstructableException(Type type) 
            : base(string.Format("The type {0} is not constructable. Either it is abstract, an interface, or has no public constructors", type.FullName))
        {
            // noop
        }
    }
}