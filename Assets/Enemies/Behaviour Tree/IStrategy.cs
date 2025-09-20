
namespace BehaviourTreeNamespace
{
    public interface IStrategy
    {
        Status Process();
        void Reset();
    }
}
