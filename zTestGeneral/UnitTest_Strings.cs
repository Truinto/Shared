using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.StringsNS;
using System.Diagnostics;

namespace UnitTest
{
    [TestClass]
    public class UnitTests_Strings
    {
        [TestMethod]
        public void Test_Strings_SplitArgs()
        {
            (string test, string[] solution)[] values = [
                ( "", [] ),
                ( " ", [] ),
                ( "\" ", [" "] ),
                ( "Foo Bar", ["Foo", "Bar"] ),  //Foo Bar
                ( "Foo Bar ", ["Foo", "Bar"] ),  //Foo Bar 
                ( "Foo\" \"Bar", ["Foo Bar"] ), //Foo" "Bar
                ( "\"Foo Bar\"", ["Foo Bar"] ), //"Foo Bar"
                ( "Foo  \"\"  Bar", ["Foo", "", "Bar"] ),  //Foo  ""  Bar
                ( "\"\"Foo Bar\"\"", ["Foo", "Bar"] ),  //""Foo Bar""
                ( "\"\"\"Foo Bar\"\"\"", ["\"Foo Bar\""] ), //"""Foo Bar"""
                ( "\"\"\"\"Foo Bar\"\"\"\"", ["\"Foo", "Bar\""] ),  //""""Foo Bar""""
                ( "\"\"Foo\" \"Bar\"\"", ["Foo Bar"] ),  //""Foo" "Bar""
                ( "\"Foo \"\"\"Bar\"\"", ["Foo \"Bar"] ),   //"Foo """Bar""
                ( "\"\"a\"\"b\"\"c\"\"d\"\"", [ "abcd" ] ), //""a""b""c""d""
                ( "\"\" \"\" \"\" \"\" \"\"", ["", "", "", "", ""] ),   //"" "" "" "" ""
                ( "\" \" \" \" \" \" \" \" \" \"", [" ", " ", " ", " ", " "] ), //" " " " " " " " " "
                ( "\"\"\"\"\"\"\"\"\"\"\"\"", ["\"\"\"\"\""] ), //""""""""""""
            ];

            int i = 0;
            foreach (var (test, solution) in values)
            {
                var actual = Strings.SplitArgs(test);
                CollectionAssert.AreEqual(solution, actual, $"{i} '{test}'({actual.Count}) != '{Strings.Join(solution)}'({solution.Length})");
                i++;
            }
        }

        [TestMethod]
        public void Test_Strings_JoinArgs()
        {
            (string[] test, string solution)[] values = [
                ( [], "" ),
                ( [""], "\"\"" ), //""
                ( ["Foo"], "Foo" ),
                ( ["Foo", "Bar"], "Foo Bar" ),
                ( ["Foo Bar"], "\"Foo Bar\"" ),
                ( ["Foo\"Bar"], "\"Foo\"\"Bar\"" ), //"Foo""Bar"
                ( ["\"Foo Bar\""], "\"\"\"Foo Bar\"\"\"" ), //"""Foo Bar"""
                ( ["\"Foo", "Bar\""], "\"\"\"Foo\" \"Bar\"\"\"" ), //"""Foo" "Bar"""
                ( ["Foo\" \"Bar"], "\"Foo\"\" \"\"Bar\"" ), //"Foo"" ""Bar"
                ( ["Foo ", "", " Bar"], "\"Foo \" \"\" \" Bar\"" ),
                ( ["Foo", "", "Bar"], "Foo \"\" Bar" ),
                ( ["Foo \"", "", "\" Bar"], "\"Foo \"\"\" \"\" \"\"\" Bar\"" ), //"Foo """ "" """ Bar"
            ];

            int i = 0;
            foreach (var (test, solution) in values)
            {
                var actual = Strings.JoinArgs(test);
                Assert.AreEqual(solution, actual, $"at {i}");
                i++;
            }
        }

