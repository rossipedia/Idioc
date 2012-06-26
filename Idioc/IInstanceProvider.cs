using System.Linq.Expressions;

namespace Idioc
{
    public interface IInstanceProvider
    {
        object GetInstance();
        Expression Expression { get; }
    }
}