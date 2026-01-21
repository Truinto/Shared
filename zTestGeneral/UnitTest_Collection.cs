using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.CollectionNS;
using System.Diagnostics;

namespace UnitTest
{
    [TestClass]
    public class UnitTests_Collection
    {
        private static object _Lock = new();
        private static string? _Shared;
        private static string Shared
        {
            get
            {
                if (_Shared is not null)
                    return _Shared;

                lock (_Lock)
                {
                    if (_Shared is not null)
                        return _Shared;

                    string shared1 = Path.DirectorySeparatorChar + "@Shared" + Path.DirectorySeparatorChar;
                    string shared2 = Path.DirectorySeparatorChar + "Shared" + Path.DirectorySeparatorChar;
                    string? path = Environment.CurrentDirectory;
                    path = Path.GetDirectoryName(path + "/"); //normalize

                    while (true)
                    {
                        path = Path.GetDirectoryName(path);

                        if (path is null or "")
                            throw new FileNotFoundException("Could not resolve Shared directory!");
                        if (Directory.Exists(path + shared1))
                        {
                            path += shared1;
                            break;
                        }
                        if (Directory.Exists(path + shared2))
                        {
                            path += shared2;
                            break;
                        }
                    }

                    return _Shared = path;
                }
            }
        }

        [TestMethod]
        public void Test_Collection_StartsWith()
        {
            Assert.IsFalse(CollectionTool.StartsWith(new FileInfo($"{Shared}zTestGeneral/Sample_ASCII.txt"), "Not correct!", false));

            Assert.IsTrue(CollectionTool.StartsWith(new FileInfo($"{Shared}zTestGeneral/Sample_ASCII.txt"), "Hello World!", false));
            Assert.IsTrue(CollectionTool.StartsWith(new FileInfo($"{Shared}zTestGeneral/Sample_Unicode_LE.txt"), "Hello World!", false));
            Assert.IsTrue(CollectionTool.StartsWith(new FileInfo($"{Shared}zTestGeneral/Sample_Unicode_BE.txt"), "Hello World!", false));
        }

        [TestMethod]
        public void Test_Collection_MoveTo()
        {
            int[] array = [];
            CollectionTool.MoveTo(array, 0, 0, false);
            CollectionAssert.AreEqual(array, (int[])[]);

            array = [1, 3, 5, 7];
            CollectionTool.MoveTo(array, 0, 3, true);
            CollectionAssert.AreEqual(array, (int[])[3, 5, 7, 1]);

            array = [1, 3, 5, 7];
            CollectionTool.MoveTo(array, 3, 0, true);
            CollectionAssert.AreEqual(array, (int[])[7, 1, 3, 5]);

            array = [1, 3, 5, 7];
            CollectionTool.MoveTo(array, 1, 2, true);
            CollectionAssert.AreEqual(array, (int[])[1, 5, 3, 7]);
        }

        [TestMethod]
        public void Test_Collection_QuickSort()
        {
            int[] array = [];
            CollectionTool.QuickSort(array, 0, array.Length - 1);
            CollectionAssert.AreEqual(array, (int[])[]); 
            
            array = [1];
            CollectionTool.QuickSort(array, 0, array.Length - 1);
            CollectionAssert.AreEqual(array, (int[])[1]); 
            
            array = [3, 2, 1];
            CollectionTool.QuickSort(array, 0, array.Length - 1);
            CollectionAssert.AreEqual(array, (int[])[1, 2, 3]);

            array = [6, 5, 1, 3, 4, 2];
            CollectionTool.QuickSort(array, 0, array.Length - 1);
            CollectionAssert.AreEqual(array, (int[])[1, 2, 3, 4, 5, 6]);

            string[] strings = ["123456", "1234", "1", "123", "12345", "", "12"];
            CollectionTool.Sort(strings, s => s.Length);
            CollectionAssert.AreEqual(strings, (string[])["", "1", "12", "123", "1234", "12345", "123456"]);
        }
    }
}
