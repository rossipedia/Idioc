using System;

namespace Idioc
{
    class TypeNotRegisteredException : Exception
    {
        public TypeNotRegisteredException(Type type) : base(string.Format("The Type {0} has not been registered with the container", type.FullName)) { }
    }
}