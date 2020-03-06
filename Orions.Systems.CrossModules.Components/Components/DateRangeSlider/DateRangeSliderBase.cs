using Microsoft.AspNetCore.Components;
using Orions.Systems.CrossModules.Components.Components.DateRangeSlider.Utils;
using Syncfusion.EJ2.Blazor.Inputs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class DateRangeSliderBase : ComponentBase
	{
		private DateTime? _minDate;

		[Parameter]
		public DateTime? MinDate
		{
			get 
			{ 
				return _minDate; 
			}
			set 
			{ 
				if(DateRangeChangedThrottleEventCallback?.ThrottlingIsInProgress == false)
				{
					_minDate = value;
					CalculateSliderValue();
				}
			}
		}

		private DateTime? _maxDate;

		[Parameter]
		public DateTime? MaxDate
		{
			get 
			{ 
				return _maxDate; 
			}
			set 
			{
				if (DateRangeChangedThrottleEventCallback?.ThrottlingIsInProgress == false)
				{
					_maxDate = value;
					CalculateSliderValue();
				}
			}
		}

		private DateTime? _currentMinDate;
		[Parameter]
		public DateTime? CurrentMinDate
		{
			get { return _currentMinDate ?? MinDate; }
			set 
			{
				if (DateRangeChangedThrottleEventCallback?.ThrottlingIsInProgress == false)
				{
					_currentMinDate = value;
				}
			}
		}

		private DateTime? _currentMaxDate;
		[Parameter]
		public DateTime? CurrentMaxDate
		{
			get { return _currentMaxDate ?? MaxDate; }
			set 
			{
				if (DateRangeChangedThrottleEventCallback?.ThrottlingIsInProgress == false)
				{
					_currentMaxDate = value;
				}
			}
		}

		[Parameter]
		public int ThrottleTimeout { get; set; } = 3500;

		[Parameter]
		public EventCallback<DateTime[]> DateRangeChanged { get; set; }
		private ThrottleAction<DateTime[]> DateRangeChangedThrottleEventCallback;

		#region Protected
		protected EjsSlider<double[]> SliderObj;
		protected double[] SliderValue;
		protected double SliderMax;
		protected bool IsInitialized { 
			get 
			{
				return this.MinDate != null && this.MaxDate != null;
			} 
		}
		#endregion // Protected


		public DateRangeSliderBase()
		{
		}

		public override Task SetParametersAsync(ParameterView parameters)
		{
			return base.SetParametersAsync(parameters);
		}

		protected override void OnParametersSet()
		{
			System.Diagnostics.Debug.WriteLine($"{nameof(DateRangeSlider)} component: {nameof(OnParametersSet)}");

			if (DateRangeChanged.HasDelegate)
			{
				Action<DateTime[]> throttleCallback = delegate (DateTime[] range)
				{
					this.InvokeAsync(async () =>
					{
						await this.DateRangeChanged.InvokeAsync(range);
						this.StateHasChanged();
					});
				};

				DateRangeChangedThrottleEventCallback = new ThrottleAction<DateTime[]>(throttleCallback, this.ThrottleTimeout);
			}

			base.OnParametersSet();
		}

		private void CalculateSliderValue()
		{
			if (IsInitialized)
			{
				var sliderValue = new double[] { (double)((CurrentMinDate - MinDate).Value.TotalSeconds), (double)((CurrentMaxDate - MinDate).Value.TotalSeconds) };

				this.SliderMax = (this.MaxDate - this.MinDate).Value.TotalSeconds;
				this.SliderValue = sliderValue;
			}
		}

		private void RecalculateDatesBasedOnSliderValue(double[] sliderValue)
		{
			if (IsInitialized)
			{
				var maxSliderValue = (this.MaxDate - this.MinDate).Value.TotalSeconds;
				this._currentMinDate = MinDate.Value.AddSeconds((MaxDate - MinDate).Value.TotalSeconds * (sliderValue[0] / maxSliderValue));
				this._currentMaxDate = MinDate.Value.AddSeconds((MaxDate - MinDate).Value.TotalSeconds * (sliderValue[1] / maxSliderValue));
			}
		}

		protected void OnSliderValueChange(SliderChangeEventArgs<double[]> e)
		{
			System.Diagnostics.Debug.WriteLine($"Component: {nameof(DateRangeSlider)}, Event: ValueChange. Slider value changed to [{e.Value[0]}, {e.Value[1]}]");

			RecalculateDatesBasedOnSliderValue(e.Value);
			this.SliderValue = e.Value;

			DateRangeChangedThrottleEventCallback?.Invoke(new DateTime[] { this.CurrentMinDate.Value, this.CurrentMaxDate.Value });
		}

		protected void OnSliderChange(SliderChangeEventArgs<double[]> e)
		{
			System.Diagnostics.Debug.WriteLine($"Component: {nameof(DateRangeSlider)}, Event: Change. Slider value changed to [{e.Value[0]}, {e.Value[1]}]");

			RecalculateDatesBasedOnSliderValue(e.Value);

		}
	}
}
