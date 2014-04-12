using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monad;
using Monad.Parsec;

namespace CSharpMonad.UnitTests.Lang
{
    [TestFixture]
    public class LangTests
    {
        Parser<IEnumerable<ParserChar>> Id;
        Parser<IEnumerable<ParserChar>> Ident;
        Parser<IEnumerable<ParserChar>> LetId;
        Parser<IEnumerable<ParserChar>> Semi;
        Parser<IEnumerable<ParserChar>> LambdaArrow;
        Parser<IEnumerable<ParserChar>> InId;
        Parser<IEnumerable<ParserChar>> Op;
        Parser<Term> Integer;
        Parser<Term> String;
        Parser<Term> Term;
        Parser<Term> Term1;
        Parser<Term> Parser;

        [Test]
        public void BuildLangParser()
        {
            var opChars = ";.,<>?/\\|\"':}{[]=+-_)(*&^%$£@!".AsEnumerable();

            Id = from w in New.Whitespace()
                 from c in New.Letter()
                 from cs in New.Many(New.LetterOrDigit())
                 select c.Cons(cs);

            Op = from w in New.Whitespace()
                 from o in New.Satisfy(c => opChars.Contains(c),"an operator")
                 from os in New.Many(New.Satisfy(c => opChars.Contains(c),"an operator"))
                 select o.Cons(os);

            Ident = from s in Id 
                    where 
                        s.IsNotEqualTo("let") && 
                        s.IsNotEqualTo("in") 
                    select s;

            LetId = from s in Id
                    where s.IsEqualTo("let")
                    select s;

            InId = from s in Id
                   where s.IsEqualTo("in")
                   select s;

            Semi = from s in Op
                   where s.IsEqualTo(";")
                   select s;

            LambdaArrow = from s in Op
                          where s.IsEqualTo("=>")
                          select s;

            Integer = from w in New.Whitespace()
                      from d in New.Integer()
                      select new IntegerTerm(d) as Term;

            String = from w in New.Whitespace()
                     from o in New.Character('"')
                     from cs in New.Many(New.Satisfy(c => c != '"', "a string"))
                     from c in New.Character('"')
                     select new StringTerm(cs) as Term;

            Term1 = Integer
                    .Or(String)
                    .Or(from x in Ident
                        select new VarTerm(x) as Term)
                    .Or(from u1 in Lang.WsChr('(')
                        from t in Term
                        from u2 in Lang.WsChr(')')
                        select t);

            Term = (from x in Ident
                    from arrow in LambdaArrow
                    from t in Term
                    select new LambdaTerm(x, t) as Term)
                    .Or(from lid in LetId
                        from x in Ident
                        from u1 in Lang.WsChr('=')
                        from t in Term
                        from s in Semi
                        from c in Term
                        select new LetTerm(x, t, c) as Term)
                    .Or(from t in Term1
                        from ts in New.Many(Term1)
                        select new AppTerm(t, ts) as Term);

            Parser = from t in Term
                     from u in Lang.WsChr(';')
                     from w in New.Whitespace()
                     select t;
        }


        [Test]
        public void RunLangParser()
        {
            BuildLangParser();

            var input = @"
                let value = 1234;
                let str = ""hello, world"";
                let t = str;
                let fn = x => x;
                fn value;";

            var result = Parser.Parse(input);

            if (result.IsFaulted)
            {
                foreach (var error in result.Errors)
                {
                    var msg = error.Message + "Expected: " + error.Expected + " at " + error.Location + " - " + error.Input.AsString().Substring(0, Math.Min(30, error.Input.AsString().Length)) + "...";
                    Console.WriteLine(msg);
                }
            }

            Assert.IsTrue(!result.IsFaulted);

            Term ast = result.Value.Single().Item1;

            // TODO: Check the validity of the produced AST

        }
    }

    public abstract class Term { }
    public class LambdaTerm : Term
    {
        public readonly IEnumerable<ParserChar> Ident; 
        public readonly Term Term;
        public LambdaTerm(IEnumerable<ParserChar> i, Term t)
        {
            Ident = i; 
            Term = t;
        }
    }

    public class StringTerm : Term
    {
        public readonly IEnumerable<ParserChar> Value;

        public StringTerm(IEnumerable<ParserChar> cs)
        {
            Value = cs;
        }
    }

    public class IntegerTerm : Term
    {
        public readonly int Value;

        public IntegerTerm(int v)
        {
            Value = v;
        }
    }

    public class LetTerm : Term
    {
        public readonly IEnumerable<ParserChar> Ident; 
        public readonly Term Rhs; 
        public Term Body;
        public LetTerm(IEnumerable<ParserChar> i, Term r, Term b)
        {
            Ident = i; 
            Rhs = r; 
            Body = b;
        }
    }
    public class AppTerm : Term
    {
        public readonly Term Func; 
        public readonly IEnumerable<Term> Args;
        public AppTerm(Term func, IEnumerable<Term> args)
        {
            Func = func; 
            Args = args;
        }
    }
    public class VarTerm : Term
    {
        public readonly IEnumerable<ParserChar> Ident;
        public VarTerm(IEnumerable<ParserChar> ident)
        {
            Ident = ident;
        }
    }


    public class WsChrParser : Parser<ParserChar>
    {
        public WsChrParser(char c)
            :
            base(
                inp => New.Whitespace()
                .And(New.Character(c))
                .Parse(inp)
            )
        {
        }
    }


    public static class Lang
    {
        public static WsChrParser WsChr(char c)
        {
            return new WsChrParser(c);
        }
    }
}