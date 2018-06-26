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
//			Interpret(1);
//			Interpret(2);
//			Interpret(3);
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

			
			
			return postfix;
		}
	}
}