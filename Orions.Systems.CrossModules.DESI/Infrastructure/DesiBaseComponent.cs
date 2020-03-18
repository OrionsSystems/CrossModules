using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Systems.Desi.Core.ViewModels.Abstract;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Orions.Systems.CrossModules.Desi.Debug.Infrastructure
{
	public class DesiBaseComponent<T> : ComponentBase where T : ViewModelBase
	{
		private T _vm;
		public T Vm
		{
			get
			{
				return _vm;
			}
			set
			{
				_vm = value;
				if (value != null)
				{
					AddVmDataPropertyChangedHandlers(value);
				}
			}
		}

		[Inject]
		public BlazorDependencyResolver DependencyResolver { get; set; }

		public DesiBaseComponent()
		{
		}


		// This method adds StateHasChanged method call to every event fired on any Vm property that implements either INotifyPropertyChanged or
		// INotifyCollectionChanged interface
		private void AddVmDataPropertyChangedHandlers(object vm)
		{
			PropertyChangedEventHandler propChangedHandler = (s, e) => this.InvokeAsync(() => this.StateHasChanged());
			NotifyCollectionChangedEventHandler collectionChangedHandler = (s, e) =>
			{
				if (e.NewItems != null && (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace))
				{
					foreach(var item in e.NewItems)
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
					var vmProps = vmType.GetProperties();
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
		}
	}
}
