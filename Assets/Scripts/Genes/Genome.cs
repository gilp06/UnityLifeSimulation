using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Genes
{
    public enum GenomeMutationAction
    {
        AddConnection,
        RemoveConnection,
        AddNode,
        ChangeWeighting,
        ChangeNodeOperator,
        None,
    }

    public class Genome
    {
        private List<Signal> _inputSignals;
        private List<Signal> _outputSignals;
        private List<Connection> _connections;
        private List<Node> _nodes;

        private int _nodeCount = 0;
        private int _connectionCount = 0;
        public int generation;
        public Genome(int inputCount, int outputCount)
        {
            _inputSignals = new List<Signal>();
            _outputSignals = new List<Signal>();
            _connections = new List<Connection>();
            _nodes = new List<Node>();

            for (int i = 0; i < inputCount; i++)
            {
                _inputSignals.Add(new Signal(SignalType.Input, -(i+1)));
            }

            for (int i = 0; i < outputCount; i++)
            {
                _outputSignals.Add(new Signal(SignalType.Output, (i+1)));
            }

            generation = 1;
        }

        public Genome(Genome parent)
        {
            _inputSignals = new List<Signal>();
            _outputSignals = new List<Signal>();
            
            for (int i = 0; i < parent._inputSignals.Count; i++)
            {
                _inputSignals.Add(new Signal(SignalType.Input, -(i+1)));
            }

            for (int i = 0; i < parent._outputSignals.Count; i++)
            {
                _outputSignals.Add(new Signal(SignalType.Output, (i+1)));
            }

            _nodes = new List<Node>(parent._nodes.Count);
            parent._nodes.ForEach(node => _nodes.Add(new Node(node)));

            _connections = new List<Connection>(parent._connections.Count);
            foreach (var connection in parent._connections)
            {
                Connectable input = null, output = null;
                switch (connection.input)
                {
                    case Node node:
                    {
                        int index = parent._nodes.IndexOf(node);
                        input = _nodes[index];
                        break;
                    }
                    case Signal signal when Mathf.Abs(signal.id) == signal.id:
                        input = _outputSignals[signal.id - 1];
                        break;
                    case Signal signal:
                        input = _inputSignals[-1 * signal.id - 1];
                        break;
                }
                
                switch (connection.output)
                {
                    case Node node:
                    {
                        int index = parent._nodes.IndexOf(node);
                        output = _nodes[index];
                        break;
                    }
                    case Signal signal when Mathf.Abs(signal.id) == signal.id:
                        output = _outputSignals[signal.id - 1];
                        break;
                    case Signal signal:
                        output = _inputSignals[-1 * signal.id - 1];
                        break;
                }
                _connections.Add(new Connection(input, output, connection.weighting, _connectionCount));
                _connectionCount++;
            }

            generation = parent.generation + 1;
        }

        public void MutateAddConnection()
        {
            List<Connectable> inputs = new List<Connectable>();
            inputs.AddRange(_inputSignals);
            inputs.AddRange(_nodes);

            List<Connectable> outputs = new List<Connectable>();
            outputs.AddRange(_outputSignals);
            outputs.AddRange(_nodes);
        
            Connectable input = inputs[Random.Range(0, inputs.Count)];
            Connectable output = outputs[Random.Range(0, outputs.Count)];

            if (input.Equals(output))
            {
                return;
            }
        
            foreach (var connection in _connections)
            {
                if (connection.input == input && connection.output == output)
                {
                    return;
                }
            }
        
            _connections.Add(new Connection(input, output, Random.Range(-20.0f, 20.0f), _connectionCount));
            _connectionCount++;
        }
        

        public void MutateRemoveConnection()
        {
            if (_connections.Count == 0) return;
            Connection connectionToRemove = _connections[Random.Range(0, _connections.Count)];
            
            GetConnectionsToRemove(connectionToRemove);
            RemoveConnections();
            RemoveUnusedNodes();
        }

        public void MutateChangeWeighting()
        {
            if (_connections.Count == 0) return;
            Connection connectionToChange = _connections[Random.Range(0, _connections.Count)];
            connectionToChange.weighting += Random.Range(-2.0f, 2.0f);
        }

        public void MutateChangeNodeOperator()
        {
            if(_nodes.Count == 0) return;
            Node node = _nodes[Random.Range(0, _nodes.Count)];
            node.Operator = SimulationManager.instance.GetRandomConnectableOperator();
        }

        private void RemoveConnections()
        {
            for (int i = _connections.Count - 1; i >= 0; i--)
            {
                if (_connections[i].shouldBeRemoved)
                {
                    _connections.RemoveAt(i);
                    _connectionCount--;
                }
            }
        }

        public void MutateAddNode()
        {
            if (_connections.Count == 0) return;
            Node nodeToAdd = new Node(_nodeCount);
            _nodeCount++;
            int index = Random.Range(0, _connections.Count);
            Connection connectionToSplit = _connections[index];
            Connection left = new Connection(connectionToSplit.input, nodeToAdd, connectionToSplit.weighting, _connectionCount);
            _connectionCount++;
            Connection right = new Connection(nodeToAdd, connectionToSplit.output, connectionToSplit.weighting, _connectionCount);
            _connectionCount++;
            _connections[index] = left;
            _connections.Add(right);
            _nodes.Add(nodeToAdd);
        }

        private void RemoveUnusedNodes()
        {
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                if (_nodes[i].shouldBeDestroyed)
                {
                    _nodes.RemoveAt(i);
                    _nodeCount--;
                }
            }
        }

        private void GetConnectionsToRemove(Connection connectionToRemove)
        {
            if (connectionToRemove.output is Node nodeOut)
            {
                if (!nodeOut.shouldBeDestroyed)
                {
                    bool leftHanging = !_connections.Any(connection => connection.output == nodeOut && connection != connectionToRemove);
    
                    if (leftHanging)
                    {
                        nodeOut.shouldBeDestroyed = true;
                        foreach (var connection in _connections.Where(connection => (connection.input == nodeOut && connection != connectionToRemove)))
                        {
                            GetConnectionsToRemove(connection);
                        }
                    }
                }
            }

            if (connectionToRemove.input is Node nodeIn)
            {
                if (!nodeIn.shouldBeDestroyed)
                {
                    bool rightHanging = !_connections.Any(connection => connection.input == nodeIn && connection != connectionToRemove);

                    if (rightHanging)
                    {
                        nodeIn.shouldBeDestroyed = true;
                        foreach (var connection in _connections.Where(connection => (connection.output == nodeIn && connection != connectionToRemove)))
                        {
                            GetConnectionsToRemove(connection);
                        }
                    }
                }
            }
            connectionToRemove.shouldBeRemoved = true;
        }

        public void SetInput(int index, float value)
        {
            if (index < _inputSignals.Count)
            {
                _inputSignals[index].InputValue = value;
            }
        }

        public float GetOutput(int index)
        {
            if (index >= _outputSignals.Count) return 0.0f;
            _outputSignals[index].EvaluateOutput();
            return _outputSignals[index].OutputValue;
        }

        public void Activate()
        {
            foreach (var node in _nodes)
            {
                node.OutputValue = 0.0f;
                node.InputValue = 0.0f;
            }

            foreach (var signal in _outputSignals)
            {
                signal.InputValue = 0.0f;
            }
        
            foreach (var connection in _connections)
            {
                connection.input.EvaluateOutput();
                connection.output.InputValue += connection.input.OutputValue * connection.weighting;
            }
            
        }

        public void DebugListConnections()
        {
            foreach (var c in _connections)
            {
                Debug.Log(c);
            }
        }
    
        public string OutputsToString()
        {
            string s = "{";
            foreach (var output in _outputSignals)
            {
                output.EvaluateOutput();
                s += output.OutputValue + ",";
            }
        
            s += "}";
            return s;
        }

        public string InputsToString()
        {
            string s = "{";
            foreach (var input in _inputSignals)
            {
                s += input.InputValue + ",";
            }
        
            s += "}";
            return s;
        }
    }
}