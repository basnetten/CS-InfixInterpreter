using System;
using System.Collections.Generic;

namespace InfixInterpreter
{
	public class InfixToPostfix
	{
		/// <summary>
		/// The open parenthesis character.
		/// </summary>
		public const char OpenParenthesis = '(';

		/// <summary>
		/// The close parenthesis character.
		/// </summary>
		public const char CloseParenthesis = ')';

		/// <summary>
		/// The operators that are known to the interpreter, together with their
		/// precedences. A lower precedence means that it will be handled first.
		/// </summary>
		public static Dictionary<char, int> OperatorPrecedences { get; }
			= new Dictionary<char, int>
			{
				{ '+', 2 },
				{ '-', 2 },
				{ '*', 1 },
				{ '/', 1 },
			};

		/// <summary>
		/// Interprets an infix string to a postfix or polish notation string.
		/// </summary>
		/// <param name="infix">The infix string.</param>
		/// <returns>The postfix string.</returns>
		public static string Interpret(string infix)
		{
			// Separate the variables from the operators in the infix string.
			(string stringVariables, string stringOperators) =
				GetVariablesAndOperatorsFromString(infix);

			Operator tree =
				ConstructOperationTree(stringOperators, stringVariables);

			return tree.Postfix();
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
			string variables = string.Empty;
			string operators = string.Empty;

			// Add each character of the string to either the operators or the variables.
			foreach (char c in str)
			{
				if (CharIsOperator(c))
					operators += c;
				else
					variables += c;
			}

			// Return a tuple with the information.
			return (variables, operators);
		}

		/// <summary>
		/// Check whether or not the character is a known operator. The known
		/// operators are stored in <see cref="OperatorPrecedences"/>.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool CharIsOperator(char c)
		{
			return OperatorPrecedences.ContainsKey(c)
			       || c == OpenParenthesis
			       || c == CloseParenthesis;
		}

		public static Operator ConstructOperationTree(string stringOperators, string stringVariables)
		{
			List<Operator> operatorList =
				GetOperatorsFromOperatorsString(stringOperators);

			ConnectSameParenthesesDepth(operatorList);

			ConnectDifferentParenthesesDepths(operatorList);

			AddVariablesAsLeafs(operatorList, stringVariables);

			return GetHeadOfTree(operatorList);
		}

		public static Operator GetHeadOfTree(List<Operator> operatorList)
		{
			return operatorList.Find(o => !o.HasParent);
		}

		public static void AddVariablesAsLeafs(List<Operator> operatorList, string stringVariables)
		{
			int numOfVariablesAssigned = 0;
			foreach (Operator @operator in operatorList)
			{
				if (@operator.Left == null)
				{
					@operator.Left = new Operator(
						stringVariables[numOfVariablesAssigned],
						@operator.ParenthesesDepth,
						@operator.Precedence,
						true);
					numOfVariablesAssigned++;
				}

				if (@operator.Right == null)
				{
					@operator.Right = new Operator(
						stringVariables[numOfVariablesAssigned],
						@operator.ParenthesesDepth,
						@operator.Precedence,
						true);
					numOfVariablesAssigned++;
				}
			}
		}

		public static void ConnectDifferentParenthesesDepths(List<Operator> operatorList)
		{
			for (int i = 0; i < operatorList.Count; i++)
			{
				Operator op = operatorList[i];
				if (i != 0)
				{
					int      j    = i;
					Operator left = null;
					// 
					while (true)
					{
						j--;
						if (j < 0) break;
						Operator next = operatorList[j];
						if (next.ParenthesesDepth != op.ParenthesesDepth + 1) break;

						left = next;
						if (!left.HasParent) break;
					}

					if (left != null && !left.HasParent)
					{
						op.Left        = left;
						left.HasParent = true;
					}
				}

				if (i != operatorList.Count - 1)
				{
					int      j     = i;
					Operator right = null;
					// 
					while (true)
					{
						j++;
						if (j > operatorList.Count - 1) break;
						Operator next = operatorList[j];
						if (next.ParenthesesDepth != op.ParenthesesDepth + 1) break;

						right = next;
						if (!right.HasParent) break;
					}

					if (right != null && !right.HasParent)
					{
						op.Right        = right;
						right.HasParent = true;
					}
				}
			}
		}

		public static void ConnectSameParenthesesDepth(List<Operator> operatorList)
		{
			for (var i = 1; i < operatorList.Count; i++)
			{
				Operator op     = operatorList[i];
				Operator opPrev = operatorList[i - 1];

				if (op.ParenthesesDepth != opPrev.ParenthesesDepth) continue;

				// op should be a child of previous. (lower precedence)
				if (op.Precedence < opPrev.Precedence)
				{
					opPrev.Right = op;
					op.HasParent = true;
				}
				// Previous should be a child of op. (previous has lower or
				// equal precedence)
				else
				{
					op.Left          = opPrev;
					opPrev.HasParent = true;
				}
			}
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

		public bool     IsLeaf    { get; set; }
		public bool     HasParent { get; set; }
		public Operator Left      { get; set; }
		public Operator Right     { get; set; }

		public string Postfix()
		{
			if (IsLeaf)
				return $"{Value}";
			else
				return $"{Left?.Postfix()}{Right?.Postfix()}{Value}";
		}

		public override string ToString()
		{
			return $"{Value}({ParenthesesDepth},{Precedence}) " +
			       $"=> [{Left?.Value}][{Right?.Value}]";
		}
	}
}