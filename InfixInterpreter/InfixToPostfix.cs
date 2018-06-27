using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace InfixInterpreter
{
	public class InfixToPostfix
	{
		/**
		 * When talking about precedence, lower values should be handled first.
		 */

		public static Dictionary<char, int> OperatorPrecedences { get; }
			= new Dictionary<char, int>
			{
				{ '+', 2 },
				{ '-', 2 },
				{ '*', 1 },
				{ '/', 1 },
			};

		public const char OpenParenthesis  = '(';
		public const char CloseParenthesis = ')';

		public static string Interpret(string infix)
		{
			(string stringVariables, string stringOperators) =
				GetVariablesAndOperatorsFromString(infix);

			List<Operator> operatorList =
				GetOperatorsFromOperatorsString(stringOperators);

			bool[] operatorIsChild =
				GenerateTreeFromOperatorList(operatorList);

			Operator head =
				GetHeadFromOperatorList(operatorList, operatorIsChild);

			Console.WriteLine($"stringVariables {stringVariables}");
			Console.WriteLine($"stringOperators {stringOperators}");
			Console.WriteLine($"operatorList {string.Join(", ",    operatorList)}");
			Console.WriteLine($"operatorIsChild {string.Join(", ", operatorIsChild)}");
			Console.WriteLine($"head {head}");

			return head.Postfix();
		}

		/// <summary>
		/// Separates operators from variables from <paramref name="str"/>.
		/// Operators are defined in <see cref="OperatorPrecedences"/>, combined
		/// with <see cref="OpenParenthesis"/> and
		/// <see cref="CloseParenthesis"/>. Variables are all other characters
		/// in the string.
		/// </summary>
		/// <remarks>
		/// It is assumed that <paramref name="str"/>'s format is correct.
		/// </remarks>
		/// <param name="str">String containing operators (as defined by
		/// <see cref="OperatorPrecedences"/>) and variables.</param>
		/// <returns>A tuple containing two strings: one containing all the
		/// variables and one containing all the operators.</returns>
		public static (string variables, string operators)
			GetVariablesAndOperatorsFromString(string str)
		{
			// Initialize local variables.
			string variables = String.Empty;
			string operators = String.Empty;

			// Add each character of the string to either the operators or the variables.
			foreach (char c in str)
			{
				if (IsOperator(c))
					operators += c;
				else
					variables += c;
			}

			// Return a tuple with the information.
			return (variables, operators);
		}

		public static List<Operator>
			GetOperatorsFromOperatorsString(string operatorsString)
		{
			List<Operator> operators = new List<Operator>();

			int currentParenthesesDepth = 0;
			for (int i = 0; i < operatorsString.Length; i++)
			{
				char @operator = operatorsString[i];
				if (@operator == OpenParenthesis)
					currentParenthesesDepth++;
				else if (@operator == CloseParenthesis)
					currentParenthesesDepth--;
				else
					operators.Add(new Operator(@operator,
					                           currentParenthesesDepth,
					                           OperatorPrecedences[@operator],
					                           false));
			}

			return operators;
		}

		public static bool[]
			GenerateTreeFromOperatorList(List<Operator> operatorList)
		{
			bool[] operatorIsChild = new bool[operatorList.Count];
			for (int i = 0; i < operatorList.Count; i++)
			{
				Operator current = operatorList[i];
				if (i > 0)
				{
					if (current.ConnectLeft(operatorList[i - 1]))
						operatorIsChild[i - 1] = true;
				}

				if (i < operatorList.Count - 1)
				{
					if (current.ConnectRight(operatorList[i + 1]))
						operatorIsChild[i + 1] = true;
				}
			}

			return operatorIsChild;
		}

		public static Operator
			GetHeadFromOperatorList(List<Operator> operatorList,
			                        bool[]         operatorIsChild)
		{
			return operatorList.Where((node, i) => !operatorIsChild[i]).First();
		}

		public static bool IsOperator(char c)
		{
			return OperatorPrecedences.ContainsKey(c)
			       || c == OpenParenthesis
			       || c == CloseParenthesis;
		}

		public class Operator
		{
			public Operator(char value,
			                int  parenthesesDepth,
			                int  precedence,
			                bool isLeaf)
			{
				Value            = value;
				ParenthesesDepth = parenthesesDepth;
				Precedence       = precedence;
				IsLeaf           = isLeaf;
			}

			// TODO make better.
			public char Value            { get; set; }
			public int  ParenthesesDepth { get; set; }
			public int  Precedence       { get; set; }

			public bool     IsLeaf { get; set; }
			public Operator Left   { get; set; }
			public Operator Right  { get; set; }

			public bool ConnectLeft(Operator other)
			{
				if (IsLeaf) return false;

				// If this is deeper, this should be child of other.
				bool deeper = ParenthesesDepth > other.ParenthesesDepth;

				// If this precedes other, this should be child of other.
				if (deeper) return false;
//				if (!(Precedence < other.Precedence && !deeper)) return false;

				Left = other;
				return true;
			}

			public bool ConnectRight(Operator other)
			{
				if (IsLeaf) return false;

				// If this is deeper, this should be child of other.
				bool deeper = ParenthesesDepth > other.ParenthesesDepth;

				// If this precedes other, this should be child of other.
				if (deeper) return false;
//				if (!(Precedence < other.Precedence && !deeper)) return false;

				Right = other;
				return true;
			}

			public string Postfix()
			{
				if (IsLeaf)
					return $"{Value}";
				else
					return $"{Left?.Postfix()}{Right?.Postfix()}{Value}";
			}

			public override string ToString()
			{
				return $"{Value}({ParenthesesDepth},{Precedence})";
			}
		}
	}
}