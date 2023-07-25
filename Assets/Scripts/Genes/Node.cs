namespace Genes
{
    public class Node : Connectable
    {
        public bool shouldBeDestroyed = false;
        public int id;
        public override string ToString()
        {
            return $"Node (Id:{id})";
        }

        public Node(Node other)
        {
            InputValue = other.InputValue;
            OutputValue = other.OutputValue;
            id = other.id;
            Operator = other.Operator;
        }

        public Node(int id)
        {
            this.id = id;
            Operator = SimulationManager.instance.GetRandomConnectableOperator();
        }
    }
}