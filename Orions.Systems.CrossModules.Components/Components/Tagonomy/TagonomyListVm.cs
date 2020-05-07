using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class TagonomyListVm : BlazorVm
	{
		public bool IsLoadedData { get; set; }

		public string Search { get; set; }

		public IHyperArgsSink HyperStore { get; set; }

		public PropertyGridVm PropertyGridVm { get; set; } = new PropertyGridVm();

		public bool IsShowProperty { get; set; }

		public List<Tagonomy> Items { get; set; } = new List<Tagonomy>();

		public Tagonomy SelectedItem { get; set; }

		public EventCallback<Tagonomy> OnManage { get; set; }

		public TagonomyListVm()
		{

		}

		public async Task Init()
		{
			if (HyperStore == null) return;

			await Populate();
		}

		public async Task ManageAsync(Tagonomy item)
		{
			await OnManage.InvokeAsync(item);
		}

		private async Task Populate()
		{
			Items.Clear();
			IsLoadedData = false;


			var findArgs = new FindHyperDocumentsArgs();
			findArgs.SetDocumentType(typeof(Tagonomy));

			if (string.IsNullOrWhiteSpace(Search) == false)
			{
				Guid parsedGuid;
				if (Guid.TryParse(Search, out parsedGuid))
				{
					findArgs.DocumentConditions.AddCondition("_id", Search);
				}
				else
				{
					var condition = new MultiScopeCondition();

					var conditionOr = new MultiScopeCondition()
					{
						Mode = AndOr.Or
					};
					var regex = $"/{Search}/ig";
					conditionOr.AddCondition(nameof(Tagonomy.Name), regex, Comparers.Regex);
					//conditionOr.AddCondition(nameof(Tagonomy.Id), regex, ScopeCondition.Comparators.Regex);
					conditionOr.AddCondition(nameof(Tagonomy.Group), regex, Comparers.Regex);
					condition.AddCondition(conditionOr);

					findArgs.DescriptorConditions.AddCondition(condition);
				}
			}

			var tagonomies = await HyperStore.ExecuteAsync(findArgs);

			if (tagonomies == null || !tagonomies.Any())
				return;

		

			foreach (var tagonomy in tagonomies)
			{
				var configuration = tagonomy.GetPayload<Tagonomy>();
				if (configuration == null)
				{
					Console.WriteLine($"Failed to load tagonomy from document: {tagonomy.Id}");
					continue;
				}

				Items.Add(configuration);
			}

			Items = Items.OrderBy(it => it.Group).ToList();

			IsLoadedData = true;
		}
		

		public async Task OnSearchBtnClick(MouseEventArgs e)
		{
			await Populate();
		}

		public async Task CreateNew()
		{
			//TODO
		}

		public void ShowPropertyGrid(Tagonomy item)
		{
			SelectedItem = item;
			IsShowProperty = true;
		}

		public Task<object> LoadPropertyGrid()
		{
			return Task.FromResult<object>(SelectedItem);
		}

		public void OnCancelProperty()
		{
			PropertyGridVm.CleanSourceCache();
			IsShowProperty = false;
		}
	}
}
