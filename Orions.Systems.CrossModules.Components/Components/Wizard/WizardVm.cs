﻿using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WizardVm : BlazorVm
	{
		private string _wizardId = null;
		private string _lastInputValue = null;
		private WizardStage _currentStage = null;
		private GetHyperJobsStatusesArgs _pendingStatusArgs = null;
		private HyperJobConfig _pendingConfig = null;

		public bool IsLoadedData { get; set; }
		public bool IsWizardFinish { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public bool ShowPropertyGrid { get; set; }
		public bool ShowConfirmDialog { get; set; }

		public List<WizardItem> Items { get; set; } = new List<WizardItem>();

		public WizardItem SelectedItem { get; set; }

		public LinkedList<WizardItem> SelectionHistory { get; set; } = new LinkedList<WizardItem>();

		public string MessageDescription { get; set; }

		public string TextBlockStatus { get; set; }

		public string ConfirmDialogTitle { get; set; }
		public string ConfirmDialogMessage { get; set; }

		public WizardVm()
		{
		}

		public async Task InitWizzard()
		{
			if (HyperStore == null) return;

			if (!string.IsNullOrWhiteSpace(_wizardId))
			{
				await this.HyperStore.ExecuteAsync(new StepAheadWizardArgs() { JobId = _wizardId, Abort = true });

				_wizardId = null;
			}

			_pendingConfig = null;
			_currentStage = null;

			var args = new GetHyperJobTypesArgs();
			var wizardTypes = await this.HyperStore.ExecuteAsync(args);
			if (wizardTypes != null)
			{
				var wTypes = wizardTypes.Where(it => it.Name.ToLower().Contains("wizard")).ToArray();

				if (wTypes != null)
				{
					Items = wTypes.Select(it => new WizardItem { TypeSource = it }).ToList();
				}
			}

			IsLoadedData = true;
		}

		public async Task OnOpenWizzard(WizardItem item)
		{
			IsLoadedData = false;

			if (item != null) {
				SelectedItem = item;

				SelectionHistory.AddLast(item);
			}

			WizardStageResult selections = null;
			if (_wizardId == null)
			{
				var wizardType = item.TypeSource;
				if (wizardType == null || this.HyperStore == null)
				{
					IsLoadedData = true;
					return;
				}

				var attr = wizardType.AsType()?.GetCustomAttributes(true);
				var configAttr = attr?.OfType<ConfigAttribute>().FirstOrDefault();

				if (_pendingConfig == null)
				{
					var configType = configAttr?.ConfigType;

					_pendingConfig = configType != null ? (HyperJobConfig)Activator.CreateInstance(configType) : null;
					if (_pendingConfig != null)
					{
						// Show the wizard properties before anything else.
						ShowPropertyGrid = true;
						IsLoadedData = true;

						SelectionHistory.RemoveLast(); // 

						return;
					}
				}

				// Create a new wizard.
				var res = await this.HyperStore.ExecuteAsync(new StartHyperJobArgs(wizardType) { JobConfig = _pendingConfig });

				// Clean it up from the last populate.
				Items.Clear();
				_pendingConfig = null;
				_wizardId = res.JobId;

			}
			else
			{
				
				if (_currentStage is WizardListStage)
				{
					//selections = new WizardStageResult() { Results = Items.Where(it=>it.Selected == true).Select(it => it.Name).ToArray() };
					selections = new WizardStageResult() { Results = new[] { SelectedItem.Value } };
				}
				else if (_currentStage is WizardDataInputStage)
				{
					//selections = new WizardStageResult() { Results = new string[] { _lastInputValue } };
					selections = new WizardStageResult() { Results = new string[] { ConfirmDialogMessage } };
				}
			}

			MessageDescription = "Wizard working...";
			Items.Clear();

			_currentStage = await this.HyperStore.ExecuteAsync(new StepAheadWizardArgs() { JobId = _wizardId, PreviousStepResult = selections });

			if (_currentStage == null || _currentStage.IsFinal)
			{
				if (_wizardId != null)
				{
					MessageDescription = "Wizard Completed, Hit RESET to start over";
					Items.Clear();
					IsWizardFinish = true;
				}
			}
			else
			{
				var listStage = _currentStage as WizardListStage;
				var inputStage = _currentStage as WizardDataInputStage;

				if (listStage != null)
				{
					var items = listStage.Entries.Select(it => new WizardItem() { Title = it.Title, Selected = it.PreSelected, Value = it.Value });

					Items.Clear();
					Items.AddRange(items);

					// MultiSelect ?

					MessageDescription = _currentStage.Title;


				}
				else if (inputStage != null)
				{

					ShowConfirmDialog = true;

					ConfirmDialogTitle = inputStage.Title;
					if (inputStage.DefaultValue != null)
					{
						ConfirmDialogMessage = Convert.ToString(inputStage.DefaultValue);
					}

				}

			}

			IsLoadedData = true;
		}

		public async Task ResetSelection()
		{
			IsLoadedData = false;
			IsWizardFinish = false;

			SelectionHistory = new LinkedList<WizardItem>();
			SelectedItem = null;

			var id = _wizardId;

			_wizardId = null;
			if (id != null && this.HyperStore != null)
			{
				await this.HyperStore.ExecuteAsync(new StepAheadWizardArgs() { JobId = id, Abort = true });
			}

			_pendingConfig = null;
			_currentStage = null;


			MessageDescription = "Select Wizard";
			TextBlockStatus = String.Empty;

			if (this.HyperStore != null)
			{
				var args = new GetHyperJobTypesArgs();
				var wizardTypes = await this.HyperStore.ExecuteAsync(args);
				if (wizardTypes != null)
				{
					var wTypes = wizardTypes.Where(it => it.Name.ToLower().Contains("wizard")).ToArray();

					if (wTypes != null)
					{
						Items = wTypes.Select(it => new WizardItem { TypeSource = it }).ToList();
					}
				}
			}

			IsLoadedData = true;
		}


		public async Task CloseConfirmDialog()
		{
			ShowConfirmDialog = false;

			await ResetSelection();
		}

		public async Task ApplyConfirmDialog()
		{
			ShowConfirmDialog = false;

			// Auto-move ahead on entry.

			await OnOpenWizzard(null);

		}


		public void UpdateUI()
		{
			var id = _wizardId;

			if (this.HyperStore == null || string.IsNullOrEmpty(id) || _pendingStatusArgs != null)
				return;

			_pendingStatusArgs = new GetHyperJobsStatusesArgs() { JobsIds = new string[] { _wizardId } };

			this.HyperStore.ExecuteAsync(_pendingStatusArgs).AsTask(TimeSpan.FromSeconds(30)).ContinueWith(a =>
			{
				_pendingStatusArgs = null;

				if (a.IsFaulted)
					return;

				var statuses = a.Result;
				if (statuses != null && statuses.Length > 0)
				{
					TextBlockStatus = statuses.Last().ToString();
				}
			});
		}


		public async Task OnCancelProperty()
		{
			PropertyGridVm.CleanSourceCache();
			ShowPropertyGrid = false;

			await OnOpenWizzard(SelectedItem);
		}


		public Task<object> LoadPropertyGrid()
		{
			return Task.FromResult((object)_pendingConfig);
		}

		public void OnClickFilter(WizardItem item) { 
		
		}

	}

	public class WizardHistoryItem
	{
		public string Name { get; set; }

		public string Value { get; set; }

		public bool IsPropertySelection { get; set; }
	}

	public class WizardItem
	{
		public bool Selected { get; set; }

		public string Title { get; set; }

		public string Value { get; set; }

		public string Name
		{
			get
			{
				if (TypeSource != null)
				{
					return TypeSource.Name;
				}

				return Title;
			}
		}

		public UniType TypeSource { get; set; }

		public override string ToString()
		{
			return Title;
		}
	}
}
