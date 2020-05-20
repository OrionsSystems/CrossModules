using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.Desi.Common.Util;

namespace Orions.Systems.CrossModules.Desi.Services
{
	public class KeyboardListener: IKeyboardListener
	{
		private readonly Subject<KeyboardEventData> _keyEventSubject = new Subject<KeyboardEventData>();

		public KeyboardListener(IJSRuntime jSRuntime)
		{
			var handle = DotNetObjectReference.Create(this);
			jSRuntime.InvokeVoidAsync("Orions.KeyboardListener.init", handle);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[JSInvokable]
		public Task OnKeyEvent(Key key, KeyModifiers modifiers, KeyboardEventType type)
		{
			_keyEventSubject.OnNext(new KeyboardEventData(key, modifiers, type));
			return Task.FromResult(true);
		}

		private IObservable<KeyboardEventData> KeyboardObservable => _keyEventSubject.AsObservable();

		public IKeyboardEventSubscription CreateSubscription() => new KeyboardEventSubscription(KeyboardObservable);

		public class KeyboardEventSubscription : IKeyboardEventSubscription
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
				.ToList()
				.Where(i => i.EventData.Equals(keyboardEventData))
				.ForEach(i => i.Handler());

			public IKeyboardEventSubscription AddShortcut(Key key, Action handler) => AddShortcut(key, KeyModifiers.None, handler, KeyboardEventType.KeyDown);

			public IKeyboardEventSubscription AddShortcut(Key key, KeyModifiers modifiers, Action handler, KeyboardEventType type)
			{
				if (handler == null)
				{
					throw new ArgumentNullException($"{nameof(handler)} can not be null.");
				}

				_shortcuts.Add((new KeyboardEventData(key, modifiers, type), handler));
				return this;
			}
		}
	}

	public struct KeyboardEventData
	{
		public KeyboardEventData(Key key) : this(key, KeyModifiers.None, KeyboardEventType.KeyDown) { }

		public KeyboardEventData(Key key, KeyModifiers keyModifiers, KeyboardEventType type)
		{
			Key = key;
			Modifiers = keyModifiers;
			KeyboardEventType = type;
		}

		public Key Key { get; }
		public KeyModifiers Modifiers { get; }
		public KeyboardEventType KeyboardEventType { get; }

		public override bool Equals(object obj)
		{
			return obj is KeyboardEventData other ? Key.Equals(other.Key) && Modifiers.Equals(other.Modifiers) && KeyboardEventType.Equals(other.KeyboardEventType): false;
		}

		public override int GetHashCode() => Key.GetHashCode() * Modifiers.GetHashCode();
	}

	public class KeyboardEventDataEquallityComparer : IEqualityComparer<KeyboardEventData>
	{
		public bool Equals([AllowNull] KeyboardEventData x, [AllowNull] KeyboardEventData y) => x.Equals(y);
		public int GetHashCode([DisallowNull] KeyboardEventData obj) => obj.GetHashCode();
	}
}
