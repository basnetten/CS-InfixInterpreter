using System;
using System.Collections.Generic;

namespace InfixInterpreter
{
	public class InToPo
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
				InfixToPostfix.GetVariablesAndOperatorsFromString(infix);

			List<Operator> operatorList =
				GetOperatorsFromOperatorsString(stringOperators);

			// Connect the same parentheses layers.
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

			int numOfVariablesAssigned = 0;
			foreach (Operator @operator in operatorList)
			{
				if (@operator.Left == null)
					@operator.Left =
						new Operator(stringVariables[numOfVariablesAssigned++],
						             @operator.ParenthesesDepth,
						             @operator.Precedence,
						             true);
				if (@operator.Right == null)
					@operator.Right =
						new Operator(stringVariables[numOfVariablesAssigned++],
						             @operator.ParenthesesDepth,
						             @operator.Precedence,
						             true);
			}

			Operator head = operatorList.Find(o => !o.HasParent);

			return head.Postfix();
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