using Orions.Systems.CrossModules.Components.Desi.Services;
using Orions.Systems.Desi.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Domain.Desi
{
	public class KeyboardListenerStub : IKeyboardListener
	{
		public IKeyboardEventSubscription CreateSubscription()
		{
			return new KeyboardEventSubscriptionStub();
		}
	}

	public class KeyboardEventSubscriptionStub : IKeyboardEventSubscription
	{
		public IKeyboardEventSubscription AddShortcut(Key key, Action handler)
		{
			return this;
		}

		public IKeyboardEventSubscription AddShortcut(Key key, KeyModifiers modifiers, Action handler, KeyboardEventType type)
		{
			return this;
		}

		public void Dispose()
		{
		}
	}
}
