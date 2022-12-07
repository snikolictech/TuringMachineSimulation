#region Synopsis
    //The following is a simulated Turing Machine, which reads rules and contents from an existing virtual tape machine (rules.txt), and then writes the results to the virtual tape (console output)
    //This was a pure exercise under time constraint and not fully optimized. While not tremendously useful, I found that understanding the conceptual framework behind a "universal turing machine" (IE: a computer) has instructive value.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TuringMachine
{
    internal class Program
    {
        private static void Main()
        {
            //Reads rules from file and creates ruleset
            FileReader.MethodSwitch method;

            method = FileReader.MethodSwitch.SkipFirst;
            var ruleCons = new FileReader(method, 4);

            method = FileReader.MethodSwitch.TakeFirst;
            var settCons = new FileReader(method, 4);

            var rules = new Rule[ruleCons.Element.Count];
            rules = Rule.CreateRuleSet(rules, ruleCons.Element);


            //Executes the commands based on given ruleset, tape contents
            var tapeMachine = new TapeMachine()
            {
                OffsetPos = int.Parse(settCons.Element[1][0]),
                State = settCons.Element[2][0],
                Halt = settCons.Element[3][0]
            };

            var inits = settCons.ParseInits(settCons.Element[0][0]);
            tapeMachine.TapeContentsR = tapeMachine.InitFrom(inits);
            tapeMachine.Run(rules);

            Console.ReadLine();
        }
    }

    internal class FileReader
    {
        public enum MethodSwitch
        {
            SkipFirst,
            TakeFirst
        };

        private readonly StreamReader sr = new StreamReader("rules.txt");
        public List<string[]> Element = new List<string[]>();

        public FileReader(MethodSwitch method, int amount)
        {
            string line;
            var entries = new List<string>();

            while ((line = sr.ReadLine()) != null)
                entries.Add(line);

            if (method == MethodSwitch.SkipFirst)
            {
                foreach (var entry in entries.Skip(amount))
                    Element.Add(entry.Split());
            }
            else if (method == MethodSwitch.TakeFirst)
            {
                foreach (var entry in entries.Take(amount))
                    Element.Add(entry.Split());
            }
        }

        public string[] ParseInits(string inits)
        {
            var Inits = new string[inits.Length];

            for (var i = 0; i < inits.Length; i++)
                Inits[i] = inits[i].ToString();

            return Inits;
        }
    }

    internal class Rule
    {
        public string move;
        public string newState;
        public string read;
        public string state;
        public string write;

        public Rule() {}

        public Rule(string state, string read, string write, string move, string newState)
        {
            this.state = state;
            this.read = read;
            this.write = write;
            this.move = move;
            this.newState = newState;
        }

        public static Rule[] CreateRuleSet(Rule[] RuleArray, List<string[]> Element)
        {
            var lineCounter = 0;
            foreach (var ruleLine in Element)
            {
                RuleArray[lineCounter] = new Rule(ruleLine[0], ruleLine[1], ruleLine[2], ruleLine[3], ruleLine[4]);
                lineCounter++;
            }

            return RuleArray;
        }
    }

    internal class TapeMachine
    {
        public string Halt;
        public int OffsetPos;
        public string State;
        
        public List<string> TapeContentsR = new List<string>();
        private readonly List<string> TapeContentsL = new List<string>();

        private List<string> tapeContents; //internal reference to TapeContentsR / TapeContentsL

        public void Run(Rule[] ruleArray)
        {
            var currentRule = new Rule();

            while (State != Halt)
            {
                tapeContents = (OffsetPos < 0) ? TapeContentsL : TapeContentsR;

                if (Math.Abs(OffsetPos) == tapeContents.Count - 1) //increase collection if needed
                    tapeContents.Add("-");

                for (var i = 0; i < ruleArray.Length; i++)
                {
                    currentRule = ruleArray[i];

                    if (State == currentRule.state)
                    {
                        if (currentRule.read == tapeContents[Math.Abs(OffsetPos)])
                        {
                            tapeContents[Math.Abs(OffsetPos)] = currentRule.write;

                            OffsetPos += int.Parse(currentRule.move);
                            State = currentRule.newState;
                            printTape();
                        }
                        if (currentRule.read == "*")
                        {
                            OffsetPos += int.Parse(currentRule.move);
                            State = currentRule.newState;
                        }
                    }
                }
            }
        }

        public List<string> InitFrom(string[] Inits)
        {
            var temp = new List<string>();

            foreach (var entry in Inits)
                temp.Add(entry);

            return temp;
        }

        private void printTape()
        {
            for (var c = TapeContentsL.Count - 1; c > 0; c--) //print left side of tape
                Console.Write(TapeContentsL[c] + " ");

            for (var c = 0; c < TapeContentsR.Count - 1; c++) //print right side of tape
                Console.Write(TapeContentsR[c] + " ");

            Console.WriteLine();
        }
    }
}