using System;
using System.Collections.Generic;

class RPNCalculator
{
    private Dictionary<string, Func<double, double, double>> operators;
    private Dictionary<string, Func<double, double>> unaryOperators;
    private Dictionary<string, double> variables;

    public RPNCalculator()
    {
        operators = new Dictionary<string, Func<double, double, double>>
        {
            { "+", (a, b) => a + b },
            { "-", (a, b) => a - b },
            { "*", (a, b) => a * b },
            { "/", (a, b) => a / b },
            { "%", (a, b) => a % b },
            { "^", Math.Pow } 
        };

        unaryOperators = new Dictionary<string, Func<double, double>>
        {
            { "abs", Math.Abs },
            { "sqr", a => a * a },
            { "sign", a => Math.Sign(a) }
        };

        variables = new Dictionary<string, double>();
    }

    private bool IsOperator(string token) => operators.ContainsKey(token);

    private bool IsUnaryOperator(string token) => unaryOperators.ContainsKey(token);

    private double ApplyOperator(string op, double a, double b) => operators[op](a, b);

    private double ApplyUnaryOperator(string op, double a) => unaryOperators[op](a);

    public double EvaluateExpression(string expression)
    {
        var stack = new Stack<double>();

        var tokens = expression.Split();
        foreach (var token in tokens)
        {
            if (double.TryParse(token, out var number))
            {
                stack.Push(number);
            }
            else if (IsOperator(token))
            {
                if (stack.Count < 2)
                    throw new InvalidOperationException($"Not enough operands for operator: {token}");

                var b = stack.Pop();
                var a = stack.Pop();
                var result = ApplyOperator(token, a, b);
                stack.Push(result);
            }
            else if (IsUnaryOperator(token))
            {
                if (stack.Count < 1)
                    throw new InvalidOperationException($"Not enough operands for operator: {token}");

                var a = stack.Pop();
                var result = ApplyUnaryOperator(token, a);
                stack.Push(result);
            }
            else if (variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            else
            {
                throw new InvalidOperationException($"Invalid token: {token}");
            }
        }

        if (stack.Count != 1)
            throw new InvalidOperationException("Invalid expression");

        return stack.Pop();
    }

    public string InfixToPostfix(string infixExpression)
    {
        var output = new List<string>();
        var operatorStack = new Stack<string>();

        var precedence = new Dictionary<string, int>
        {
            { "+", 1 }, { "-", 1 },
            { "*", 2 }, { "/", 2 }, { "%", 2 },
            { "^", 3 }, 
            { "abs", 4 }, { "sqr", 4 }, { "sign", 4 },
            { "(", 0 }
        };

        var tokens = infixExpression.Split();
        foreach (var token in tokens)
        {
            if (double.TryParse(token, out _))
            {
                output.Add(token);
            }
            else if (IsOperator(token) || IsUnaryOperator(token))
            {
                while (operatorStack.Count > 0 && precedence[operatorStack.Peek()] >= precedence[token])
                {
                    output.Add(operatorStack.Pop());
                }
                operatorStack.Push(token);
            }
            else if (token == "(")
            {
                operatorStack.Push(token);
            }
            else if (token == ")")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                {
                    output.Add(operatorStack.Pop());
                }
                operatorStack.Pop();  
            }
            else if (variables.ContainsKey(token))
            {
                output.Add(variables[token].ToString());
            }
            else
            {
                throw new InvalidOperationException($"Invalid token: {token}");
            }
        }

        while (operatorStack.Count > 0)
        {
            output.Add(operatorStack.Pop());
        }

        return string.Join(" ", output);
    }

    public void ProcessExpression(string infixExpression)
    {
        var postfixExpression = InfixToPostfix(infixExpression);
        Console.WriteLine($"Postfix expression: {postfixExpression}");

        var result = EvaluateExpression(postfixExpression);
        Console.WriteLine($"Result: {result}");
    }

    public static void Main()
    {
        var calculator = new RPNCalculator();
        var infixExpression = "( 3 + 5 ) * sqr ( 2 ) - abs ( -10 ) + 11 % 2";
        calculator.ProcessExpression(infixExpression);
    }
}
