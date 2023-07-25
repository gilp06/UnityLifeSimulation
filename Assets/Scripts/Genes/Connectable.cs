using System;
using UnityEngine;

namespace Genes
{
    public enum ConnectableOperator
    {
        Sigmoid,
        Abs,
        Clamped,
        Cube,
        Exp,
        Gauss,
        Hat,
        Sin,
        Square,
        Tanh,
        Identity,
    }
    
    public class Connectable
    {
        public float InputValue { get; set; }
        public float OutputValue { get; set; }
        public ConnectableOperator Operator { get; set; }

        public void EvaluateOutput()
        {
            switch (Operator)
            {
                case ConnectableOperator.Sigmoid:
                    OutputValue = 1.0f/(1.0f + Mathf.Exp(-InputValue));
                    break;
                case ConnectableOperator.Identity:
                    OutputValue = InputValue;
                    break;
                case ConnectableOperator.Abs:
                    OutputValue = Mathf.Abs(InputValue);
                    break;
                case ConnectableOperator.Clamped:
                    OutputValue = Mathf.Clamp(InputValue, -1.0f, 1.0f);
                    break;
                case ConnectableOperator.Cube:
                    OutputValue = Mathf.Pow(InputValue, 3);
                    break;
                case ConnectableOperator.Exp:
                    OutputValue = Mathf.Exp(InputValue);
                    break;
                case ConnectableOperator.Gauss:
                    OutputValue = Mathf.Exp(-Mathf.Pow(InputValue, 2));
                    break;
                case ConnectableOperator.Hat:
                    OutputValue = Mathf.Max(1 - Mathf.Abs(InputValue), 0);
                    break;
                case ConnectableOperator.Sin:
                    OutputValue = Mathf.Sin(InputValue);
                    break;
                case ConnectableOperator.Square:
                    OutputValue = Mathf.Pow(InputValue, 2);
                    break;
                case ConnectableOperator.Tanh:
                    OutputValue = MathF.Tanh(InputValue);
                    break;
            }
        }

    }
}