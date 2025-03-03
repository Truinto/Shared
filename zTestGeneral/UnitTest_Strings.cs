﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.StringsNS;

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
                CollectionAssert.AreEqual(solution, actual, $"{i++} '{test}'({actual.Count}) != '{Strings.Join(solution)}'({solution.Length})");
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
                Assert.AreEqual(solution, actual, $"{i++}");
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
    }
}
