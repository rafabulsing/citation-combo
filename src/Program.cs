using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WikiDumpParser;
using WikiDumpParser.Models;
using Newtonsoft.Json;

namespace scanner
{
    class ReferenceCombo {
        public int combo;
        public String lastReference;
        public String pageTitle;

        public ReferenceCombo(int combo, String lastReference, String pageTitle) {
            this.combo = combo;
            this.lastReference = lastReference;
            this.pageTitle = pageTitle;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var parser = Parser.Create(File.Open("../data.xml", FileMode.Open));
            
            var biggestCombo = 1;
            Page bestPage = null;
            var regex = new Regex(@"(<ref.*?(>.*?</ref>|/>)){1}", RegexOptions.Singleline);
            var index = 0;
            var combos = new List<ReferenceCombo>();

            foreach (var page in parser.ReadPages()) {
                if (index % 500000 == 0) {
                    Console.WriteLine($"Page #{index}");
                }
                index++;

                var referencesSection = new Regex(@"==References==").Match(page.Text);
                var text = referencesSection.Success ? page.Text.Substring(0, referencesSection.Index) : page.Text;

                var refs = regex.Matches(text);

                var consecutive = getConsecutiveRefs(refs);

                if (consecutive.combo > biggestCombo) {
                    biggestCombo = consecutive.combo;
                    bestPage = page;
                    Console.WriteLine($"Best yet: {page.Title} (Combo: {biggestCombo}, \"{consecutive.lastMatch}\")\n");
                } else if (consecutive.combo == biggestCombo) {
                    Console.WriteLine($"Tie: {page.Title} (Combo: {biggestCombo}, \"{consecutive.lastMatch}\")\n");
                }

                if (consecutive.combo > 20) {
                    Console.WriteLine($"Added to list: {page.Title} (Combo: {consecutive.combo}, \"{consecutive.lastMatch}\")\n");
                    combos.Add(new ReferenceCombo(consecutive.combo, consecutive.lastMatch, page.Title));
                }
            }

            combos.Sort(async (a, b) => b.combo - a.combo);

            var json = JsonConvert.SerializeObject(combos);

            File.WriteAllText("../output.json", json);

            Console.WriteLine("Finished!");
        }

        static (int combo, String lastMatch) getConsecutiveRefs(MatchCollection matches) {
            var combo = 0;
            var lastIndex = 0;
            var biggestCombo = 0;
            String biggestComboLastEntry = null;


            foreach (Match match in matches) {
                // Console.WriteLine($"{match.Index}, {lastIndex}");

                if (lastIndex == 0 || match.Index == lastIndex) {
                    combo++;
                    lastIndex += match.Length;
                    if (combo > biggestCombo) {
                        biggestComboLastEntry = match.Value;
                    }
                } else {
                    // Console.WriteLine("Reseted");
                    lastIndex = match.Index + match.Length;
                    if (combo > biggestCombo) {
                        biggestCombo = combo;
                    }
                    combo = 1;
                }
            }

            return (biggestCombo, biggestComboLastEntry);
        }
    }
}
