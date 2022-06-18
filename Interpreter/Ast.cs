using System;

namespace Basic.Interpreter
{
	internal class Ast
	{

		private readonly Dictionary<long, Stmt> _lineNumberIndex;
		private readonly List<Stmt> _statements;
        private readonly Dictionary<long, int> _dataIndex;
		private readonly List<Token> _data;
		private int _dataPointer = 0;

        public Ast(List<Stmt> statements,
			Dictionary<long, Stmt> lineNumberIndex,
			Dictionary<long, List<Token>> data)
        {
			_lineNumberIndex = lineNumberIndex;
			_statements = statements;

			_dataIndex = new Dictionary<long, int>();
			_data = new List<Token>();
			
			foreach (var kvp in data.OrderBy(x => x.Key))
            {
				_dataIndex[kvp.Key] = _data.Count;
				_data.AddRange(kvp.Value);
            }
        }

		public Stmt[] Statements => _statements.ToArray();

        internal Stmt StatementIndex(long lineNumber)
        {
			return _lineNumberIndex[lineNumber];
        }

		public bool ValidLineNumber(long lineNumber)
        {
			return _lineNumberIndex.ContainsKey(lineNumber);
        }

		public string FetchDataLiteral()
        {
			if (_dataPointer >= _data.Count) throw new Exception("Out of data.");
			var r = (string)_data[_dataPointer].Literal;
			_dataPointer++;
			return r;
        }
    }
}

