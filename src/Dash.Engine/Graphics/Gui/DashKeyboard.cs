using System;
using System.Collections.Generic;
using System.Text;

/* DashKeyboard.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public static class DashKeyboard
    {
        static KeyboardState lastState;
        static Key currentKey;

        static readonly float repeatInitDelay = 0.3f;
        static readonly float repeatDelay = 0.05f;
        static float repeatTime = repeatInitDelay;

        #region Key Map
        struct CharPair
        {
            public char Lower;
            public char? Upper;

            public CharPair(char lower, char? upper)
            {
                Lower = lower;
                Upper = upper;
            }
        }

        private static Dictionary<Key, CharPair> keyMap = new Dictionary<Key, CharPair>()
        {
            // Top Row
            {Key.Tilde, new CharPair('`', '~')},
            {Key.Number0, new CharPair('0', ')')},
            {Key.Number1, new CharPair('1', '!')},
            {Key.Number2, new CharPair('2', '@')},
            {Key.Number3, new CharPair('3', '#')},
            {Key.Number4, new CharPair('4', '$')},
            {Key.Number5, new CharPair('5', '%')},
            {Key.Number6, new CharPair('6', '^')},
            {Key.Number7, new CharPair('7', '&')},
            {Key.Number8, new CharPair('8', '*')},
            {Key.Number9, new CharPair('9', '(')},
            {Key.Minus, new CharPair('-', '_')},
            {Key.Equals, new CharPair('=', '+')},

            // Second Row
            {Key.LeftBracket, new CharPair('[', '{')},
            {Key.RightBracket, new CharPair(']', '}')},
            {Key.Backslash, new CharPair('\\', '|')},

            // Third Row
            {Key.Semicolon, new CharPair(';', ':')},
            {Key.Apostrophe, new CharPair('\'', '"')},
            {Key.Comma, new CharPair(',', '<')},
            {Key.Period, new CharPair('.', '>')},
            {Key.Slash, new CharPair('/', '?')},

            // Keypad
            {Key.Keypad0, new CharPair('0', null)},
            {Key.Keypad1, new CharPair('1', null)},
            {Key.Keypad2, new CharPair('2', null)},
            {Key.Keypad3, new CharPair('3', null)},
            {Key.Keypad4, new CharPair('4', null)},
            {Key.Keypad5, new CharPair('5', null)},
            {Key.Keypad6, new CharPair('6', null)},
            {Key.Keypad7, new CharPair('7', null)},
            {Key.Keypad8, new CharPair('8', null)},
            {Key.Keypad9, new CharPair('9', null)},
            {Key.KeypadAdd, new CharPair('+', null)},
            {Key.KeypadDivide, new CharPair('/', null)},
            {Key.KeypadMultiply, new CharPair('*', null)},
            {Key.KeypadSubtract, new CharPair('-', null)},
            {Key.KeypadDecimal, new CharPair('.', null)},

        };
        #endregion

        public static bool ParseKey(Key key, bool shift, out char keyAsChar)
        {
            if ((Key.A <= key && key <= Key.Z) || key == Key.Space)
            {
                if (key == Key.Space)
                {
                    keyAsChar = ' ';
                    return true;
                }
                else
                {
                    // Key is a letter or space
                    keyAsChar = shift ? Enum.GetName(typeof(Key), key)[0] : char.ToLower(Enum.GetName(typeof(Key), key)[0]);
                    return true;
                }
            }
            else
            {
                // Try keymap since its a symbol
                CharPair cpair;
                if (keyMap.TryGetValue(key, out cpair))
                {
                    if (shift && cpair.Upper.HasValue)
                    {
                        keyAsChar = cpair.Upper.Value;
                        return true;
                    }
                    else if (!shift)
                    {
                        keyAsChar = cpair.Lower;
                        return true;
                    }
                }
            }

            // Failed to convert
            keyAsChar = ' ';
            return false;
        }

        public static void ProcessKeyInput(KeyboardState currentState, float deltaTime, 
            out int spacesMoved, out Key[] ControlKeys, 
            int maxLength = int.MaxValue)
        {
            string nullStr = "";
            ProcessTextKeyInput(currentState, deltaTime, 0, ref nullStr, out spacesMoved, out ControlKeys, new Key[0], maxLength);
        }

        static StringBuilder sb = new StringBuilder();
        public static void ProcessTextKeyInput(KeyboardState currentState, float deltaTime, int i, ref string textString,
            out int spacesMoved, out Key[] controlKeys, Key[] excludeTextKeys, int maxLength = int.MaxValue)
        {
            spacesMoved = 0;

            Key[] keys = Input.GetPressedKeys();
            List<Key> pressedControlKeys = new List<Key>();
            sb.Clear();
            sb.Append(textString);

            bool shiftHeld = currentState.IsKeyDown(Key.LeftShift) || currentState.IsKeyDown(Key.RightShift);
            bool controlHeld = currentState.IsKeyDown(Key.LeftControl) || currentState.IsKeyDown(Key.RightControl);
            bool vPressed = false;

            foreach (Key key in keys)
            {
                if ((int)key == -1)
                    continue;

                // Key is in the middle of repeating
                if (!IsKeyPressedRepeat(key, deltaTime))
                    continue;

                char keyAsChar;
                if (ParseKey(key, shiftHeld, out keyAsChar))
                {
                    bool exlude = false;
                    if (!shiftHeld)
                    {
                        if (excludeTextKeys != null)
                        {
                            foreach (Key ekey in excludeTextKeys)
                                if (ekey == key)
                                {
                                    exlude = true;
                                    break;
                                }
                        }
                    }

                    if (!exlude && (keyAsChar == 'v' || keyAsChar == 'V'))
                        vPressed = true;

                    if (!exlude && !controlHeld && sb.Length < maxLength)
                    {
                        if (sb.Length == 0 || i == sb.Length)
                        {
                            sb.Append(keyAsChar);
                            i++;
                        }
                        else
                            sb.Insert(i++, keyAsChar);

                        spacesMoved++;
                    }
                }
                else
                    pressedControlKeys.Add(key);
            }

            if (controlHeld && vPressed)
            {
                string clipboard = GameWindow.Instance.GetClipboard();
                int charsToAppend = Math.Min(clipboard.Length, maxLength - sb.Length);
                sb.Append(clipboard, 0, charsToAppend);
                spacesMoved += charsToAppend;
            }

            controlKeys = pressedControlKeys.ToArray();
            textString = sb.ToString();

            lastState = currentState;
        }

        static bool IsKeyPressedRepeat(Key key, float deltaTime)
        {
            // First press
            if (lastState != null && lastState.IsKeyUp(key))
            {
                repeatTime = repeatInitDelay;
                currentKey = key;
                return true;
            }

            // Repeating
            if (key == currentKey)
            {
                repeatTime -= deltaTime;
                if (repeatTime <= 0)
                {
                    // Only reason adding is done here is to compenstate for lag in 
                    // finding that the timer is at zero (lag spikes can cause it to wait longer initially)
                    repeatTime += repeatDelay;
                    return true;
                }
            }

            // Either not pressed, or in the middle of a repeat delay.
            return false;
        }
    }
}
