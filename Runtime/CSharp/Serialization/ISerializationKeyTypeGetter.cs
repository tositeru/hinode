
namespace Hinode.Serialization
{
    /// <summary>
    /// <see cref="ISerializer"/>
    /// </summary>
    public interface ISerializationKeyTypeGetter
    {
        System.Type Get(string key);
    }

    /// <summary>
    /// <see cref="ISerializer"/>
    /// </summary>
    public class PredicateSerializationKeyTypeGetter : ISerializationKeyTypeGetter
    {
        System.Func<string, System.Type> _predicate;

        public PredicateSerializationKeyTypeGetter(System.Func<string, System.Type> predicate)
        {
            _predicate = predicate;
        }

        public System.Type Get(string key)
        {
            return _predicate(key);
        }
    }
}
