#if UNISH_INPUT_SYSTEM_SUPPORT
using UnityEngine.InputSystem;
#else
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace RUtil.Debug.Shell
{
    public class DefaultInputHandler : IUnishInputHandler
    {
        private Dictionary<UnishInputType, float> longPushStartTimes = new Dictionary<UnishInputType, float>();

        private Dictionary<UnishInputType, float>
            longPushLastInputTimes = new Dictionary<UnishInputType, float>();

        private Dictionary<UnishInputType, bool> longPushFlag = new Dictionary<UnishInputType, bool>();

        private UnishInputType[] inputs;

        private bool mIsRunning;

        private readonly IUnishTimeProvider mTimeProvider;
#if UNISH_INPUT_SYSTEM_SUPPORT
        public event Action<char> OnTextInput
        {
            add => Keyboard.current.onTextInput += value;
            remove => Keyboard.current.onTextInput -= value;
        }
#else
        public event Action<char> OnTextInput;
#endif
        private static char mCurrentCharInput;
        #if UNISH_INPUT_SYSTEM_SUPPORT
        private static bool hasBackSpaceInput;
#endif
        public         char CurrentCharInput => mCurrentCharInput;

        private void UpdateCurrentCharInput(char c)
        {
            switch (c)
            {
                case var bs when bs == 8:
#if UNISH_INPUT_SYSTEM_SUPPORT
                    hasBackSpaceInput = true;
#endif
                    return;
                default:
                    mCurrentCharInput = c;
                    return;
            }
        }

        public DefaultInputHandler(IUnishTimeProvider timeProvider)
        {
            mTimeProvider = timeProvider;
        }

        public UniTask InitializeAsync()
        {
            inputs                 =  Enum.GetValues(typeof(UnishInputType)).Cast<UnishInputType>().ToArray();
            longPushStartTimes     =  inputs.ToDictionary(x => x, x => 0f);
            longPushLastInputTimes =  inputs.ToDictionary(x => x, x => 0f);
            longPushFlag           =  inputs.ToDictionary(x => x, x => false);
            OnTextInput            += UpdateCurrentCharInput;
            mIsRunning             =  true;
            StartPreUpdate().Forget();
            StartPreLateUpdate().Forget();
            return default;
        }


        private async UniTaskVoid StartPreUpdate()
        {
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.PreUpdate))
            {
                if (!mIsRunning)
                {
                    return;
                }

                EarlyUpdate();
            }
        }

        private async UniTaskVoid StartPreLateUpdate()
        {
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.PreLateUpdate))
            {
                if (!mIsRunning)
                {
                    return;
                }

                LateUpdate();
            }
        }

        public UniTask FinalizeAsync()
        {
            OnTextInput -= UpdateCurrentCharInput;
            mIsRunning  =  false;
            return default;
        }

        public void EarlyUpdate()
        {
            var now = mTimeProvider.Now;


#if !UNISH_INPUT_SYSTEM_SUPPORT
            if (Input.anyKeyDown && !string.IsNullOrEmpty(Input.inputString))
            {
                OnTextInput(Input.inputString[0]);
            }
#endif

            foreach (var key in inputs)
            {
                longPushFlag[key] = false;
                if (longPushStartTimes[key] > 0)
                {
                    if (CheckInput(key))
                    {
                        if (longPushLastInputTimes[key] == 0 && now > longPushStartTimes[key] + 0.5f)
                        {
                            longPushLastInputTimes[key] = now;
                            longPushFlag[key]           = true;
                        }
                        else if (longPushLastInputTimes[key] > 0 &&
                                 now > longPushLastInputTimes[key] + 0.03f)
                        {
                            longPushLastInputTimes[key] = now;
                            longPushFlag[key]           = true;
                        }
                    }
                    else
                    {
                        longPushStartTimes[key]     = 0;
                        longPushFlag[key]           = false;
                        longPushLastInputTimes[key] = 0;
                    }
                }
                else
                {
                    if (CheckInputOnThisFramePure(key))
                    {
                        longPushStartTimes[key] = now;
                    }
                }
            }
        }

        public void LateUpdate()
        {
#if UNISH_INPUT_SYSTEM_SUPPORT
            hasBackSpaceInput = false;
#endif
            mCurrentCharInput = default;
        }


        public bool CheckInputOnThisFrame(UnishInputType input)
        {
            return CheckInputOnThisFramePure(input) || longPushFlag[input];
        }

        private static bool CheckInputOnThisFramePure(UnishInputType input)
        {
#if UNISH_INPUT_SYSTEM_SUPPORT
            var kb = Keyboard.current;
            var ctrlcmd = kb.ctrlKey.isPressed || kb.leftCommandKey.isPressed || kb.rightCommandKey.isPressed;
            return input switch
            {
                UnishInputType.Up => !ctrlcmd && kb.upArrowKey.wasPressedThisFrame,
                UnishInputType.Down => !ctrlcmd && kb.downArrowKey.wasPressedThisFrame,
                UnishInputType.Left => !ctrlcmd && kb.leftArrowKey.wasPressedThisFrame,
                UnishInputType.Right => !ctrlcmd && kb.rightArrowKey.wasPressedThisFrame,
                UnishInputType.ScrollUp => ctrlcmd && kb.upArrowKey.wasPressedThisFrame,
                UnishInputType.ScrollDown => ctrlcmd && kb.downArrowKey.wasPressedThisFrame,
                UnishInputType.PageUp => ctrlcmd && kb.leftArrowKey.wasPressedThisFrame,
                UnishInputType.PageDown => ctrlcmd && kb.rightArrowKey.wasPressedThisFrame,
                UnishInputType.BackSpace => !ctrlcmd && (kb.backspaceKey.wasPressedThisFrame || hasBackSpaceInput),
                UnishInputType.Delete => !ctrlcmd && kb.deleteKey.wasPressedThisFrame,
                UnishInputType.Submit => !ctrlcmd && kb.enterKey.wasPressedThisFrame,
                UnishInputType.Quit => (!ctrlcmd && kb.escapeKey.wasPressedThisFrame) ||
                                       (ctrlcmd && kb.enterKey.wasPressedThisFrame),
                _ => false,
            };

#else
            var ctrlcmd = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                          Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
            return input switch
            {
                UnishInputType.Up => !ctrlcmd && Input.GetKeyDown(KeyCode.UpArrow),
                UnishInputType.Down => !ctrlcmd && Input.GetKeyDown(KeyCode.DownArrow),
                UnishInputType.Left => !ctrlcmd && Input.GetKeyDown(KeyCode.LeftArrow),
                UnishInputType.Right => !ctrlcmd && Input.GetKeyDown(KeyCode.RightArrow),
                UnishInputType.ScrollUp => ctrlcmd && Input.GetKeyDown(KeyCode.UpArrow),
                UnishInputType.ScrollDown => ctrlcmd && Input.GetKeyDown(KeyCode.DownArrow),
                UnishInputType.PageUp => ctrlcmd && Input.GetKeyDown(KeyCode.LeftArrow),
                UnishInputType.PageDown => ctrlcmd && Input.GetKeyDown(KeyCode.RightArrow),
                UnishInputType.BackSpace => !ctrlcmd && Input.GetKeyDown(KeyCode.Backspace),
                UnishInputType.Delete => !ctrlcmd && Input.GetKeyDown(KeyCode.Delete),
                UnishInputType.Submit => !ctrlcmd && Input.GetKeyDown(KeyCode.Return),
                UnishInputType.Quit => (!ctrlcmd && Input.GetKeyDown(KeyCode.Escape)) ||
                                       (ctrlcmd && Input.GetKeyDown(KeyCode.D)),
                _ => false,
            };
#endif
        }


        private static bool CheckInput(UnishInputType input)
        {
#if UNISH_INPUT_SYSTEM_SUPPORT
            var kb = Keyboard.current;
            var ctrlcmd = kb.ctrlKey.isPressed || kb.leftCommandKey.isPressed || kb.rightCommandKey.isPressed;
            return input switch
            {
                UnishInputType.Up => kb.upArrowKey.isPressed,
                UnishInputType.Down => kb.downArrowKey.isPressed,
                UnishInputType.Left => kb.leftArrowKey.isPressed,
                UnishInputType.Right => kb.rightArrowKey.isPressed,
                UnishInputType.ScrollUp => ctrlcmd && kb.upArrowKey.isPressed,
                UnishInputType.ScrollDown => ctrlcmd && kb.downArrowKey.isPressed,
                UnishInputType.PageUp => ctrlcmd && kb.leftArrowKey.isPressed,
                UnishInputType.PageDown => ctrlcmd && kb.rightArrowKey.isPressed,
                UnishInputType.BackSpace => kb.backspaceKey.isPressed,
                UnishInputType.Delete => kb.deleteKey.isPressed,
                UnishInputType.Submit => kb.enterKey.isPressed,
                _ => false,
            };
#else
            var ctrlcmd = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                          Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
            return input switch
            {
                UnishInputType.Up => !ctrlcmd && Input.GetKey(KeyCode.UpArrow),
                UnishInputType.Down => !ctrlcmd && Input.GetKey(KeyCode.DownArrow),
                UnishInputType.Left => !ctrlcmd && Input.GetKey(KeyCode.LeftArrow),
                UnishInputType.Right => !ctrlcmd && Input.GetKey(KeyCode.RightArrow),
                UnishInputType.ScrollUp => ctrlcmd && Input.GetKey(KeyCode.UpArrow),
                UnishInputType.ScrollDown => ctrlcmd && Input.GetKey(KeyCode.DownArrow),
                UnishInputType.PageUp => ctrlcmd && Input.GetKey(KeyCode.LeftArrow),
                UnishInputType.PageDown => ctrlcmd && Input.GetKey(KeyCode.RightArrow),
                UnishInputType.BackSpace => !ctrlcmd && Input.GetKey(KeyCode.Backspace),
                UnishInputType.Delete => !ctrlcmd && Input.GetKey(KeyCode.Delete),
                UnishInputType.Submit => !ctrlcmd && Input.GetKey(KeyCode.Return),
                _ => false,
            };

#endif
        }
    }
}
