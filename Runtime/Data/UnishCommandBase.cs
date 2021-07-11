using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public abstract class UnishCommandBase
    {
        private   IUnishPresenter     mShell;
        protected IUnishEnv           Env         => mShell?.Env;
        protected IUnishIO            IO          => mShell?.IO;
        protected IUnishDirectoryRoot Directory   => mShell?.Directory;
        protected IUnishInterpreter   Interpreter => mShell?.Interpreter;

        public virtual bool RequiresPreParseArguments => true;
        public virtual bool AllowTrailingNullParams   => false;

        public abstract string[] Ops { get; }

        public virtual string[] Aliases { get; } =
        {
        };

        public abstract (UnishVariableType type, string name, string defVal, string info)[] Params { get; }

        public virtual (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
        };

        public virtual string Usage(string op)
        {
            return "";
        }

        protected UniTask RunNewCommandAsync(string cmd)
        {
            return Interpreter.RunCommandAsync(mShell, cmd);
        }

        protected abstract UniTask Run(
            string op,
            Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options);

        private static readonly char[] Separators =
        {
            ' ',
        };

        public async UniTask Run(IUnishPresenter shell, UnishParseResult parsed)
        {
            mShell = shell;
            var dParams  = new Dictionary<string, UnishVariable>();
            var dOptions = new Dictionary<string, UnishVariable>();

            int i;
            for (i = 0; i < Options.Length && i < parsed.Options.Count; i++)
            {
                var option      = Options[i];
                var optionTyped = new UnishVariable(option.name, option.type, parsed.Options[i]);
                if (optionTyped.Type == UnishVariableType.Error)
                {
                    await IO.WriteErrorAsync(new Exception($"Type mismatch: {parsed.Params[i]} is not {option.type}."));
                    await WriteUsage(parsed.Command);
                    return;
                }

                dOptions[option.name] = optionTyped;
            }

            for (i = 0; i < Params.Length && i < parsed.Params.Count; i++)
            {
                var param      = Params[i];
                var paramTyped = new UnishVariable(param.name, param.type, parsed.Params[i]);
                if (paramTyped.Type == UnishVariableType.Error)
                {
                    await IO.WriteErrorAsync(new Exception($"Type mismatch: {parsed.Params[i]} is not {param.type}."));
                    await WriteUsage(parsed.Command);
                    return;
                }

                dParams[param.name] = paramTyped;
            }

            for (; i < Params.Length; i++)
            {
                var param = Params[i];
                dParams[param.name] = new UnishVariable(param.name, param.type, param.defVal);
            }

            await Run(parsed.Command, dParams, dOptions);
        }

        public async UniTask Run(IUnishPresenter shell, string op, string argsNotParsed)
        {
            mShell = shell;
            var isError = false;
            var dic     = new Dictionary<string, UnishVariable>();
            var options = new Dictionary<string, UnishVariable>();

            if (!RequiresPreParseArguments)
            {
                if (!AllowTrailingNullParams && string.IsNullOrWhiteSpace(argsNotParsed))
                {
                    await WriteUsage(op);
                    return;
                }

                dic[""] = new UnishVariable("", UnishVariableType.String, argsNotParsed ?? "");
                await Run(op, dic, options);
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
                            var optionArg = UnishVariable.Unit(Options[k].name);
                            if (Options[k].type != UnishVariableType.Unit)
                            {
                                if (i == args.Length - 1 || (args[i + 1].StartsWith("-") &&
                                                             !float.TryParse(args[i + 1], out var _)))
                                {
                                    optionArg = new UnishVariable(Options[k].name, Options[k].type, Options[k].defVal);
                                }
                                else
                                {
                                    optionArg = new UnishVariable(Options[k].name, Options[k].type, args[i + 1]);
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
                        await IO.WriteErrorAsync(new Exception("Invalid Option."));
                        await WriteUsage(op);
                        return;
                    }
                }
                else if (j >= Params.Length)
                {
                    await IO.WriteErrorAsync(new Exception("Too many arguments."));
                    await WriteUsage(op);
                    return;
                }
                else if (args[i] == "_")
                {
                    if (string.IsNullOrEmpty(Params[j].defVal))
                    {
                        if (AllowTrailingNullParams)
                        {
                            dic[Params[j].name] = new UnishVariable(Params[j].name, Params[j].type, Params[j].defVal);
                        }
                        else
                        {
                            await IO.WriteErrorAsync(new Exception($"Argument <{Params[j].name}> is required."));
                            isError = true;
                        }
                    }
                    else
                    {
                        dic[Params[j].name] = new UnishVariable(Params[j].name, Params[j].type, Params[j].defVal);
                        if (dic[Params[j].name].Type == UnishVariableType.Error)
                        {
                            await IO.WriteErrorAsync(new Exception($"Type mismatch: {Params[j]} is not {Params[j].type}."));
                            isError = true;
                        }
                    }

                    j++;
                }
                else
                {
                    dic[Params[j].name] = new UnishVariable(Params[j].name, Params[j].type, args[i]);
                    if (dic[Params[j].name].Type == UnishVariableType.Error)
                    {
                        await IO.WriteErrorAsync(new Exception($"Type mismatch: {args[i]} is not {Params[j].type}."));
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
                        dic[Params[j].name] = new UnishVariable(Params[j].name, Params[j].type, Params[j].defVal);
                    }
                    else
                    {
                        await IO.WriteErrorAsync(new Exception($"Argument <{Params[j].name}> is required."));
                        isError = true;
                    }
                }
                else
                {
                    dic[Params[j].name] = new UnishVariable(Params[j].name, Params[j].type, Params[j].defVal);
                    if (dic[Params[j].name].Type == UnishVariableType.Error)
                    {
                        await IO.WriteErrorAsync(new Exception($"Type mismatch: {Params[j]} is not {Params[j].type}."));
                        isError = true;
                    }
                }
            }

            if (isError)
            {
                await WriteUsage(op);
                return;
            }

            await Run(op, dic, options);
        }

        public UniTask WriteUsage(IUnishIO io, bool drawTopLine = true, bool drawBottomLine = true)
        {
            return WriteUsageInternal(io, Ops[0], drawTopLine, drawBottomLine);
        }

        public UniTask WriteUsage(IUnishIO io, string op, bool drawTopLine = true, bool drawBottomLine = true)
        {
            return WriteUsageInternal(io, op ?? Ops[0], drawTopLine, drawBottomLine);
        }

        protected UniTask WriteUsage(bool drawTopLine = true, bool drawBottomLine = true)
        {
            return WriteUsage(Ops[0], drawTopLine, drawBottomLine);
        }

        protected UniTask WriteUsage(string op, bool drawTopLine = true, bool drawBottomLine = true)
        {
            return WriteUsageInternal(mShell.IO, op, drawTopLine, drawBottomLine);
        }

        private async UniTask WriteUsageInternal(IUnishIO io, string op, bool drawTopLine, bool drawBottomLine)
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
                    if (option.type == UnishVariableType.Unit)
                    {
                        await io.WriteLineAsync(
                            $" <color=#ff7777>-{option.name.PadRight(labelWidth - 1)}</color>{option.info}", "#aaaaaa"
                        );
                    }
                    else
                    {
                        var padding = labelWidth - 1 - option.name.Length - 3 - option.type.ToString().Length;
                        await io.WriteLineAsync(
                            $" <color=#ff7777>-{option.name}</color> <<color=cyan>{option.type}</color>>" +
                            new string(' ', padding) + option.info, "#aaaaaa");
                    }
                }

                await io.WriteLineAsync("");
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
