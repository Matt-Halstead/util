<Query Kind="Program" />

void Main()
{
	var terms = Term.And()
		.Add("PO > @PO", "@PO", "A123345")
		.AddIf(() => true, "BBB > @PO", "@PO", "A123345")
		.Add("PO > @PO", "@PO", "A123345")
		.AddIf(() => false, "AAA > @PO", "@PO", "A123345")
		.And(g => g
			.Add("Line > @Line", "@Line", "234")
			.Or(g1 => g1
				.Add("Line1 > @Line", "@Line", "234")
				.Add("Line2 > @Line", "@Line", "234")
				.Or(g2 => g2
					.Add("Line3 > @Line", "@Line", "234")
					.Add("Line4 > @Line", "@Line", "234")
				)
			)
		)
		;
	
	var sb = new StringBuilder();
	var openGroups = new Stack<Term>();
	
	Action<Term> visitor = (term) =>
	{
		if (term.Operation == TermType.Term)
		{
			return;
		}

		var useBrackets = term.Children.Any(t => t.Operation != TermType.Term);
		var delimiter = term.Operation == TermType.And ? " AND " : " OR ";
		
		if (useBrackets)
		{
			sb.Append("(");
			openGroups.Push(term);
		}
		sb.Append(string.Join(delimiter, term.Children.Select(c => c.Statement)));
	};

	terms.Traverse(visitor);
	Console.WriteLine(sb.ToString());
}

enum TermType
{
	Term,
	And,
	Or
}

class Term
{
	public string Statement { get; set; }
	public string ParamName { get; set; }
	public object ParamValue { get; set; }
	public TermType Operation { get; set; }
	public List<Term> Children { get; } = new List<Term>();

	private Term()
	{
	}

	public Term Add(string statement, string paramName, object paramValue)
	{
		var newTerm = new Term { Statement = statement, ParamName = paramName, ParamValue = paramValue, Operation = TermType.Term };
		Children.Add(newTerm);
		return this;
	}

	public Term AddIf(Func<bool> condition, string statement, string paramName, object paramValue)
	{
		if (condition?.Invoke() ?? false)
		{
			var newTerm = new Term { Statement = statement, ParamName = paramName, ParamValue = paramValue, Operation = TermType.Term };
			Children.Add(newTerm);
		}
		return this;
	}

	public static Term And()
	{
		return new Term() { Operation = TermType.And };
	}

	public static Term Or()
	{
		return new Term() { Operation = TermType.Or };
	}

	public Term And(Func<Term, Term> groupAction)
	{
		var andGroup = new Term() { Operation = TermType.And };
		Children.Add(andGroup);
		groupAction?.Invoke(andGroup);
		return this;
	}

	public Term Or(Func<Term, Term> groupAction)
	{
		var orGroup = new Term() { Operation = TermType.Or };
		Children.Add(orGroup);
		groupAction?.Invoke(orGroup);
		return this;
	}

	public void Traverse(Action<Term> visitor)
	{
		var Q = new Queue<Term>();
		var S = new HashSet<Term>();
		Q.Enqueue(this);
		S.Add(this);		
		visitor(this);
					
		while (Q.Count > 0)
		{
			Term parentTerm = Q.Dequeue();

			foreach (Term childTerm in parentTerm.Children)
			{
				if (!S.Contains(childTerm))
				{
					Q.Enqueue(childTerm);
					S.Add(childTerm);
					visitor(childTerm);
				}
			}
		}
	}
}