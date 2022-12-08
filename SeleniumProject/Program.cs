using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;

namespace SeleniumProject
{
    class Program
    {
        private static List<string> _persons = new List<string>();

        static void Main(string[] args)
        {
            IWebDriver driver = null;
            driver = new ChromeDriver();

            /*
            driver.Url = "https://localhost:44378/Account/Register";
            driver.FindElement(By.Id("Email")).SendKeys("vincent.li@ie.com.au");
            driver.FindElement(By.Id("Password")).SendKeys("Password@1");
            driver.FindElement(By.Id("ConfirmPassword")).SendKeys("Password@1");
            driver.FindElement(By.CssSelector("input[value='Register']")).Click();
            */

            driver.Url = "https://localhost:44378/Account/Login";

            // Find the element that's ID attribute is 'account'(My Account) 
            
            driver.FindElement(By.Id("Email")).SendKeys("vincent.li@ie.com.au");
            driver.FindElement(By.Id("Password")).SendKeys("Password@1");
            driver.FindElement(By.CssSelector("input[value='Log in']")).Click();
            

            driver.Url = "https://localhost:44378/People";
            var fullHtml = driver.PageSource;

            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtml);
            ProcessPerson(doc.DocumentNode);
            ProcessSubjectTable(doc.DocumentNode.SelectNodes("//table[@class='subject']"));
            
            Console.ReadLine();
        }

        private static void ProcessPerson(HtmlNode documentNode)
        {
            var tableNodes = documentNode.SelectNodes("//table[@class='table']");
            var firstTable = tableNodes[0];
            var chidldNodes = firstTable.ChildNodes[0];
            var tbody = GetTbodyNode(chidldNodes);

            foreach (var childNode in tbody.ChildNodes)
            {
                if (!string.Equals(childNode.Name, "tr", StringComparison.InvariantCultureIgnoreCase)) continue;

                ProcesssNode(childNode);
            }
        }

        private static HtmlNode GetTbodyNode(HtmlNode node)
        {
            if (string.Equals(node.Name, "tbody")) return node;

            return GetTbodyNode(node.NextSibling);
        }

        private static void ProcesssNode(HtmlNode currentNode)
        {
            var isLastNodes = currentNode.ChildNodes.Count == 0;
            if (!isLastNodes)
            {
                foreach (var childNode in currentNode.ChildNodes)
                {
                    ProcesssNode(childNode);
                }
                return;
            }

            var isPersonNode = HasSiblingOfClass(currentNode.ParentNode.NextSibling, "subject", false);

            if (isPersonNode && !string.IsNullOrWhiteSpace(currentNode.InnerText))
            {
                _persons.Add(currentNode.InnerText);
            }
        }

        public static bool HasSiblingOfClass(HtmlNode currentNode, string @class, bool found)
        {
            if (currentNode == null) return false;

            if (currentNode.HasClass(@class))
            {
                return true;
            }

            if (currentNode.NextSibling != null)
            {
                found = found || HasSiblingOfClass(currentNode.NextSibling, @class, found);
            }

            return found; 
        }

        private static void ProcessSubjectTable(HtmlNodeCollection tableNodes)
        {
            if (tableNodes == null) return;

            var personCounter = 0;

            Dictionary<string, int> scores;
            List<string> subjects;
            
            foreach (HtmlNode tableNode in tableNodes)
            {
                scores = new Dictionary<string, int>();
                subjects = new List<string>();
                var counter = 0;
                ReadSubjectNode(tableNode, scores, subjects, ref counter, ref personCounter);
                Console.WriteLine("-------------------------------------");
                PrintSubjectScores(scores, ref personCounter);
                Console.WriteLine();
            }

            personCounter++;
        }

        private static void PrintSubjectScores(Dictionary<string, int> scores, ref int personCounter)
        {
            Console.WriteLine(_persons[personCounter]);
            foreach (var keyValue in scores)
            {
                Console.WriteLine($"\tsubject '{keyValue.Key}' score = {keyValue.Value}");
            }
            personCounter++;
        }

        private static void ReadSubjectNode(HtmlNode currentNode, Dictionary<string, int> scores, List<string> subjects, ref int counter, ref int personCounter)
        {
            var isLastNode = currentNode.ChildNodes.Count == 0;

            if (!isLastNode)
            {
                foreach (var htmlNode in currentNode.ChildNodes)
                {
                    ReadSubjectNode(htmlNode, scores, subjects, ref counter, ref personCounter);
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(currentNode.InnerText)) return;

            var value = currentNode.InnerText;

            if (int.TryParse(value, out var parseInteger))
            {
                scores[subjects[counter]] = parseInteger;
                counter++;
            }
            else
            {
                scores.Add(value, -1);
                subjects.Add(value);
            }
        }
    }
}
