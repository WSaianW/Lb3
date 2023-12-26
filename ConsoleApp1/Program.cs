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
        if (HasRedundantParentheses(infixExpression))
        {
            Console.WriteLine("Error: Redundant parentheses detected. Please fix the expression.");
            return;
        }

        var postfixExpression = InfixToPostfix(infixExpression);
        Console.WriteLine($"Postfix expression: {postfixExpression}");

        var result = EvaluateExpression(postfixExpression);
        Console.WriteLine($"Result: {result}");
    }

    private bool HasRedundantParentheses(string infixExpression)
    {
        int openingParentheses = 0;
        int closingParentheses = 0;

        foreach (char character in infixExpression)
        {
            if (character == '(')
            {
                openingParentheses++;
            }
            else if (character == ')')
            {
                closingParentheses++;
            }
        }

        return openingParentheses != closingParentheses;
    }
    public static void Main()
    {
        
        var calculator = new RPNCalculator();

        do
        {
            Console.Write("Enter an infix expression (or enter 2 to quit, 1 to continue): ");
            string userInput = Console.ReadLine();

            if (userInput == "2")
            {
                break;
            }
            else if (userInput == "1")
            {
                Console.Write("Enter the infix expression: ");
                string infixExpression = Console.ReadLine();

                if (calculator.HasRedundantParentheses(infixExpression))
                {
                    Console.WriteLine("Error: Redundant parentheses detected. Please fix the expression.");
                    continue;
                }

                calculator.ProcessExpression(infixExpression);
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter 1 to continue or 2 to quit.");
            }

        } while (true);
    }
}
