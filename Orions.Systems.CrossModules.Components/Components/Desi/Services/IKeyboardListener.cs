﻿using Orions.Systems.Desi.Common.Util;
using System;

namespace Orions.Systems.CrossModules.Components.Desi.Services
{
	public interface IKeyboardListener
	{
		IKeyboardEventSubscription CreateSubscription();
	}

	public interface IKeyboardEventSubscription: IDisposable
	{
		IKeyboardEventSubscription AddShortcut(Key key, Action handler);
		IKeyboardEventSubscription AddShortcut(Key key, KeyModifiers modifiers, Action handler, KeyboardEventType type);
	}
}
