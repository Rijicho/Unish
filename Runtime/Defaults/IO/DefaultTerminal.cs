using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RUtil.Debug.Shell
{
    public class DefaultTerminal : IUnishTerminal
    {
        private const string PrefabResourcePath = "Prefabs/Unish";

        public IUnishEnv BuiltInEnv { protected get; set; }

        private Image background;
        private Text  displayText;

        private readonly Font               mFont;
        private readonly IUnishInputHandler mInputHandler;
        private readonly IUnishTimeProvider mTimeProvider;
        private readonly IUnishColorParser  mColorParser;

        private int        mCharCountPerLine;
        private int        mLineCount;
        private bool       mIsReading;
        private GameObject mInstantiated;

        // 入力履歴
        private readonly List<string> mSubmittedInputs = new List<string>();

        // 表示履歴
        private readonly List<string> mSubmittedLines = new List<string>();

        // 入力中文字列
        private string mInput = "";

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

        public DefaultTerminal(
            Font font = default,
            IUnishInputHandler inputHandler = default,
            IUnishTimeProvider timeProvider = default,
            IUnishColorParser colorParser = default)
        {
            mFont         = font;
            mInputHandler = inputHandler ?? new DefaultInputHandler(DefaultTimeProvider.Instance);
            mTimeProvider = timeProvider ?? DefaultTimeProvider.Instance;
            mColorParser  = colorParser ?? DefaultColorParser.Instance;
        }

        public async UniTask InitializeAsync()
        {
            mSubmittedLines.Clear();
            mSubmittedInputs.Clear();

            var prefab = await Resources.LoadAsync(PrefabResourcePath) as GameObject;
            mInstantiated      = Object.Instantiate(prefab);
            mInstantiated.name = "Unish";
            Object.DontDestroyOnLoad(mInstantiated);
            var component = mInstantiated.GetComponent<DefaultDisplay>();
            background  = component.Background;
            displayText = component.Text;
            if (mFont)
            {
                displayText.font = mFont;
            }

            background.color     = BuiltInEnv.Get(UnishBuiltInEnvKeys.BgColor, mColorParser.Parse("#000000cc"));
            mCharCountPerLine    = BuiltInEnv.Get(UnishBuiltInEnvKeys.CharCountPerLine, 100);
            mLineCount           = BuiltInEnv.Get(UnishBuiltInEnvKeys.LineCount, 24);
            displayText.fontSize = BuiltInEnv.Get(UnishBuiltInEnvKeys.FontSize, 24);


            // 画面サイズ設定
            RefleshSize();

            await mInputHandler.InitializeAsync();
            BuiltInEnv.OnSet     += OnEnvSet;
            BuiltInEnv.OnRemoved += OnEnvRemoved;

            StartUpdate().Forget();
        }

        public async UniTask FinalizeAsync()
        {
            BuiltInEnv.OnRemoved -= OnEnvRemoved;
            BuiltInEnv.OnSet     -= OnEnvSet;

            await mInputHandler.FinalizeAsync();
            displayText = null;
            background  = null;
            Object.Destroy(mInstantiated);
        }

        public event Action OnHaltInput;


        public UniTask WriteAsync(string input)
        {
            if (mSubmittedLines.Count == 0)
            {
                mSubmittedLines.Add("");
            }

            var j = 0;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '\n')
                {
                    mSubmittedLines[mSubmittedLines.Count - 1] += input.Substring(j, i - j);
                    mSubmittedLines.Add("");
                    j = i + 1;
                }
            }

            if (j < input.Length)
            {
                mSubmittedLines[mSubmittedLines.Count - 1] += input.Substring(j);
            }

            return default;
        }

        public async UniTask WriteErrorAsync(Exception error)
        {
            await WriteAsync($"error: {error.Message}\n");
            UnityEngine.Debug.LogError(error);
        }


        public IUniTaskAsyncEnumerable<string> ReadLinesAsync(bool withPrompt = false)
        {
            return UniTaskAsyncEnumerable.Create<string>(async (writer, token) =>
            {
                while (!token.IsCancellationRequested)
                {
                    var input = await ReadLineAsync(withPrompt);
                    if (input == null)
                    {
                        break;
                    }

                    var eotIndex = input.IndexOf('\u0004');
                    if (eotIndex < 0)
                    {
                        await writer.YieldAsync(input);
                    }
                    else if (eotIndex == 0)
                    {
                        break;
                    }
                    else
                    {
                        await writer.YieldAsync(input.Substring(0, eotIndex));
                        break;
                    }
                }
            });
        }
        private async UniTask<string> ReadLineAsync(bool withPrompt)
        {
            mIsReading = true;
            if (withPrompt)
            {
                await WriteAsync(ParsedPrompt);
            }

            string ret = null;
            while (ret == null)
            {
                ret = await HandleSubmissionInput();
                await UniTask.Yield();
            }

            mIsReading = false;
            return ret;
        }

        private void OnEnvSet(UnishVariable envvar)
        {
            switch (envvar.Name)
            {
                case UnishBuiltInEnvKeys.BgColor:
                    background.color = envvar.CastOr(mColorParser.Parse("#000000cc"));
                    break;
                case UnishBuiltInEnvKeys.CharCountPerLine:
                    mCharCountPerLine = Mathf.Max(20, envvar.CastOr(100));
                    RefleshSize();
                    break;
                case UnishBuiltInEnvKeys.LineCount:
                    mLineCount = Mathf.Max(1, envvar.CastOr(24));
                    RefleshSize();
                    break;
                case UnishBuiltInEnvKeys.FontSize:
                    displayText.fontSize = envvar.CastOr(24);
                    RefleshSize();
                    break;
            }
        }

        private void OnEnvRemoved(string key)
        {
            switch (key)
            {
                case UnishBuiltInEnvKeys.BgColor:
                    background.color = mColorParser.Parse("#000000cc");
                    break;
                case UnishBuiltInEnvKeys.CharCountPerLine:
                    mCharCountPerLine = 100;
                    RefleshSize();
                    break;
                case UnishBuiltInEnvKeys.LineCount:
                    mLineCount = 24;
                    RefleshSize();
                    break;
            }
        }

        private void RefleshSize()
        {
            var prevText    = displayText.text;
            var placeHolder = new StringBuilder();
            for (var i = 0; i < mLineCount - 1; i++)
            {
                placeHolder.AppendLine(new string('0', mCharCountPerLine));
            }


            displayText.text                    = placeHolder.ToString();
            displayText.rectTransform.sizeDelta = new Vector2(displayText.preferredWidth, displayText.preferredHeight);
            background.rectTransform.sizeDelta  = new Vector2(displayText.preferredWidth + 20, displayText.preferredHeight + 20);
            displayText.text                    = prevText;
        }

        private async UniTaskVoid StartUpdate()
        {
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate())
            {
                if (!OnUpdate())
                {
                    break;
                }
            }
        }

        private bool isInputEventUsed;

        private bool OnUpdate()
        {
            isInputEventUsed = HandleScrollInput();

            UpdateDisplay();
            return true;
        }

        private async UniTask<string> HandleSubmissionInput()
        {
            if (isInputEventUsed)
            {
                return null;
            }


            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Up))
            {
                if (mReferenceIndex < mSubmittedInputs.Count)
                {
                    if (mReferenceIndex == 0)
                    {
                        mReferenceCache = mInput;
                    }

                    mReferenceIndex++;
                    mInput = mSubmittedInputs[mSubmittedInputs.Count - mReferenceIndex];
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Down))
            {
                if (mReferenceIndex > 0)
                {
                    mReferenceIndex--;
                }

                mInput = mReferenceIndex > 0
                    ? mSubmittedInputs[mSubmittedInputs.Count - mReferenceIndex]
                    : mReferenceCache;
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Left))
            {
                if (mCursorIndex < mInput.Length)
                {
                    mCursorIndex++;
                    mCursorBrinkStartTime = mTimeProvider.Now;
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Right))
            {
                if (mCursorIndex > 0)
                {
                    mCursorIndex--;
                    mCursorBrinkStartTime = mTimeProvider.Now;
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.BackSpace))
            {
                if (mInput.Length > 0)
                {
                    if (mCursorIndex == 0)
                    {
                        mInput = mInput.Substring(0, mInput.Length - 1);
                    }
                    else if (mCursorIndex < mInput.Length)
                    {
                        mInput = mInput.Substring(0, mInput.Length - mCursorIndex - 1) +
                                 mInput.Substring(mInput.Length - mCursorIndex);
                    }
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Delete))
            {
                if (mInput.Length > 0 && mCursorIndex > 0)
                {
                    //3210   
                    //abc
                    if (mCursorIndex == 1)
                    {
                        mInput = mInput.Substring(0, mInput.Length - 1);
                    }
                    else if (mCursorIndex == mInput.Length)
                    {
                        mInput = mInput.Substring(1);
                    }
                    else
                    {
                        mInput = mInput.Substring(0, mInput.Length - mCursorIndex) +
                                 mInput.Substring(mInput.Length - mCursorIndex + 1);
                    }

                    mCursorIndex--;
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Submit))
            {
                if (!string.IsNullOrWhiteSpace(mInput))
                {
                    mSubmittedInputs.Add(mInput);
                }

                var ret = mInput;
                await WriteAsync(mInput + "\n");
                mInput             = "";
                mDisplayLineOffset = 0;
                mReferenceIndex    = 0;
                mCursorIndex       = 0;
                return ret;
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Quit))
            {
                return "\u0004";
            }

            if (mInputHandler.CurrentCharInput != default)
            {
                mInput = mInput.Insert(mInput.Length - mCursorIndex, mInputHandler.CurrentCharInput.ToString());
            }


            return null;
        }


        private bool HandleScrollInput()
        {
            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.ScrollUp))
            {
                if (mDisplayLineOffset < mSubmittedLines.Count - mLineCount)
                {
                    mDisplayLineOffset++;
                }

                return true;
            }

            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.ScrollDown))
            {
                if (mDisplayLineOffset > 0)
                {
                    mDisplayLineOffset--;
                }

                return true;
            }

            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.PageUp))
            {
                mDisplayLineOffset = Mathf.Min(mDisplayLineOffset + mLineCount,
                    mSubmittedLines.Count - mLineCount);
                return true;
            }

            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.PageDown))
            {
                mDisplayLineOffset = Mathf.Max(mDisplayLineOffset - mLineCount, 0);
                return true;
            }

            return false;
        }


        private void UpdateDisplay()
        {
            static string TagEscape(string str)
            {
                return str.Replace("<", "<\b").Replace(">", "\b>");
            }

            var now = mTimeProvider.Now;
            var inputWithCursor = mCursorIndex == 0
                ? TagEscape(mInput) + (Mathf.RoundToInt((now - mCursorBrinkStartTime) * 60) % 60 > 30
                    ? "<color=yellow>_</color>"
                    : " ")
                : TagEscape(mInput.Substring(0, mInput.Length - mCursorIndex))
                  + (Mathf.RoundToInt((now - mCursorBrinkStartTime) * 60) % 60 > 30
                      ? "<color=yellow>_</color>"
                      : $"<color=orange>{TagEscape(mInput.Substring(mInput.Length - mCursorIndex, 1))}</color>")
                  + TagEscape(mInput.Substring(mInput.Length - mCursorIndex + 1));

            if (!displayText)
            {
                return;
            }

            if (mSubmittedLines.Count == 0)
            {
                displayText.text = mIsReading ? inputWithCursor : "";
                return;
            }

            if (mDisplayLineOffset == 0)
            {
                displayText.text = mSubmittedLines
                                       .Skip(mSubmittedLines.Count - mLineCount)
                                       .ToSingleString("\n") +
                                   (mIsReading ? inputWithCursor : "");
                return;
            }

            displayText.text = mSubmittedLines
                                   .Skip(mSubmittedLines.Count - mLineCount - mDisplayLineOffset)
                                   .Take(Mathf.Min(mLineCount, mSubmittedLines.Count) - 1)
                                   .ToSingleString("\n") + "\n" + mSubmittedLines.Last() +
                               (mIsReading ? inputWithCursor : "");
        }


        private string ParsedPrompt
        {
            get
            {
                var prompt = BuiltInEnv[UnishBuiltInEnvKeys.Prompt].S;
                var pwd    = BuiltInEnv[UnishBuiltInEnvKeys.WorkingDirectory].S;
                if (!prompt.Contains("%d"))
                {
                    return prompt;
                }

                if (pwd == UnishPathConstants.Root)
                {
                    return prompt.Replace("%d", UnishPathConstants.Root);
                }

                if (pwd == BuiltInEnv[UnishBuiltInEnvKeys.HomePath].S)
                {
                    return prompt.Replace("%d", UnishPathConstants.Home);
                }

                return prompt.Replace("%d", pwd.Substring(pwd.LastIndexOf(UnishPathConstants.Separator) + 1));
            }
        }
    }
}