        [TestMethod]
        public void Test_Strings_TrySubstringByOccurrence()
        {
            Assert.IsTrue(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', 0, out string str));
            Assert.AreEqual("Foo", str);
            Assert.IsTrue(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', 1, out str));
            Assert.AreEqual("Bar", str);
            Assert.IsTrue(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', 2, out str));
            Assert.AreEqual("Mee", str);
            Assert.IsTrue(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', ^2, out str));
            Assert.AreEqual("Foo", str);
            Assert.IsTrue(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', ^1, out str));
            Assert.AreEqual("Bar", str);
            Assert.IsTrue(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', ^0, out str));
            Assert.AreEqual("Mee", str);

            Assert.IsFalse(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', 3, out str));
            Assert.AreEqual("Foo.Bar.Mee", str);
            Assert.IsFalse(Strings.TrySubstringByOccurrence("Foo.Bar.Mee", '.', ^3, out str));
            Assert.AreEqual("Foo.Bar.Mee", str);
        }

        [TestMethod]
        public void Test_Strings_ToDateTime()
        {
            DateTime expected, actual;

            // basic short hand versions
            expected = DateTime.Parse("2020-01-01"); actual = Strings.ToDateTime("2020"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-01"); actual = Strings.ToDateTime("2020-06"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15"); actual = Strings.ToDateTime("2020-06-15"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15T12:30:20"); actual = Strings.ToDateTime("2020-06-15-12:30:20"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15T12:30:20Z"); actual = Strings.ToDateTime("2020-06-15-12:30:20Z"); Assert.AreEqual(expected, actual);

            // ticks
            expected = DateTime.Parse("2020-06-15T12:30:20.1234567"); actual = Strings.ToDateTime("2020-06-15-12:30:20.1234567"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15T12:30:20.1234567Z"); actual = Strings.ToDateTime("2020-06-15-12:30:20.1234567Z"); Assert.AreEqual(expected, actual);

            // time offset
            expected = DateTime.Parse("2020-06-15T12:30:20+01:30"); actual = Strings.ToDateTime("2020-06-15-12:30:20+01:30"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15T12:30:20-01:30"); actual = Strings.ToDateTime("2020-06-15-12:30:20-01:30"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15T12:30:20.1234567+01:30"); actual = Strings.ToDateTime("2020-06-15-12:30:20.1234567+01:30"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15T12:30:20.1234567-01:30"); actual = Strings.ToDateTime("2020-06-15-12:30:20.1234567-01:30"); Assert.AreEqual(expected, actual);

            // spacebars
            expected = DateTime.Parse("2020-06-15T12:30:20"); actual = Strings.ToDateTime(" 2020-06-15T12:30:20 "); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("2020-06-15T12:30:20Z"); actual = Strings.ToDateTime(" 2020-06-15T12:30:20 Z "); Assert.AreEqual(expected, actual);

            // natural
            expected = DateTime.Parse("12:30:20 01.02.2020"); actual = Strings.ToDateTime("12:30:20 01.02.2020"); Assert.AreEqual(expected, actual);
            expected = DateTime.Parse("01.02.2020 12:30:20"); actual = Strings.ToDateTime("01.02.2020 12:30:20"); Assert.AreEqual(expected, actual);

            // this isn't a valid format at the time
            //expected = DateTime.Now.AddDays(-30); actual = Strings.ToDateTime("30d"); Assert.IsInRange(-TimeSpan.TicksPerSecond, TimeSpan.TicksPerSecond, expected.Ticks - actual.Ticks);
        }

        [TestMethod]
        public void Test_Strings_CompareNatural()
        {
            (string? str1, string? str2, int solution)[] values = [
                ( "", "", 0 ),
                ( null, null, 0 ),
                ( "", null, 1 ),
                ( null, "", -1 ),
                ( "abc", "cba", -1 ),
                ( "cba", "abc", 1 ),
                ( "a123", "a0123", 0 ),
                ( "2", "11", -1 ),
                ( "11", "2", 1 ),
                ( "000 2", "0 11", -1 ),
                ( "000 11", "0 2", 1 ),
                ( "0 2", "000 11", -1 ),
                ( "0 11", "000 2", 1 ),
                ( "1", "1a", -1 ),
                ( "1a", "1", 1 ),
            ];

            int i = 0;
            foreach (var (str1, str2, solution) in values)
            {
                Assert.AreEqual(solution, Strings.CompareNatural(str1, str2), $"at {i}");
                i++;
            }
        }
    }
}
