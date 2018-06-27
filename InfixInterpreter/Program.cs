using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Linq;
using Microsoft.SqlServer.Server;

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

		public const char OpenParenthesis  = '(';
		public const char CloseParenthesis = ')';

		public static void Main(string[] args)
		{
			Interpret(0);
			Interpret(1);
			Interpret(2);
			Interpret(3);
			Interpret(4);
			Interpret(5);
		}

		public static void Interpret(int index)
		{
			string input    = Infixes[index];
			string actual   = InToPo.Interpret(input);
			string expected = PostFixes[index];
			bool   equal    = actual == expected;

			Console.WriteLine($"{input} => \"{actual}\" [{expected}] [{(equal ? "true" : "false")}]");
		}

		public static string Interpret(string infix)
		{
			string postfix = string.Empty;

			string operators = string.Empty;
			string variables = string.Empty;

			foreach (char c in infix)
			{
				if (Operators.ContainsKey(c) || c == CloseParenthesis || c == OpenParenthesis) operators += c;
				else variables                                                                           += c;
			}

			var ops = new List<Operator>();
			int cpd = 0; // Current parentheses depth.
			for (var i = 0; i < operators.Length; i++)
			{
				char o = operators[i];
				if (o == OpenParenthesis)
				{
					cpd++;
					continue;
				}

				if (o == CloseParenthesis)
				{
					cpd--;
					continue;
				}

				ops.Add(new Operator { Value = o, ParenthesesCount = cpd, Precedence = Operators[o] });
			}

			var nodes = new List<Tree.Node>();
			foreach (Operator op in ops)
			{
				nodes.Add(new Tree.Node { Operator = op });
			}

			bool[] bitmask = new bool[nodes.Count];
			for (int i = 0; i < nodes.Count; i++)
			{
				Tree.Node prevnode = i > 0 ? nodes[i - 1] : null;
				Tree.Node curnode  = nodes[i];
				Tree.Node nextnode = i + 1 < nodes.Count ? nodes[i + 1] : null;
				if (prevnode?.CompareTo(curnode) < 0)
				{
					curnode.Left   = prevnode;
					bitmask[i - 1] = true;
				}

				if (curnode.CompareTo(nextnode) > 0)
				{
					curnode.Right  = nextnode;
					bitmask[i + 1] = true;
				}
			}

			Tree tree = new Tree { Head = nodes.Where((node, i) => !bitmask[i]).First() };
			tree.Head.FillLeaves(variables, 0);

			Console.WriteLine($"operators {operators}");
			Console.WriteLine($"variables {variables}");
			Console.WriteLine($"ops       {string.Join(", ", ops)}");
			Console.WriteLine($"bitmask   {string.Join(", ", bitmask)}");
			Console.WriteLine($"tree      {tree.Head}");

			return tree.Head.ToString();
		}

		public class Tree
		{
			public Node Head { get; set; }

			public class Node : IComparable<Node>
			{
				public Node Left  { get; set; }
				public Node Right { get; set; }

				public Operator Operator { get; set; }

				public virtual int LeafCount
				{
					get
					{
						int c = 0;
						if (Left  == null) c++;
						if (Right == null) c++;
						return c;
					}
				}

				public int FillLeaves(string variables, int startIndex)
				{
					int inc = 0;
					if (LeafCount > 0)
					{
						if (Left == null)
						{
							Left = new LeafNode { Operator = new Operator { Value = variables[startIndex + inc] } };
							inc++;
						}
						else
						{
							inc += Left.FillLeaves(variables, startIndex + inc);
						}

						if (Right == null)
						{
							Right = new LeafNode { Operator = new Operator { Value = variables[startIndex + inc] } };
							inc++;
						}
						else
						{
							inc += Right.FillLeaves(variables, startIndex + inc);
						}
					}
					else
					{
						inc += (Left?.FillLeaves(variables, startIndex + inc)).GetValueOrDefault();
						inc += (Right?.FillLeaves(variables, startIndex + inc)).GetValueOrDefault();
					}

					return inc;
				}

				public int CompareTo(Node other)
				{
					return Operator.CompareTo(other?.Operator);
				}

				public override string ToString()
				{
					return $"[{Left?.ToString() ?? "X"}][{Right?.ToString() ?? "X"}]{Operator.Value}";
				}
			}

			public class LeafNode : Node
			{
				public override int LeafCount => 0;
				
				public override string ToString()
				{
					return $"{Operator.Value}";
				}
			}
		}

		public class Operator : IComparable<Operator>
		{
			public char Value            { get; set; }
			public int  Precedence       { get; set; }
			public int  ParenthesesCount { get; set; }

			public override string ToString()
			{
				return $"{Value} {Precedence} {ParenthesesCount}";
			}

			public int CompareTo(Operator other)
			{
				if (other == null) return -1;

				if (other.ParenthesesCount < ParenthesesCount) return -1;
				if (other.ParenthesesCount > ParenthesesCount) return 1;

				int precedenceCompare = Precedence.CompareTo(other.Precedence);
				if (precedenceCompare == 0) return -1;
				return precedenceCompare;
			}
		}
	}
}