
using System.Collections.Generic;

namespace BehaviourTreeNamespace
{
    public enum Status { Running, Success, Failure }

    public class BehaviourNode
    {
        public readonly string Name;

        public readonly List<BehaviourNode> children = new();
        protected int currentChild;

        public BehaviourNode(string name)
        {
            Name = name;
        }

        public void AddChild(BehaviourNode child)
        {
            children.Add(child);
        }

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset()
        {
            currentChild = 0;
            foreach (var child in children)
                child.Reset();
        }
    }

    public class LeafNode : BehaviourNode
    {
        readonly IStrategy strategy;

        public LeafNode(string name, IStrategy strategy) : base(name)
        {
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();
        public override void Reset() => strategy.Reset();
    }

    public class BehaviourTree : BehaviourNode
    {
        public BehaviourTree(string name) : base(name) { }

        public override Status Process()
        {
            while (currentChild < children.Count)
            {
                var status = children[currentChild].Process();
                if (status != Status.Success)
                    return status;
                currentChild++;
            }
            return Status.Success;
        }
    }
}