namespace Genes
{
    public enum SignalType
    {
        Input,
        Output,
    }


    public class Signal : Connectable
    {
        public SignalType type;
        public int id;
        public Signal(SignalType type, int id)
        {
            this.type = type;
            this.id = id;
            Operator = type == SignalType.Output ? ConnectableOperator.Sigmoid : ConnectableOperator.Identity;
        }

        public Signal(Signal other)
        {
            InputValue = other.InputValue;
            OutputValue = other.OutputValue;
            id = other.id;
            type = other.type;
        }
        
        public override string ToString()
        {
            return $"Signal (id: {id})";
        }
        
    }
}