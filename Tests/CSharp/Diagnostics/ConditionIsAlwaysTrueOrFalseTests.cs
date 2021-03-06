using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConditionIsAlwaysTrueOrFalseTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestComparsionWithNull()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
		if ($i == null$) {
		}
	}
}
", @"
class Test
{
	void Foo(int i)
	{
		if (false) {
		}
	}
}
");
        }


        [Test]
        public void TestComparsionWithNullCase2()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
enum Bar { A, B }
class Test
{
	void Foo(Bar i)
	{
		if ($i != null$) {
		}
	}
}
", @"
enum Bar { A, B }
class Test
{
	void Foo(Bar i)
	{
		if (true) {
		}
	}
}
");
        }


        [Test]
        public void TestComparison()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
		if ($1 > 2$) {
		}
	}
}
", @"
class Test
{
	void Foo(int i)
	{
		if (false) {
		}
	}
}
");
        }

        [Test]
        public void TestUnary()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
		if ($!true$) {
		}
	}
}
", @"
class Test
{
	void Foo(int i)
	{
		if (false) {
		}
	}
}
");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ConditionIsAlwaysTrueOrFalseAnalyzerID + @"
		if (i == null) {
		}
	}
}
");
        }


        [Test]
        public void CompareWithNullable()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Bar
{
	public void Test(int? a)
	{
		if (a != null) {

		}
	}
}
");
        }

        [Test]
        public void UserDefinedOperatorsNoReferences()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
struct Foo 
{
	public static bool operator ==(Foo value, Foo o)
	{
		return false;
	}

	public static bool operator !=(Foo value, Foo o)
	{
		return false;
	}
}

class Bar
{
	public void Test(Foo a)
	{
		if ($a != null$) {

		}
	}
}
");
        }

        [Test]
        public void UserDefinedOperators()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
struct Foo 
{
	public static bool operator ==(Foo value, object o)
	{
		return false;
	}

	public static bool operator !=(Foo value, object o)
	{
		return false;
	}
}

class Bar
{
	public void Test(Foo a)
	{
		if (a != null) {

		}
	}
}
");
        }


        /// <summary>
        /// Bug 15099 - Wrong always true context
        /// </summary>
        [Test]
        public void TestBug15099()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
struct Foo 
{
	string name;

	public Foo (string name)
	{
		this.name = name;
	}

	public static bool operator ==(Foo value, Foo o)
	{
		return value.name == o.name;
	}

	public static bool operator !=(Foo value, Foo o)
	{
		return !(value == o);
	}

	public static implicit operator Foo (string name)
	{
		return new Foo (name);
	}
}

class Bar
{
	public static void Main (string[] args)
	{
		var foo = new Foo (null);
		System.Console.WriteLine (foo == null);
	}
}");
        }



    }
}

