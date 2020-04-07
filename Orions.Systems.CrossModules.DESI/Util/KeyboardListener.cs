using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Orions.Systems.CrossModules.Desi.Util
{
	public static class KeyboardListener
	{
		private static readonly Subject<KeyboardEventData> _keyEventSobject = new Subject<KeyboardEventData>();

		[EditorBrowsable(EditorBrowsableState.Never)]
		[JSInvokable]
		public static Task OnKeyEvent(Key key, KeyModifiers modifiers)
		{
			Debug.WriteLine($"{modifiers} {key}");
			_keyEventSobject.OnNext(new KeyboardEventData(key, modifiers));
			return Task.FromResult(true);
		}

		private static IObservable<KeyboardEventData> KeyboardObservable => _keyEventSobject.AsObservable();

		public static KeyboardEventSubscription CreateSubscription() => new KeyboardEventSubscription(KeyboardObservable);

		public class KeyboardEventSubscription : IDisposable
		{
			private readonly List<(KeyboardEventData EventData, Action Handler)> _shortcuts = new List<(KeyboardEventData, Action)>();

			private IDisposable _sub;

			internal KeyboardEventSubscription(IObservable<KeyboardEventData> keyboardEventObservable)
			{
				_sub = keyboardEventObservable.Subscribe(OnKeyboardEvent);
			}

			public void Dispose()
			{
				_sub.Dispose();
				_shortcuts.Clear();
			}

			private void OnKeyboardEvent(KeyboardEventData keyboardEventData) => _shortcuts
				.Where(i => i.EventData.Equals(keyboardEventData))
				.ForEach(i => { 
					i.Handler(); 
				});

			public KeyboardEventSubscription AddShortcut(Key key, Action handler) => AddShortcut(key, KeyModifiers.None, handler);

			public KeyboardEventSubscription AddShortcut(Key key, KeyModifiers modifiers, Action handler)
			{
				if (handler == null)
				{
					throw new ArgumentNullException($"{nameof(handler)} can not be null.");
				}

				_shortcuts.Add((new KeyboardEventData(key, modifiers), handler));
				return this;
			}
		}
	}

	public struct KeyboardEventData
	{
		public KeyboardEventData(Key key) : this(key, KeyModifiers.None) { }

		public KeyboardEventData(Key key, KeyModifiers keyModifiers)
		{
			Key = key;
			Modifiers = keyModifiers;
		}

		public Key Key { get; }
		public KeyModifiers Modifiers { get; }

		public override bool Equals(object obj)
		{
			return obj is KeyboardEventData other ? Key.Equals(other.Key) && Modifiers.Equals(other.Modifiers) : false;
		}

		public override int GetHashCode() => Key.GetHashCode() * Modifiers.GetHashCode();
	}

	public class KeyboardEventDataEquallityComparer : IEqualityComparer<KeyboardEventData>
	{
		public bool Equals([AllowNull] KeyboardEventData x, [AllowNull] KeyboardEventData y) => x.Equals(y);
		public int GetHashCode([DisallowNull] KeyboardEventData obj) => obj.GetHashCode();
	}

	[Flags]
	public enum KeyModifiers
	{
		None = 0,
		Shift = 0x01,
		Ctrl = 0x02,
		Alt = 0x04
	}

	public enum Key
	{
		Backspace = 8,
		Tab = 9,
		Enter = 13,
		Shift = 16,
		Ctrl = 17,
		Alt = 18,
		Pause = 19,
		Capslock = 20,
		Escape = 27,
		PageUp = 33,
		Space = 32,
		Pagedown = 34,
		End = 35,
		Home = 36,
		ArrowLeft = 37,
		ArrowUp = 38,
		ArrowRight = 39,
		ArrowDown = 40,
		PrintScreen = 44,
		Insert = 45,
		Delete = 46,
		Num0 = 48,
		Num1 = 49,
		Num2 = 50,
		Num3 = 51,
		Num4 = 52,
		Num5 = 53,
		Num6 = 54,
		Num7 = 55,
		Num8 = 56,
		Num9 = 57,
		A = 65,
		B = 66,
		C = 67,
		D = 68,
		E = 69,
		F = 70,
		G = 71,
		H = 72,
		I = 73,
		J = 74,
		K = 75,
		L = 76,
		M = 77,
		N = 78,
		O = 79,
		P = 80,
		Q = 81,
		R = 82,
		S = 83,
		T = 84,
		U = 85,
		V = 86,
		W = 87,
		X = 88,
		Y = 89,
		Z = 90,
		LeftWindow = 91,
		RightWindowy = 92,
		Select = 93,
		NumPad0 = 96,
		NumPad1 = 97,
		NumPad2 = 98,
		NumPad3 = 99,
		NumPad4 = 100,
		NumPad5 = 101,
		NumPad6 = 102,
		NumPad7 = 103,
		NumPad8 = 104,
		NumPad9 = 105,
		Multiply = 106,
		Add = 107,
		Subtract = 109,
		DecimalPoint = 110,
		Divide = 111,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
		NumLock = 144,
		ScrollLock = 145,
		Computer = 182,
		Calculator = 183,
		Semicolon = 186,
		Equalsign = 187,
		Comma = 188,
		Dash = 189,
		Period = 190,
		ForwardSlash = 191,
		OpenBracket = 219,
		BackSlash = 220,
		CloseNraket = 221,
		SingleQuote = 222,
	}
}
