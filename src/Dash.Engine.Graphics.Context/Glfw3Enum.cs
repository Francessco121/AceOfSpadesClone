#region License
// Copyright (c) 2013 Antonie Blom
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace Dash.Engine
{
    public enum KeyModifiers
    {
        Shift = 1 << 0, //1
        Control = 1 << 1, //2
        Alt = 1 << 2, //4
        Super = 1 << 3  //8
    }

    public enum KeyAction
    {
        Release = 0,
        Press,
        Repeat,
    }

    public enum MouseButton
    {
        Button1 = 0,
        Button2,
        Button3,
        Button4,
        Button5,
        Button6,
        Button7,
        Button8,
        Left = 0,
        Right,
        Middle,
    }

    public enum Joystick
    {
        Joystick1 = 0,
        Joystick2,
        Joystick3,
        Joystick4,
        Joystick5,
        Joystick6,
        Joystick7,
        Joystick8,
        Joystick9,
        Joystick10,
        Joystick11,
        Joystick12,
        Joystick13,
        Joystick14,
        Joystick15,
        Joystick16,
    }

    public enum Key
    {
        Space = 32,
        Apostrophe = 39,
        Comma = 44,
        Minus,
        Period,
        Slash,
        Number0,
        Number1,
        Number2,
        Number3,
        Number4,
        Number5,
        Number6,
        Number7,
        Number8,
        Number9,
        Semicolon = 59,
        Equals = 61,
        A = 65,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        LeftBracket,
        Backslash,
        RightBracket,
        Tilde = 96,
        World1 = 161,
        World2,
        Escape = 256,
        Enter,
        Tab,
        Backspace,
        Insert,
        Delete,
        Right,
        Left,
        Down,
        Up,
        PageUp,
        PageDown,
        Home,
        End,
        CapsLock = 280,
        ScrollLock,
        NumLock,
        PrintScreen,
        Pause,
        F1 = 290,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        F13,
        F14,
        F15,
        F16,
        F17,
        F18,
        F19,
        F20,
        F21,
        F22,
        F23,
        F24,
        F25,
        Keypad0 = 320,
        Keypad1,
        Keypad2,
        Keypad3,
        Keypad4,
        Keypad5,
        Keypad6,
        Keypad7,
        Keypad8,
        Keypad9,
        KeypadDecimal,
        KeypadDivide,
        KeypadMultiply,
        KeypadSubtract,
        KeypadAdd,
        KeypadEnter,
        KeypadEquals,
        LeftShift = 340,
        LeftControl,
        LeftAlt,
        LeftSuper,
        RightShift,
        RightControl,
        RightAlt,
        RightSuper,
        Menu,
    }

    public enum CursorMode
    {
        CursorNormal = 0x00034001,
        CursorHidden,
        CursorCaptured,
    }

    public enum ConnectionState
    {
        Connected = 0x00040001,
        Disconnected,
    }
}

namespace Dash.Engine.Graphics.Context
{
    public enum InputMode
    {
        CursorMode = 0x00033001,
        StickyKeys,
        StickyMouseButtons,
    }

    public enum GlfwError {
		NoError = 0,
        NotInitialized = 0x10001,
		NoCurrentContext,
		InvalidEnum,
		InvalidValue,
		OutOfMemory,
		APIUnavailable,
		VersionUnavailable,
		PlatformError,
		FormatUnavailable,
	}

	public enum WindowAttrib {
		Focused = 0x20001,
		Iconified,
		Resizeable,
        Visible,
        Decorated
	}

	public enum WindowHint {
        Resizeable = 131075,
        Visible,
        Decorated,
        RedBits = 0x21001,
		GreenBits,
		BlueBits,
		AlphaBits,
		DepthBits,
		StencilBits,
		AccumRedBits,
		AccumGreenBits,
		AccumBlueBits,
		AccumAlphaBits,
		AuxBuffers,
		Stereo,
		Samples,
		SRGBCapable,
        RefreshRate,
		ClientAPI = 0x22001,
		ContextVersionMajor,
		ContextVersionMinor,
        ContextRevision,
		ContextRobustness,
		OpenGLForwardCompat,
		OpenGLDebugContext,
		OpenGLProfile,
	}

	public enum OpenGLAPI {
		OpenGLAPI = 0x30001,
		OpenGLESAPI,
	}

	public enum ContextRobustness {
		NoRobustness = 0,
        NoResetNotification = 0x031001,
		LoseContextOnReset,
	}

	public enum OpenGLProfile {
		Any = 0,
        Core = 0x032001,
		Compatibility,
	}
}