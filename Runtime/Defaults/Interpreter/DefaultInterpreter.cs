using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using VDictionary = System.Collections.Generic.Dictionary<string, RUtil.Debug.Shell.UnishVariable>;

namespace RUtil.Debug.Shell
{
    public class DefaultInterpreter : IUnishInterpreter
    {
        public  IUnishEnv                                     BuiltInEnv { protected get; set; }
        public  IDictionary<string, string>                   Aliases    { get;           private set; }
        public  IReadOnlyDictionary<string, UnishCommandBase> Commands   => mRepository.Map;
        private IUnishCommandRepository                       mRepository;

        // ----------------------------------
        // public methods
        // ----------------------------------
        public async UniTask InitializeAsync()
        {
            mRepository = DefaultCommandRepository.Instance;
            Aliases     = new Dictionary<string, string>();
            await mRepository.InitializeAsync();
        }

        public async UniTask FinalizeAsync()
        {
            await mRepository.FinalizeAsync();
            Aliases     = null;
            mRepository = null;
        }

        public async UniTask RunCommandAsync(IUnishProcess shell, string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return;
            }

            cmd = cmd.TrimStart();


            // エイリアス解決
            foreach (var kv in Aliases)
            {
                if (cmd.TrimEnd() == kv.Key)
                {
                    cmd = kv.Value;
                    break;
                }

                if (cmd.StartsWith(kv.Key + " "))
                {
                    cmd = kv.Value + cmd.Substring(kv.Key.Length);
                    break;
                }
            }

            var noVariableInput = UnishCommandUtils.ParseVariables(cmd, shell.Env);
            var tokens          = UnishCommandUtils.CommandToTokens(noVariableInput);


            //var tokens          = UnishCommandUtils.SplitCommand(noVariableInput);
            var cmdToken = tokens[0].Token;
            var arguments = tokens
                .Skip(1).ToArray();

            // シェル変数への代入命令は特別扱い
            var eqIdx = cmdToken.IndexOf('=');
            if (eqIdx > 0 && eqIdx < cmdToken.Length - 1)
            {
                var left  = cmdToken.Substring(0, eqIdx);
                var right = cmdToken.Substring(eqIdx + 1);
                right = UnishCommandUtils.RemoveQuotesIfExist(right);
                if (shell.Env.BuiltIn.ContainsKey(left))
                {
                    shell.Env.BuiltIn.Set(left, right);
                }
                else
                {
                    shell.Env.Shell.Set(left, right);
                }

                return;
            }

            // 対応するコマンドが存在すれば実行
            if (mRepository.Map.TryGetValue(cmdToken, out var cmdInstance))
            {
                try
                {
                    var parsed = await ParseArguments(shell.IO, cmdInstance, cmdToken, arguments);
                    if (parsed.IsSucceeded)
                    {
                        // リダイレクトに応じてIO差し替え
                        var io          = ConstructIO(parsed, shell.IO, shell.Directory);
                        var runnerShell = cmdInstance.IsBuiltIn ? shell : shell.Fork(io);
                        await cmdInstance.Run(runnerShell, parsed.Params, parsed.Options);
                    }
                }
                catch (Exception e)
                {
                    await shell.IO.Err(e);
                }

                return;
            }

            // コマンドが見つからなかった場合の追加評価処理が定義されていれば実行
            if (!await TryRunUnknownCommand(cmd))
            {
                await shell.IO.Err(new Exception("Unknown Command. Enter 'h' to show help."));
            }

            await UniTask.Yield();
        }


        // ----------------------------------
        // protected methods
        // ----------------------------------
        protected virtual UniTask<bool> TryRunUnknownCommand(string cmd)
        {
            return UniTask.FromResult(false);
        }

        // ----------------------------------
        // private methods
        // ----------------------------------

        public class UnishCommandParseResult
        {
            public bool        IsSucceeded;
            public VDictionary Params;
            public VDictionary Options;
            public string      RedirectIn;
            public string      RedirectOut;
            public string      RedirectErr;
            public bool        IsRedirectOutAppend;
            public bool        IsRedirectErrAppend;
        }

