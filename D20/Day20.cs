using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode;

public class Day20
{

    public void Execute()
    {
        var moduleMap1 = ParseInput("D20/input.txt");
        var moduleMap2 = ParseInput("D20/input.txt"); // This allows us to mutate the maps... and was easier to do that proper reset

        // This shows that the graph ends with 4 modules feeding into single conj. Maybe LCM can help?
        // 
        //PrintMermaidGraph(moduleMap);

        int low = 0;
        int high = 0;
        for (int i = 0; i < 1000; i++)
        {
            var (lowPulses, highPulses) = PulseButton(moduleMap1);

            checked
            {
                low += lowPulses;
                high += highPulses;
            }
        }

        checked
        {
            Console.WriteLine($"Day 20 I : {low * high}");
        }


        var fastestButton = ComputeNeededMachinePressed(moduleMap2);
        Console.WriteLine($"Day 20 II: {fastestButton}");
    }

    private static void PrintMermaidGraph(Dictionary<string, Module> moduleMap)
    {
        Console.WriteLine("stateDiagram-v2\r\n    classDef out fill:#a13a3a,color:white\r\n    classDef flip fill:#286325,color:white\r\n    classDef conj fill:#252f63,color:white\r\n\r\n    button --> broadcaster");
        foreach (var (key, module) in moduleMap)
        {
            for (int i = 0; i < module.Outputs.Count; i++)
            {
                Console.WriteLine($"    {module.Name} --> {module.Outputs[i].Name}");

                if (module.Name == "rx")
                    Console.WriteLine($"    class {module.Name} out");
                else if (module.Mode == ModuleMode.FlipFlop)
                    Console.WriteLine($"    class {module.Name} flip");
                else if (module.Mode == ModuleMode.Conj)
                    Console.WriteLine($"    class {module.Name} conj");

            }
        }
    }

