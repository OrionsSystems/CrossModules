using System;
using Microsoft.AspNetCore.Components;
using Orions.Systems.Desi.Core.ViewModels.Abstract;
using Orions.Systems.Desi.Common.Util;

namespace Orions.Systems.CrossModules.Desi.Infrastructure
{
	public class BaseViewModelComponent<VmType> : BaseComponent
		where VmType : ViewModelBase
	{
		private VmType _vm;
		private IDisposable _propertyChangedTracker;

		[Inject]
		public VmType Vm
		{
			get => _vm;
			set
			{
				if (value != null && value != _vm && AutoWirePropertyChangedListener)
				{
					_propertyChangedTracker?.Dispose();
					_propertyChangedTracker = value.CreatePropertyChangedTracker(UpdateState);
				}
				_vm = value;
			}
		}

		protected virtual bool AutoWirePropertyChangedListener { get; } = true;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_propertyChangedTracker?.Dispose();
				_propertyChangedTracker = null;
			}
			base.Dispose(disposing);
		}

		// This method adds StateHasChanged method call to every event fired on any Vm property that implements either INotifyPropertyChanged or
		// INotifyCollectionChanged interface
		// Replace it with PropertyTracker
		/*
		protected void AddVmDataPropertyChangedHandlers(object vm)
		{
			PropertyChangedEventHandler propChangedHandler = (s, e) =>
			{
				if (e != null && e.PropertyName != null)
				{
					var prop = s.GetType().GetProperty(e.PropertyName);
					if (prop != null)
					{
						var suppressUiUpdateAttr = prop.GetCustomAttributes(typeof(SuppressUiUpdate), true);
						if (suppressUiUpdateAttr.Any(a => (a as SuppressUiUpdate).AppType == "BlazorDesi"))
						{
							return;
						}
					};
				}

				if (e?.PropertyName != null)
				{
					var newPropValue = s.GetType().GetProperty(e.PropertyName)?.GetValue(s);

					if (newPropValue != null)
					{
						AddVmDataPropertyChangedHandlers(newPropValue);
					}
				}

				UpdateState();
			};
			NotifyCollectionChangedEventHandler collectionChangedHandler = (s, e) =>
			{
				if (e.NewItems != null && (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace))
				{
					foreach (var item in e.NewItems)
					{
						AddVmDataPropertyChangedHandlers(item);
					}
				}

				propChangedHandler(s, null);
			};

			var notifyPropertyChangedType = typeof(INotifyPropertyChanged);
			var observableCollectionType = typeof(INotifyCollectionChanged);
			var enumerableType = typeof(IEnumerable);
			var vmType = vm.GetType();

			if (notifyPropertyChangedType.IsAssignableFrom(vmType))
			{
				var vmAsNotifyPropChanged = vm as INotifyPropertyChanged;
				vmAsNotifyPropChanged.PropertyChanged += propChangedHandler;

				if (!enumerableType.IsAssignableFrom(vmType))
				{
					var vmProps = vmType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
					foreach (var prop in vmProps)
					{
						var propValue = prop.GetValue(vm);
						if (propValue != null)
						{
							AddVmDataPropertyChangedHandlers(propValue);
						}
					}
				}
			}

			if (observableCollectionType.IsAssignableFrom(vmType))
			{
				var propValueAsObservableColleciton = vm as INotifyCollectionChanged;
				propValueAsObservableColleciton.CollectionChanged += collectionChangedHandler;

				var propValueAsEnumerable = vm as IEnumerable;
				foreach (var item in propValueAsEnumerable)
				{
					AddVmDataPropertyChangedHandlers(item);
				}
			}
		}*/
	}
}
