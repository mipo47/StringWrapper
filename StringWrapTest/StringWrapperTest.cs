using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using StringWrap;

namespace StringWrapTest
{
    public class TestCase
    {
        public string Name { get; set; }

        public int MaxLength { get; set; }
        public string Input { get; set; }

        public string Output { get; set; }
        public string Expect { get; set; }
    }

    [TestFixture]
    public class StringWrapperTest
    {
        StringWrapper textWrapper;

        [SetUp]
        public void Init()
        {
            textWrapper = new StringWrapper();
        }

        [Test, TestCaseSource("TestCases")]
        public void WordWrap(TestCase testCase)
        {
            testCase.Output = textWrapper.Wrap(testCase.Input, testCase.MaxLength);

            SaveTestResults(testCase);

            // Test line length limitation
            var lines = testCase.Output.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.None
            );
            foreach (string line in lines)
                Assert.LessOrEqual(line.Length, testCase.MaxLength, line);

            Assert.AreEqual(testCase.Expect, testCase.Output, "Unexpected wrap result");
        }

        [Test]
        public void TabsToSpaces()
        {
            string result = textWrapper.Wrap("\tVALUE =\t1", 100);
            Assert.AreEqual("    VALUE =    1", result);

            textWrapper.TabSize = 2;
            result = textWrapper.Wrap("\tVALUE =\t1", 100);
            Assert.AreEqual("  VALUE =  1", result);
        }

        [Test]
        public void UnixToWindows()
        {
            textWrapper.NewLine = "\r\n";
            string result = textWrapper.Wrap("line1\nline2\n", 100);
            string expected = string.Join("\r\n", new string[] {
                "line1",
                "line2",
                ""
            });
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WindowsToUnix()
        {
            textWrapper.NewLine = "\n";
            string result = textWrapper.Wrap("line1\r\nline2\r\n", 100);
            string expected = string.Join("\n", new string[] {
                "line1",
                "line2",
                ""
            });
            Assert.AreEqual(expected, result);
        }

        #region Helpers
        static string ProjectRoot
        {
            get
            {
                return Path.Combine(TestContext.CurrentContext.TestDirectory, "../..");
            }
        }

        static IEnumerable<TestCaseData> TestCases()
        {
            string testCasesPath = Path.Combine(ProjectRoot, "TestCases");
            foreach (var file in Directory.GetFiles(testCasesPath))
            {
                var content = File.ReadAllText(file);
                int textEnd = content.IndexOf("\n## WRAP_", StringComparison.InvariantCulture);
                if (textEnd < 1)
                    continue;

                bool isUnixLineEnding = content[textEnd - 1] != '\r';

                int maxLength;
                if (int.TryParse(content.Substring(textEnd + 9, 3), out maxLength))
                {
                    var testCase = new TestCase
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Input = content.Substring(0, textEnd - (isUnixLineEnding ? 0 : 1)),
                        MaxLength = maxLength,
                        Expect = content.Substring(textEnd + (isUnixLineEnding ? 16 : 17))
                    };
                    var testCaseData = new TestCaseData(testCase);
                    testCaseData.SetName(testCase.Name);
                    yield return testCaseData;
                }
            }
        }

        static void SaveTestResults(TestCase testCase)
        {
            File.WriteAllText($"{ProjectRoot}/Results/{testCase.Name}.{testCase.MaxLength}.input.txt", testCase.Input);
            File.WriteAllText($"{ProjectRoot}/Results/{testCase.Name}.{testCase.MaxLength}.output.txt", testCase.Output);
            File.WriteAllText($"{ProjectRoot}/Results/{testCase.Name}.{testCase.MaxLength}.expect.txt", testCase.Expect);
        }
        #endregion
    }
}
