using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;

namespace InfixInterpreter
{
	internal class Program
	{
		public static string[] Infixes { get; } =
		{
			"A+B*C",
			"A*B+C",
			"A*B+C*D",
			"(A+B)/(C-D)",
			"A*B/C",
			"((A+B)*C+D)/(E+F+G)",
		};

		public static string[] PostFixes { get; } =
		{
			"ABC*+",
			"AB*C+",
			"AB*CD*+",
			"AB+CD-/",
			"AB*C/",
			"AB+C*D+EF+G+/",
		};

		public static Dictionary<char, int> Operators { get; } = new Dictionary<char, int>
		{
			{ '+', 2 },
			{ '-', 2 },
			{ '*', 1 },
			{ '/', 1 },
		};

		public const char OpenParenthesis = '(';
		public const char CloseParenthesis = '(';

		public static void Main(string[] args)
		{
			Interpret(0);
			Interpret(1);
			Interpret(2);
			Interpret(3);
		}

		public static void Interpret(int index)
		{
			string input    = Infixes[index];
			string actual   = Interpret(input);
			string expected = PostFixes[index];
			bool   equal    = actual == expected;
			
			Console.WriteLine($"{input} => \"{actual}\" [{expected}] [{(equal ? "true" : "false")}]");
		}

		public static string Interpret(string infix)
		{
			string postfix = string.Empty;

			for (int i = 0; i < infix.Length; i++)
			{
				string result = HandleChar(infix, i);
				if (result.Length > 1)
				{
					postfix += result;
					i       += result.Length - 2;
				}
			}

			return postfix;
		}

		public static string HandleChar(string infix, int index)
		{
			char current = infix[index];

			// Is operator.
			if (Operators.ContainsKey(current))
			{
				int precedence = Operators[current];

				string deeperPostfix    = string.Empty;
				string shallowerPostfix = string.Empty;

				// One operator over.
				if (index + 2 < infix.Length)
				{
					char nextOp = infix[index + 2];
					if (Operators.ContainsKey(nextOp))
					{
						int nextPrecedence = Operators[nextOp];

						if (nextPrecedence < precedence)
						{
							deeperPostfix = HandleChar(infix, index + 2);
						}
						else
						{
							deeperPostfix = infix[index + 1].ToString();
						}
					}else
					{
						deeperPostfix = infix[index + 1].ToString();
					}
				}
				else
				{
					deeperPostfix = infix[index + 1].ToString();
				}

				if (index - 2 >= 0)
				{
					char prevOp = infix[index - 2];
					if (Operators.ContainsKey(prevOp))
					{
						int prevPrecedence = Operators[prevOp];

						if (prevPrecedence > precedence)
						{
							shallowerPostfix = infix[index - 1].ToString();
						}
					}
				}
				else
				{
					shallowerPostfix = infix[index - 1].ToString();
				}

				return $"{shallowerPostfix}{deeperPostfix}{infix[index]}";
			}

			if (current == OpenParenthesis)
			{
				return Interpret(infix.Substring(index+1));
			}

			if (current == CloseParenthesis)
			{
				return "";
			}

			return infix[index].ToString();
		}
	}
}