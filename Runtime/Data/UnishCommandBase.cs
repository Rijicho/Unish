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
            Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options);

        public UniTask Run(IUnishPresenter shell,
            Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            mShell = shell;
            foreach (var e in Env)
            {
                if (!args.ContainsKey(e.Key))
                {
                    args[e.Key] = e.Value;
                }
            }

            return Run(args, options);
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
