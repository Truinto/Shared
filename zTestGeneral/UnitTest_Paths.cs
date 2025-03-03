using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;
using Shared.PathsNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace UnitTest
{
    [TestClass]
    public class UnitTests_Path
    {
        [TestMethod]
        public void Test_Paths_AreEqual()
        {
            Assert.IsTrue(Paths.AreEqual("", ""));
            Assert.IsTrue(Paths.AreEqual("C:/Temp", "C:/Temp"));

            Assert.IsTrue(Paths.AreEqual("C:/Temp", "C:/Temp/"));
            Assert.IsTrue(Paths.AreEqual("C:/Temp/", "C:/Temp"));

            Assert.IsFalse(Paths.AreEqual("C:/Temp", "C:/Tem"));
            Assert.IsFalse(Paths.AreEqual("C:/Tem", "C:/Temp"));

            Assert.IsFalse(Paths.AreEqual("C:/Temp", "C:/Tem/"));
            Assert.IsFalse(Paths.AreEqual("C:/Tem/", "C:/Temp"));

            Assert.IsFalse(Paths.AreEqual("C:/Temps", "C:/Temp/"));
            Assert.IsFalse(Paths.AreEqual("C:/Temp/", "C:/Temps"));
        }

        [TestMethod]
        public void Test_Paths_IsValidPath()
        {
            Assert.IsFalse(Paths.IsValidPath(""));
            Assert.IsFalse(Paths.IsValidPath("?"));
            Assert.IsFalse(Paths.IsValidPath(":"));
            Assert.IsFalse(Paths.IsValidPath(".:"));
            Assert.IsFalse(Paths.IsValidPath("\\\\?\\"));
            Assert.IsFalse(Paths.IsValidPath("\\\\?\\/"));
            Assert.IsFalse(Paths.IsValidPath("\\\\?\\C:"));
            Assert.IsFalse(Paths.IsValidPath("\\\\?\\path.\\"));

            Assert.IsTrue(Paths.IsValidPath("\\\\?\\C:\\"));
            Assert.IsTrue(Paths.IsValidPath("C:/"));
            Assert.IsTrue(Paths.IsValidPath("."));
        }

        [TestMethod]
        public void Test_PathInfo()
        {
            (string path, string root, string directory, string fileNameNoExtension, string extension)[] test_strings = [
                (@"", "", "", "", ""),

                (@"C:/Folder/", "C:/", "C:/Folder/", "", ""),
                (@"C:\.Folder/", "C:/", "C:/.Folder/", "", ""),

                (@"C:\Folder\File", "C:/", "C:/Folder/", "File", ""),
                (@"C:\Folder\File.ext", "C:/", "C:/Folder/", "File", "ext"),
                (@"C:\.Folder\.File", "C:/", "C:/.Folder/", ".File", ""),
                (@"C:\.Folder\.File.ext", "C:/", "C:/.Folder/", ".File", "ext"),

                (@"File", "", "", "File", ""),
                (@"File.ext", "", "", "File", "ext"),
                (@".File", "", "", ".File", ""),
                (@".File.ext", "", "", ".File", "ext"),

                (@"\File", "/", "/", "File", ""),
                (@"\File.ext", "/", "/", "File", "ext"),
                (@"\.File", "/", "/", ".File", ""),
                (@"\.File.ext", "/", "/", ".File", "ext"),

                (@"..\File", "", "../", "File", ""),
                (@"..\File.ext", "", "../", "File", "ext"),
                (@"..\.File", "", "../", ".File", ""),
                (@"..\.File.ext", "", "../", ".File", "ext"),
            ];

            char c1 = PathInfo.PathSeparator == '/' ? '\\' : '/';
            char c2 = PathInfo.PathSeparator;

            foreach (var (path, root, directory, fileNameNoExtension, extension) in test_strings)
            {
                var p = new PathInfo(path);
                Assert.AreEqual(root.Replace(c1, c2), p.Root);
                Assert.AreEqual(directory.Replace(c1, c2), p.Directory);
                Assert.AreEqual(fileNameNoExtension.Replace(c1, c2), p.FileNameNoExtension);
                Assert.AreEqual(extension.Replace(c1, c2), p.Extension);
            }
        }

        [TestMethod]
        public void Test_Paths_Rx_Path()
        {
            (string path, string dir, string vol, string? folder1, string? folder2, string file, string name, string ext)[] test_strings = [
                /* 0*/ ("", "", "", null, null, "", "", ""),

                /* 1*/ ("C:/", "C:/", "C:/", null, null, "", "", ""),
                /* 2*/ ("C:/File", "C:/", "C:/", null, null, "File", "File", ""),
                /* 3*/ ("C:/.ext", "C:/", "C:/", null, null, ".ext", "", ".ext"),
                /* 4*/ ("C:/.File.ext", "C:/", "C:/", null, null, ".File.ext", ".File", ".ext"),
                /* 5*/ ("C:/Folder1/.Folder2/.File.ext", "C:/Folder1/.Folder2/", "C:/", "Folder1", ".Folder2", ".File.ext", ".File", ".ext"),
                /* 6*/ ("Folder1/.Folder2/.File.ext", "Folder1/.Folder2/", "", "Folder1", ".Folder2", ".File.ext", ".File", ".ext"),
                /* 7*/ ("/Folder1/.Folder2/.File.ext", "/Folder1/.Folder2/", "/", "Folder1", ".Folder2", ".File.ext", ".File", ".ext"),
                /* 8*/ ("../.Folder2/.File.ext", "../.Folder2/", "", "..", ".Folder2", ".File.ext", ".File", ".ext"),
                
                /* 9*/ ("C:/folder/", "C:/folder/", "C:/", "folder", null, "", "", ""),
            ];

            int i = 0;
            foreach (var (path, dir, vol, folder1, folder2, file, name, ext) in test_strings)
            {
                var match = Paths.Rx_Path().Match(path);
                Assert.AreEqual(dir, match.Groups["dir"].Value, $"dir mismatch {i}");
                Assert.AreEqual(vol, match.Groups["vol"].Value, $"vol mismatch {i}");
                Assert.AreEqual(file, match.Groups["file"].Value, $"file mismatch {i}");
                Assert.AreEqual(name, match.Groups["name"].Value, $"name mismatch {i}");
                Assert.AreEqual(ext, match.Groups["ext"].Value, $"ext mismatch {i}");

                var cap = match.Groups["folder"].Captures;
                Assert.AreEqual(cap.Count > 0 ? cap[0].Value : null, folder1, $"folder1 mismatch {i}");
                Assert.AreEqual(cap.Count > 1 ? cap[1].Value : null, folder2, $"folder2 mismatch {i}");
                i++;
            }
        }

        [TestMethod]
        public void Test_Paths_GetUniqueFilename()
        {
            File.WriteAllText("1.txt", "");
            Assert.AreEqual("1(1).txt", Paths.GetUniqueFilename("1.txt"));

            File.WriteAllText("2.txt", "");
            File.WriteAllText("2(1).txt", "");
            Assert.AreEqual("2(2).txt", Paths.GetUniqueFilename("2.txt"));

            Directory.CreateDirectory("3");
            Assert.AreEqual("3(1)", Paths.GetUniqueFilename("3"));

            Directory.CreateDirectory("4");
            Directory.CreateDirectory("4(1)");
            Assert.AreEqual("4(2)", Paths.GetUniqueFilename("4"));
        }

        [TestMethod]
        public void Test_Paths_Split()
        {
            CollectionAssert.AreEqual((string[])["C:\\WINDOWS\\system32", "C:\\Temp;2"], Paths.Split("C:\\WINDOWS\\system32;\"C:\\Temp;2\""));
            CollectionAssert.AreEqual((string[])["", "", ""], Paths.Split(";;"));
            CollectionAssert.AreEqual((string[])["", "", ""], Paths.Split("\"\";;"));
            CollectionAssert.AreEqual((string[])["", "", ""], Paths.Split(";\"\";"));
            CollectionAssert.AreEqual((string[])["", "", ""], Paths.Split(";;\"\""));
            CollectionAssert.AreEqual((string[])[], Paths.Split(""));
            CollectionAssert.AreEqual((string[])[], Paths.Split(null));
        }
    }
}
