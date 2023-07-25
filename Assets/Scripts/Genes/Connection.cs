namespace Genes
{
    public class Connection
    {
        public Connectable input;
        public Connectable output;
        public int id;
        public float weighting = 1.0f;
        public bool shouldBeRemoved = false;

        public override string ToString()
        {
            return $"Connection (Id: {id},In: {input}, Out: {output}, Weighting: {weighting})";
        }

        public Connection(Connection connection)
        {
            input = connection.input;
            output = connection.output;
            id = connection.id;
            weighting = connection.weighting;
        }

        public Connection(Connectable input, Connectable output, float weighting, int id)
        {
            this.input = input;
            this.output = output;
            this.id = id;
            this.weighting = weighting;
        }
    }
}