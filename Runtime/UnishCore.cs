using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public abstract class UnishCore : IUnish
    {
        // ----------------------------------
        // non-serialized fields
        // ----------------------------------

        private static bool mIsUprofileExecuted;
        private bool mIsInitialized;
        private bool mIsRunningUpdate;
        private bool mIsRunningCommand;
        private bool mIsClosing;

        // 入力履歴
        private readonly List<string> mSubmittedInputs = new List<string>();

        // 表示履歴
        private readonly List<string> mSubmittedLines = new List<string>();

        // 入力中文字列
        private string mInput = "";

        // このフレームに入力された文字
        private char mCharInput;

        // コマンド実行中の追加入力受付中かどうか
        private bool mIsWaitingNewSubmission;

        // コマンド実行中の追加入力で最後にSubmitされた文字列
        private string mAdditionalSubmittedInput;

        // 入力履歴参照先インデックス
        private int mReferenceIndex;

        // 入力履歴を参照中にもともとの履歴を保持しておくキャッシュ
        private string mReferenceCache = "";

        // 表示履歴のオフセット
        private int mDisplayLineOffset;

        // カーソル位置　終端にある状態が0, 始端にある状態がinput.Length
        private int mCursorIndex;

        // カーソルの点滅開始時刻
        private float mCursorBrinkStartTime;

        // ----------------------------------
        // properties
        // ----------------------------------
        public abstract IUnishView View { get; }
        public abstract IUnishCommandRepository CommandRepository { get; }
        public abstract IColorParser ColorParser { get; }
        public abstract IUnishInputHandler InputHandler { get; }
        public abstract ITimeProvider TimeProvider { get; }
        public abstract IUnishRcRepository RcRepository { get; }
        public abstract IEnumerable<IUnishDirectorySystem> DirectorySystems { get; }
        public IUnishDirectorySystem CurrentDirectorySystem { get; set; }
        public string Prompt { get; set; } = "> ";

        // ----------------------------------
        // public methods
        // ----------------------------------
        public async UniTask OpenAsync()
        {
            mIsRunningUpdate = false;
            mIsInitialized = false;
            mIsClosing = false;
            mIsRunningCommand = false;
            mSubmittedLines.Clear();
            mSubmittedInputs.Clear();

            await OnPreOpenAsync();
            await View.InitializeAsync();
            InputHandler.Initialize();
            CommandRepository.Initialize();
            InputHandler.OnTextInput += OnCharInput;
            await OnPostOpenAsync();
            mIsInitialized = true;
            try
            {
                if (!mIsUprofileExecuted)
                {
                    await foreach (var c in RcRepository.ReadUProfile()) await RunCommandAsync(c);

                    mIsUprofileExecuted = true;
                }

                await foreach (var c in RcRepository.ReadUnishRc()) await RunCommandAsync(c);
            }
            catch (Exception e)
            {
                this.SubmitError(e.Message);
                this.SubmitError(e.StackTrace);
            }

            StartUpdate().Forget();
        }

        public async UniTask CloseAsync()
        {
            mIsClosing = true;
            while (mIsRunningUpdate)
                await UniTask.Yield();
            await OnPreCloseAsync();
            InputHandler.OnTextInput -= OnCharInput;
            await View.DestroyAsync();
            await OnPostCloseAsync();
            mIsClosing = false;
        }

        public async UniTask RunCommandAsync(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return;

            cmd = cmd.TrimStart();

            // エイリアス解決
            foreach (var kv in CommandRepository.Aliases)
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

            // オペランドのみパースし、コマンドが存在すれば実行
            if (TryPreParseCommand(cmd, out var c, out var op, out var argsNotParsed))
            {
                try
                {
                    await c.Run(this, op, argsNotParsed, this.SubmitTextIndented, this.SubmitError);
                }
                catch (Exception e)
                {
                    this.SubmitError(e.Message ?? "");
                    this.SubmitTextIndented(e.StackTrace, "#ff7777");
                }
            }
            // 失敗時の追加評価処理が定義されていれば実行
            else if (!await TryRunInvalidCommand(cmd))
                this.SubmitError("Unknown Command/Expr. Enter 'h' to show help.");

            await UniTask.Yield();
        }

        public void WriteLine(string line)
        {
            mSubmittedLines.Add(line ?? "");
        }

        public async UniTask<string> ReadLineAsync()
        {
            mIsWaitingNewSubmission = true;
            mAdditionalSubmittedInput = null;
            while (string.IsNullOrEmpty(mAdditionalSubmittedInput))
                await UniTask.Yield();
            mIsWaitingNewSubmission = false;
            return mAdditionalSubmittedInput;
        }


        // ----------------------------------
        // protected methods
        // ----------------------------------

        protected virtual UniTask<bool> TryRunInvalidCommand(string cmd)
        {
            return UniTask.FromResult(false);
        }

        protected virtual UniTask OnPreOpenAsync()
        {
            return default;
        }

        protected virtual UniTask OnPostOpenAsync()
        {
            return default;
        }

        protected virtual UniTask OnPreCloseAsync()
        {
            return default;
        }

        protected virtual UniTask OnPostCloseAsync()
        {
            return default;
        }

        // ----------------------------------
        // private methods
        // ----------------------------------

        private async UniTaskVoid StartUpdate()
        {
            mIsRunningUpdate = true;
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate())
            {
                if (mIsClosing)
                    break;
                Update();
            }

            mIsRunningUpdate = false;
        }

        private void Update()
        {
            if (mIsClosing || !mIsInitialized)
                return;

            InputHandler.Update();

            var isInputEventUsed = HandleScrollInput();

            if (!isInputEventUsed && (!mIsRunningCommand || mIsWaitingNewSubmission))
            {
                if (InputHandler.CheckInputOnThisFrame(UnishInputType.Quit))
                {
                    CloseAsync().Forget();
                    return;
                }

                HandleSubmissionInput();
            }

            mCharInput = default;

            UpdateDisplay();
        }

        private void HandleSubmissionInput()
        {
            if (InputHandler.CheckInputOnThisFrame(UnishInputType.Up))
            {
                if (mReferenceIndex < mSubmittedInputs.Count)
                {
                    if (mReferenceIndex == 0)
                        mReferenceCache = mInput;
                    mReferenceIndex++;
                    mInput = mSubmittedInputs[mSubmittedInputs.Count - mReferenceIndex];
                }
            }
            else if (InputHandler.CheckInputOnThisFrame(UnishInputType.Down))
            {
                if (mReferenceIndex > 0)
                    mReferenceIndex--;
                mInput = mReferenceIndex > 0
                    ? mSubmittedInputs[mSubmittedInputs.Count - mReferenceIndex]
                    : mReferenceCache;
            }
            else if (InputHandler.CheckInputOnThisFrame(UnishInputType.Left))
            {
                if (mCursorIndex < mInput.Length)
                {
                    mCursorIndex++;
                    mCursorBrinkStartTime = TimeProvider.Now;
                }
            }
            else if (InputHandler.CheckInputOnThisFrame(UnishInputType.Right))
            {
                if (mCursorIndex > 0)
                {
                    mCursorIndex--;
                    mCursorBrinkStartTime = TimeProvider.Now;
                }
            }
            else if (InputHandler.CheckInputOnThisFrame(UnishInputType.BackSpace))
            {
                if (mInput.Length > 0)
                {
                    if (mCursorIndex == 0)
                        mInput = mInput.Substring(0, mInput.Length - 1);
                    else if (mCursorIndex < mInput.Length)
                    {
                        mInput = mInput.Substring(0, mInput.Length - mCursorIndex - 1) +
                                 mInput.Substring(mInput.Length - mCursorIndex);
                    }
                }
            }
            else if (InputHandler.CheckInputOnThisFrame(UnishInputType.Delete))
            {
                if (mInput.Length > 0 && mCursorIndex > 0)
                {
                    //3210   
                    //abc
                    if (mCursorIndex == 1)
                        mInput = mInput.Substring(0, mInput.Length - 1);
                    else if (mCursorIndex == mInput.Length)
                        mInput = mInput.Substring(1);
                    else
                    {
                        mInput = mInput.Substring(0, mInput.Length - mCursorIndex) +
                                 mInput.Substring(mInput.Length - mCursorIndex + 1);
                    }

                    mCursorIndex--;
                }
            }
            else if (InputHandler.CheckInputOnThisFrame(UnishInputType.Submit))
            {
                if (!mIsRunningCommand && !string.IsNullOrWhiteSpace(mInput))
                    mSubmittedInputs.Add(mInput);
                Submit().Forget();
            }
            else if (mCharInput != default) mInput = mInput.Insert(mInput.Length - mCursorIndex, mCharInput.ToString());
        }

        private bool HandleScrollInput()
        {
            if (InputHandler.CheckInputOnThisFrame(UnishInputType.ScrollUp))
            {
                if (mDisplayLineOffset < mSubmittedLines.Count - View.MaxLineCount)
                    mDisplayLineOffset++;
                return true;
            }

            if (InputHandler.CheckInputOnThisFrame(UnishInputType.ScrollDown))
            {
                if (mDisplayLineOffset > 0)
                    mDisplayLineOffset--;
                return true;
            }

            if (InputHandler.CheckInputOnThisFrame(UnishInputType.PageUp))
            {
                mDisplayLineOffset = Mathf.Min(mDisplayLineOffset + View.MaxLineCount,
                    mSubmittedLines.Count - View.MaxLineCount);
                return true;
            }

            if (InputHandler.CheckInputOnThisFrame(UnishInputType.PageDown))
            {
                mDisplayLineOffset = Mathf.Max(mDisplayLineOffset - View.MaxLineCount, 0);
                return true;
            }

            return false;
        }

        private void UpdateDisplay()
        {
            var now = TimeProvider.Now;
            var inputWithCursor = mCursorIndex == 0
                ? mInput + (Mathf.RoundToInt((now - mCursorBrinkStartTime) * 60) % 60 > 30
                    ? "<color=yellow>_</color>"
                    : " ")
                : mInput.Substring(0, mInput.Length - mCursorIndex)
                  + (Mathf.RoundToInt((now - mCursorBrinkStartTime) * 60) % 60 > 30
                      ? "<color=yellow>_</color>"
                      : $"<color=orange>{mInput.Substring(mInput.Length - mCursorIndex, 1)}</color>")
                  + mInput.Substring(mInput.Length - mCursorIndex + 1);

            var currentWithCursor = mIsWaitingNewSubmission
                ? $"<color=orange>|> {inputWithCursor}</color>"
                : ParsedPrompt + inputWithCursor;

            View.DisplayText = mSubmittedLines.Count > 0
                ? mSubmittedLines
                      .Skip(mSubmittedLines.Count - View.MaxLineCount - mDisplayLineOffset)
                      .Take(Mathf.Min(View.MaxLineCount, mSubmittedLines.Count))
                      .ToSingleString("\n", true) +
                  (!mIsRunningCommand || mIsWaitingNewSubmission ? currentWithCursor : "")
                : !mIsRunningCommand || mIsWaitingNewSubmission
                    ? currentWithCursor
                    : "";
        }

        private string ParsedPrompt => Prompt.Replace("%d", CurrentDirectorySystem == null ? "/"
            : string.IsNullOrEmpty(CurrentDirectorySystem.Current) ? "~"
            : Path.GetFileName(CurrentDirectorySystem.Current));

        private void OnCharInput(char c)
        {
            mCharInput = c;
        }


        private async UniTask Submit()
        {
            if (mIsWaitingNewSubmission)
            {
                mAdditionalSubmittedInput = mInput;
                this.SubmitText($"<color=orange>|> {mInput}</color>");
                mInput = "";
                mDisplayLineOffset = 0;
                mCursorIndex = 0;
                return;
            }

            if (mIsRunningCommand) return;
            mIsRunningCommand = true;

            var cmd = mInput;
            this.SubmitText(ParsedPrompt + mInput);
            mInput = "";
            mDisplayLineOffset = 0;
            mReferenceIndex = 0;
            mCursorIndex = 0;

            await UniTask.Yield();

            await RunCommandAsync(cmd);

            mIsRunningCommand = false;
        }


        private bool TryPreParseCommand(string cmd, out UnishCommandBase op, out string leading, out string trailing)
        {
            leading = cmd;
            trailing = "";
            for (var i = 0; i < cmd.Length; i++)
            {
                if (cmd[i] == ' ')
                {
                    leading = cmd.Substring(0, i);
                    trailing = cmd.Substring(i + 1);
                    break;
                }
            }

            return CommandRepository.Map.TryGetValue(leading, out op)
                   || CommandRepository.Map.TryGetValue("@" + leading, out op);
        }
    }
}