        private static async UniTask<UnishCommandParseResult> ParseArguments(
            UnishIOs io,
            UnishCommandBase targetCommand,
            string op,
            IEnumerable<(string Token, UnishCommandTokenType TokenType)> arguments)
        {
            var ret      = new UnishCommandParseResult();
            var dParams  = new VDictionary();
            var dOptions = new VDictionary();
            dParams["0"] = new UnishVariable("0", op);

            var parsingOptionName    = "";
            var parsingOptionType    = UnishVariableType.Unit;
            var parsingOptionDefault = "";

            var currentParamIndex = 0;

            foreach (var (token, tokenType) in arguments)
            {
                // 前のトークンがオプションで、引数を必要としていて、現在のトークンがパラメータ以外なら既定値を入れて生成
                if (parsingOptionType != UnishVariableType.Unit && tokenType != UnishCommandTokenType.Param)
                {
                    dOptions[parsingOptionName] = new UnishVariable(parsingOptionName, parsingOptionType, parsingOptionDefault);
                    parsingOptionType           = UnishVariableType.Unit;
                    parsingOptionName           = "";
                    parsingOptionDefault        = "";
                }

                // 現在のトークンがオプションなら
                if (tokenType == UnishCommandTokenType.Option)
                {
                    // 現在のトークンに相当するオプションを検索
                    foreach (var expected in targetCommand.Options)
                    {
                        if (token != expected.name)
                        {
                            continue;
                        }

                        // 引数を必要としないならこの場で生成
                        if (expected.type == UnishVariableType.Unit)
                        {
                            dOptions[token]      = UnishVariable.Unit(token);
                            parsingOptionType    = UnishVariableType.Unit;
                            parsingOptionName    = "";
                            parsingOptionDefault = "";
                        }
                        // 引数を必要とするなら、次のトークンを引数として判定するための情報を確保
                        else
                        {
                            parsingOptionType    = expected.type;
                            parsingOptionName    = expected.name;
                            parsingOptionDefault = expected.defVal;
                        }

                        break;
                    }

                    continue;
                }

                // 前のトークンがオプションかつ要引数で、現在のトークンがパラメータの場合
                if (parsingOptionType != UnishVariableType.Unit && tokenType == UnishCommandTokenType.Param)
                {
                    // 現在のトークンをオプションの引数として格納
                    var arg = new UnishVariable(parsingOptionName, parsingOptionType, token);
                    dOptions[parsingOptionName] = arg;
                    // 型エラーチェック
                    if (arg.Type == UnishVariableType.Error)
                    {
                        await io.Err(new Exception($"Type mismatch: {token} is not {parsingOptionType}."));
                        await targetCommand.WriteUsage(io, op);
                        return default;
                    }

                    parsingOptionType    = UnishVariableType.Unit;
                    parsingOptionName    = "";
                    parsingOptionDefault = "";
                    continue;
                }

                // 現在のトークンがパラメータであり、期待されるパラメータの個数に収まっている場合
                if (tokenType == UnishCommandTokenType.Param && currentParamIndex < targetCommand.Params.Length)
                {
                    var expectedParam = targetCommand.Params[currentParamIndex];
                    // 現在のトークンをコマンドの引数として格納
                    var arg = new UnishVariable(expectedParam.name, expectedParam.type, token);
                    // $1, $2,...にも格納
                    dParams[$"{currentParamIndex + 1}"] = dParams[expectedParam.name] = arg;
                    // 型エラーチェック
                    if (arg.Type == UnishVariableType.Error)
                    {
                        await io.Err(new Exception($"Type mismatch: {token} is not {expectedParam.type}."));
                        await targetCommand.WriteUsage(io, op);
                        return default;
                    }

                    // 次に期待されるコマンドにindexを進める
                    currentParamIndex++;
                    continue;
                }

                // 現在のトークンはパラメータだが、期待されるパラメータ数を超過している場合
                if (tokenType == UnishCommandTokenType.Param)
                {
                    // string入力として $1, $2,...にのみ格納
                    var name = $"{currentParamIndex + 1}";
                    dParams[name] = new UnishVariable(name, token);
                    currentParamIndex++;
                    continue;
                }

                // 他のトークンタイプの場合
                switch (tokenType)
                {
                    case UnishCommandTokenType.RedirectIn:
                        ret.RedirectIn = token;
                        break;
                    case UnishCommandTokenType.RedirectOut:
                        ret.RedirectOut         = token;
                        ret.IsRedirectOutAppend = false;
                        break;
                    case UnishCommandTokenType.RedirectErr:
                        ret.RedirectErr         = token;
                        ret.IsRedirectErrAppend = false;
                        break;
                    case UnishCommandTokenType.RedirectOutAppend:
                        ret.RedirectOut         = token;
                        ret.IsRedirectOutAppend = true;
                        break;
                    case UnishCommandTokenType.RedirectErrAppend:
                        ret.RedirectErr         = token;
                        ret.IsRedirectErrAppend = true;
                        break;
                }
            }

            // 最後のオプションが引数を必要とするものだった場合、既定値を格納
            if (parsingOptionType != UnishVariableType.Unit)
            {
                dOptions[parsingOptionName] = new UnishVariable(parsingOptionName, parsingOptionType, parsingOptionDefault);
            }

            // 入力だけでは期待されるパラメータリストを全て満たせない場合、それぞれ既定値を格納
            while (currentParamIndex < targetCommand.Params.Length)
            {
                var expectedParam = targetCommand.Params[currentParamIndex];
                dParams[$"{currentParamIndex + 1}"] = dParams[expectedParam.name]
                    = new UnishVariable(expectedParam.name, expectedParam.type, expectedParam.defVal);

                currentParamIndex++;
            }

            // パラメータの個数を格納
            dParams["#"] = new UnishVariable("#", currentParamIndex);

            var assembledParams = "";
            var listedParams    = new string[currentParamIndex];
            for (var i = 1; i <= currentParamIndex; i++)
            {
                var p = dParams[$"{i}"].S;
                assembledParams += p;
                if (i < currentParamIndex)
                {
                    assembledParams += " ";
                }

                listedParams[i - 1] = p;
            }

            var assembledOptions = "";
            foreach (var option in dOptions.Keys)
            {
                assembledOptions += option;
            }

            dParams["*"] = new UnishVariable("*", assembledParams);
            dParams["@"] = new UnishVariable("@", listedParams);
            dParams["-"] = new UnishVariable("-", assembledOptions);

            ret.Params      = dParams;
            ret.Options     = dOptions;
            ret.IsSucceeded = true;
            return ret;
        }

        private static UnishIOs ConstructIO(UnishCommandParseResult parsed, UnishIOs stdio, IUnishFileSystemRoot fileSystem)
        {
            return new UnishIOs(
                string.IsNullOrEmpty(parsed.RedirectIn)
                    ? stdio.In
                    : _ => UniTask.FromResult(fileSystem.Read(parsed.RedirectIn)),
                string.IsNullOrEmpty(parsed.RedirectOut)
                    ? stdio.Out
                    : text =>
                    {
                        if (parsed.IsRedirectOutAppend)
                        {
                            fileSystem.Append(parsed.RedirectOut, text);
                        }
                        else
                        {
                            fileSystem.Write(parsed.RedirectOut, text);
                        }

                        return default;
                    },
                string.IsNullOrEmpty(parsed.RedirectErr)
                    ? stdio.Err
                    : err =>
                    {
                        var message = err.Message + "\n" + err.StackTrace + "\n";
                        if (parsed.IsRedirectErrAppend)
                        {
                            fileSystem.Append(parsed.RedirectErr, message);
                        }
                        else
                        {
                            fileSystem.Write(parsed.RedirectErr, message);
                        }

                        return default;
                    }
            );
        }
    }
}
