using System;
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

        protected abstract UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options);

        private static readonly char[] Separators =
        {
            ' ',
        };


        public delegate void SubmitLineAction(string line, string colorCode, bool allowOverflow);

        public delegate void SubmitErrorAction(string message);

        public async UniTask Run(IUnishPresenter shell, string op, string argsNotParsed)
        {
            var isError = false;
            var dic     = new Dictionary<string, UnishCommandArg>();
            var options = new Dictionary<string, UnishCommandArg>();

            if (!RequiresPreParseArguments)
            {
                if (!AllowTrailingNullParams && string.IsNullOrWhiteSpace(argsNotParsed))
                {
                    await SubmitUsage(op, shell.IO);
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
                        await shell.IO.WriteErrorAsync(new Exception("Invalid Option."));
                        await SubmitUsage(op, shell.IO);
                        return;
                    }
                }
                else if (j >= Params.Length)
                {
                    await shell.IO.WriteErrorAsync(new Exception("Too many arguments."));
                    await SubmitUsage(op, shell.IO);
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
                            await shell.IO.WriteErrorAsync(new Exception($"Argument <{Params[j].name}> is required."));
                            isError = true;
                        }
                    }
                    else
                    {
                        dic[Params[j].name] = new UnishCommandArg(Params[j].type, Params[j].defVal);
                        if (dic[Params[j].name].Type == UnishCommandArgType.Error)
                        {
                            await shell.IO.WriteErrorAsync(new Exception($"Type mismatch: {Params[j]} is not {Params[j].type}."));
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
                        await shell.IO.WriteErrorAsync(new Exception($"Type mismatch: {args[i]} is not {Params[j].type}."));
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
                        await shell.IO.WriteErrorAsync(new Exception($"Argument <{Params[j].name}> is required."));
                        isError = true;
                    }
                }
                else
                {
                    dic[Params[j].name] = new UnishCommandArg(Params[j].type, Params[j].defVal);
                    if (dic[Params[j].name].Type == UnishCommandArgType.Error)
                    {
                        await shell.IO.WriteErrorAsync(new Exception($"Type mismatch: {Params[j]} is not {Params[j].type}."));
                        isError = true;
                    }
                }
            }

            if (isError)
            {
                await SubmitUsage(op, shell.IO);
                return;
            }

            await Run(shell, op, dic, options);
        }

        public async UniTask SubmitUsage(IUnishIO io, bool drawTopLine = true, bool drawBottomLine = true)
        {
            await SubmitUsage(Ops[0], io, drawTopLine, drawBottomLine);
        }

        public async UniTask SubmitUsage(string op, IUnishIO io, bool drawTopLine = true,
            bool drawBottomLine = true)
        {
            if (drawTopLine)
            {
                await io.WriteLineAsync("+-----------------------------+", "#aaaaaa");
            }

            var i = 0;

            var optionString = Options?.Length > 0 ? "[<color=#ff7777>options</color>] " : "";

            var argString = Params.ToSingleString(i++ % 3 == 2 ? "\n" + new string(' ', op.Length + 1) : " ",
                toString: x =>
                    x.defVal == null
                        ? $"<<color=cyan>{x.type}</color> <color=#77ff77>{x.name}</color>>"
                        : $"<<color=cyan>{x.type}</color> <color=#77ff77>{x.name}</color>=<color=#aaaaaa>{(x.defVal == "" ? "\"\"" : x.defVal)}</color>>");

            await io.WriteLineAsync($"<color=yellow>{op}</color> {optionString}{argString}");

            var labelWidth = 20;
            if (Params.Length > 0 && Params.Any(p => !string.IsNullOrEmpty(p.info)))
            {
                await io.WriteLineAsync("<color=#aaaaaa>params:</color>");
                foreach (var p in Params)
                {
                    await io.WriteLineAsync(
                        $" <color=#aaaaaa><color=#77ff77>{p.name.PadRight(labelWidth)}</color>{p.info}</color>");
                }
            }

            if (Options?.Length > 0)
            {
                await io.WriteLineAsync("options", "#aaaaaa");
                foreach (var option in Options)
                {
                    if (option.type == UnishCommandArgType.None)
                    {
                        await io.WriteColoredAsync(
                            $" <color=#ff7777>-{option.name.PadRight(labelWidth - 1)}</color>{option.info}", "#aaaaaa"
                            );
                    }
                    else
                    {
                        var padding = labelWidth - 1 - option.name.Length - 3 - option.type.ToString().Length;
                        await io.WriteColoredAsync(
                            $" <color=#ff7777>-{option.name}</color> <<color=cyan>{option.type}</color>>" +
                            new string(' ', padding) + option.info, "#aaaaaa");
                    }
                }

                await io.WriteColoredAsync("", "#aaaaaa");
            }

            var usage = Usage(op);
            if (!string.IsNullOrEmpty(usage))
            {
                await io.WriteLineAsync(usage, "#aaaaaa");
            }

            if (drawBottomLine)
            {
                await io.WriteLineAsync("+-----------------------------+", "#aaaaaa");
            }
        }
    }
}
