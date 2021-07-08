using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public abstract class UnishCommandBase
    {
        public abstract string[] Ops { get; }

        public virtual string[] Aliases { get; } =
        {
        };

        public abstract (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; }

        public virtual (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
        };

        public virtual string Usage(string op)
        {
            return "";
        }

        public virtual bool RequiresPreParseArguments => true;

        public virtual bool AllowTrailingNullParams => false;

        protected abstract UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options);

        private static readonly char[] Separators =
        {
            ' ',
        };


        public delegate void SubmitLineAction(string line, string colorCode, bool allowOverflow);

        public delegate void SubmitErrorAction(string message);

        public async UniTask Run(IUnish shell, string op, string argsNotParsed, SubmitLineAction submitter,
            SubmitErrorAction errorSubmitter)
        {
            var isError = false;
            var dic     = new Dictionary<string, UnishCommandArg>();
            var options = new Dictionary<string, UnishCommandArg>();

            if (!RequiresPreParseArguments)
            {
                if (!AllowTrailingNullParams && string.IsNullOrWhiteSpace(argsNotParsed))
                {
                    SubmitUsage(op, submitter);
                    return;
                }

                dic[""] = new UnishCommandArg(UnishCommandArgType.String, argsNotParsed ?? "");
                await Run(shell, op, dic, options);
                return;
            }

            var isInString = false;
            var header     = 0;
            var argList    = new List<string>();
            {
                var i = 0;
                for (; i < argsNotParsed.Length; i++)
                {
                    if (Separators.Contains(argsNotParsed[header]))
                    {
                        header++;
                        continue;
                    }

                    var c = argsNotParsed[i];
                    switch (c)
                    {
                        case '"':
                            {
                                isInString = !isInString;
                                break;
                            }
                        case var space when Separators.Contains(space) && !isInString:
                            {
                                //      h   i
                                // 0   4    9
                                // hoge fuga piyo
                                argList.Add(argsNotParsed.Substring(header, i - header));
                                header = i + 1;
                                break;
                            }
                    }
                }

                if (header < i)
                {
                    argList.Add(argsNotParsed.Substring(header));
                }
            }
            var args = argList.ToArray();

            var j = 0;
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-") && !float.TryParse(args[i], out var _))
                {
                    var found = false;
                    for (var k = 0; k < Options.Length; k++)
                    {
                        if (Options[k].name == args[i].Substring(1))
                        {
                            var optionArg = UnishCommandArg.None;
                            if (Options[k].type != UnishCommandArgType.None)
                            {
                                if (i == args.Length - 1 || (args[i + 1].StartsWith("-") &&
                                                             !float.TryParse(args[i + 1], out var _)))
                                {
                                    optionArg = new UnishCommandArg(Options[k].type, Options[k].defVal);
                                }
                                else
                                {
                                    optionArg = new UnishCommandArg(Options[k].type, args[i + 1]);
                                    i++;
                                }
                            }

                            options[Options[k].name] = optionArg;
                            found                    = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        errorSubmitter("Invalid Option.");
                        SubmitUsage(op, submitter);
                        return;
                    }
                }
                else if (j >= Params.Length)
                {
                    errorSubmitter("Too many arguments.");
                    SubmitUsage(op, submitter);
                    return;
                }
                else if (args[i] == "_")
                {
                    if (string.IsNullOrEmpty(Params[j].defVal))
                    {
                        if (AllowTrailingNullParams)
                        {
                            dic[Params[j].name] = new UnishCommandArg(Params[j].type, Params[j].defVal);
                        }
                        else
                        {
                            errorSubmitter($"Argument <{Params[j].name}> is required.");
                            isError = true;
                        }
                    }
                    else
                    {
                        dic[Params[j].name] = new UnishCommandArg(Params[j].type, Params[j].defVal);
                        if (dic[Params[j].name].Type == UnishCommandArgType.Error)
                        {
                            errorSubmitter($"Type mismatch: {Params[j]} is not {Params[j].type}.");
                            isError = true;
                        }
                    }

                    j++;
                }
                else
                {
                    dic[Params[j].name] = new UnishCommandArg(Params[j].type, args[i]);
                    if (dic[Params[j].name].Type == UnishCommandArgType.Error)
                    {
                        errorSubmitter($"Type mismatch: {args[i]} is not {Params[j].type}.");
                        isError = true;
                    }

                    j++;
                }
            }

            for (; j < Params.Length; j++)
            {
                if (Params[j].defVal == null)
                {
                    if (AllowTrailingNullParams)
                    {
                        dic[Params[j].name] = new UnishCommandArg(Params[j].type, Params[j].defVal);
                    }
                    else
                    {
                        errorSubmitter($"Argument <{Params[j].name}> is required.");
                        isError = true;
                    }
                }
                else
                {
                    dic[Params[j].name] = new UnishCommandArg(Params[j].type, Params[j].defVal);
                    if (dic[Params[j].name].Type == UnishCommandArgType.Error)
                    {
                        errorSubmitter($"Type mismatch: {Params[j]} is not {Params[j].type}.");
                        isError = true;
                    }
                }
            }

            if (isError)
            {
                SubmitUsage(op, submitter);
                return;
            }

            await Run(shell, op, dic, options);
        }

        public void SubmitUsage(SubmitLineAction submitter, bool drawTopLine = true, bool drawBottomLine = true)
        {
            SubmitUsage(Ops[0], submitter, drawTopLine, drawBottomLine);
        }

        public void SubmitUsage(string op, SubmitLineAction submitter, bool drawTopLine = true,
            bool drawBottomLine = true)
        {
            if (drawTopLine)
            {
                submitter("+-----------------------------+", "#aaaaaa", false);
            }

            var i = 0;

            var optionString = Options?.Length > 0 ? "[<color=#ff7777>options</color>] " : "";

            var argString = Params.ToSingleString(i++ % 3 == 2 ? "\n" + new string(' ', op.Length + 1) : " ",
                toString: x =>
                    x.defVal == null
                        ? $"<<color=cyan>{x.type}</color> <color=#77ff77>{x.name}</color>>"
                        : $"<<color=cyan>{x.type}</color> <color=#77ff77>{x.name}</color>=<color=#aaaaaa>{(x.defVal == "" ? "\"\"" : x.defVal)}</color>>");

            submitter($"<color=yellow>{op}</color> {optionString}{argString}", "orange",
                true);

            var labelWidth = 20;
            if (Params.Length > 0 && Params.Any(p => !string.IsNullOrEmpty(p.info)))
            {
                submitter("params:", "#aaaaaa", false);
                foreach (var p in Params)
                {
                    submitter(
                        $" <color=#77ff77>{p.name.PadRight(labelWidth)}</color>{p.info}", "#aaaaaa",
                        true);
                }
            }

            if (Options?.Length > 0)
            {
                submitter("options:", "#aaaaaa", false);
                foreach (var option in Options)
                {
                    if (option.type == UnishCommandArgType.None)
                    {
                        submitter(
                            $" <color=#ff7777>-{option.name.PadRight(labelWidth - 1)}</color>{option.info}", "#aaaaaa",
                            true);
                    }
                    else
                    {
                        var padding = labelWidth - 1 - option.name.Length - 3 - option.type.ToString().Length;
                        submitter(
                            $" <color=#ff7777>-{option.name}</color> <<color=cyan>{option.type}</color>>" +
                            new string(' ', padding) + option.info, "#aaaaaa", true);
                    }
                }

                submitter("", "#aaaaaa", false);
            }

            var usage = Usage(op);
            if (!string.IsNullOrEmpty(usage))
            {
                submitter(usage, "#aaaaaa", false);
            }

            if (drawBottomLine)
            {
                submitter("+-----------------------------+", "#aaaaaa", false);
            }
        }
    }
}