    private long ComputeNeededMachinePressed(Dictionary<string, Module> moduleMap)
    {
        if (!moduleMap.TryGetValue("rx", out var machineModule))
        {
            Console.WriteLine("INCOMPATIBLE INPUT: There is no 'rx' module");
            return -1;
        }

        if (machineModule.Inputs.Count != 1 || machineModule.Inputs[0].Mode != ModuleMode.Conj)
        {
            Console.WriteLine("INCOMPATIBLE INPUT: 'rx' has more than one conj input");
            return -1;
        }

        var parent = machineModule.Inputs[0];
        var keyModules = new List<Module>(parent.Inputs.Count);

        foreach (var k in parent.Inputs)
        {
            if (k.Inputs.Count != 1 || k.Mode != ModuleMode.Conj)
            {
                Console.WriteLine("INCOMPATIBLE INPUT: Parent 'rx' has unexpected input");
                return -1;
            }

            keyModules.Add(k);
        }

        List<int> loops = new(keyModules.Count);

        int presses = 0;
        while (keyModules.Count > 0)
        {
            presses += 1;
            var _ = PulseButton(moduleMap);

            for (int ki = keyModules.Count - 1; ki >= 0; ki--)
            {
                if (keyModules[ki].ReceivedLowPulseEver)
                {
                    keyModules.RemoveAt(ki);
                    loops.Add(presses);
                }
            }
        }

        return loops.Aggregate(1L, (a, b) => lcm(a, b));
    }
    static long gcf(long a, long b)
    {
        checked
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
        }
        return a;
    }

    static long lcm(long a, long b)
    {
        checked
        {
            return (a / gcf(a, b)) * b;
        }
    }

    private Dictionary<string, Module> ParseInput(string path)
    {
        var stream = File.OpenText(path);

        List<ModuleRaw> readModules = new();
        HashSet<string> knownModules = new();
        HashSet<string> mentionedModules = new();

        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine()!;

            var match = Regex.Match(line, @"([%&]?)(\w+)\s->\s(.+)");
            var mode = match.Groups[1].Value switch
            {
                "%" => ModuleMode.FlipFlop,
                "&" => ModuleMode.Conj,
                "" => ModuleMode.Relay,
                _ => throw new Exception()
            };
            var name = match.Groups[2].Value;

            var targets = match.Groups[3].Value.Split(", ");

            foreach (var t in targets)
            {
                if (!knownModules.Contains(t))
                    mentionedModules.Add(t);
            }

            mentionedModules.Remove(name);
            knownModules.Add(name);

            readModules.Add(new ModuleRaw(name, mode, targets));
        }

        Dictionary<string, Module> moduleMap = new();

        foreach (var m in readModules)
        {
            moduleMap.Add(m.Name, new Module(m.Name, m.Mode));
        }

        foreach (var m in mentionedModules)
        {
            moduleMap.Add(m, new Module(m, ModuleMode.Relay));
        }

        foreach (var raw in readModules)
        {
            var m = moduleMap[raw.Name];
            for (int i = 0; i < raw.Targets.Length; i++)
            {
                var output = moduleMap[raw.Targets[i]];
                output.Inputs.Add(m);
                m.Outputs.Add(output);
            }
        }

        return moduleMap;
    }

    private (int lowPulses, int highPulses) PulseButton(Dictionary<string, Module> moduleMap)
    {
        int lowPulses = 0;
        int highPulses = 0;
        Queue<Pulse> pulses = new();
        pulses.Enqueue(new("button", moduleMap["broadcaster"], false));

        while (pulses.Count > 0)
        {
            var pulse = pulses.Dequeue();
            //Console.WriteLine($"{pulse.Source} -{(pulse.isHigh ? "high" : "low")}-> {pulse.Target.Name}");

            pulse.Target.Process(pulse.isHigh, pulses);

            if (pulse.isHigh) highPulses += 1;
            else lowPulses += 1;
        }

        //StringBuilder b = new StringBuilder();

        //foreach (var (_, m) in moduleMap)
        //{
        //    b.Append(m.FlipFlopState ? '1' : '0');
        //    b.Append(m.LastSentPulse ? '1' : '0');
        //}

        ////Console.WriteLine(b.ToString());

        //bool machineEnabled = false;
        //if (moduleMap.TryGetValue("rx", out Module? rxM))
        //{
        //    machineEnabled = rxM.ReceivedLowPulseEver;
        //}

        return (lowPulses, highPulses);
    }

    record struct Pulse(string Source, Module Target, bool isHigh);

    record ModuleRaw(string Name, ModuleMode Mode, string[] Targets);
    enum ModuleMode { Relay, FlipFlop, Conj }

    class Module
    {
        public readonly string Name;
        public readonly ModuleMode Mode;
        public List<Module> Outputs = new();
        public List<Module> Inputs = new();

        public bool FlipFlopState { get; private set; } = false;
        public bool LastSentPulse { get; private set; } = false;
        public bool ReceivedLowPulseEver { get; private set; } = false;

        public Module(string name, ModuleMode mode)
        {
            Name = name;
            Mode = mode;
        }

        internal void Process(bool isHigh, Queue<Pulse> pulses)
        {
            if (!isHigh) ReceivedLowPulseEver = true;

            switch (Mode)
            {
                case ModuleMode.Relay:
                    {
                        LastSentPulse = isHigh;
                        for (int i = 0; i < Outputs.Count; i++)
                        {
                            pulses.Enqueue(new(Name, Outputs[i], isHigh));
                        }
                    }
                    break;
                case ModuleMode.FlipFlop:
                    {
                        if (isHigh) return;
                        FlipFlopState = !FlipFlopState;

                        LastSentPulse = FlipFlopState;
                        for (int i = 0; i < Outputs.Count; i++)
                        {
                            pulses.Enqueue(new(Name, Outputs[i], FlipFlopState));
                        }

                    }
                    break;
                case ModuleMode.Conj:
                    /* No clue */
                    {
                        var pulseToSend = !Inputs.TrueForAll(m => m.LastSentPulse);
                        LastSentPulse = pulseToSend;
                        for (int i = 0; i < Outputs.Count; i++)
                        {
                            pulses.Enqueue(new(Name, Outputs[i], pulseToSend));
                        }
                    }
                    break;
            }
        }
    }
}

