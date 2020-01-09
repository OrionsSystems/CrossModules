using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
    public class MetadataReviewVm : BlazorVm
    {
        public ViewModelProperty<List<HyperTag>> HyperTags = new ViewModelProperty<List<HyperTag>>(new List<HyperTag>());
        private NetStore _store;
        private HyperDocumentId _hyperDocumentId;

        public ViewModelProperty<int> PageNumber { get; set; } = new ViewModelProperty<int>(1);
        public ViewModelProperty<int> PageSize { get; set; } = new ViewModelProperty<int>(8);

        public ViewModelProperty<long> TotalPages { get; set; } = new ViewModelProperty<long>();

        public async Task Oninitialize(NetStore store)
        {
            this._store = store;

            _hyperDocumentId = new HyperDocumentId("33cbd135-f5d6-49dd-baac-fc6be31a52fb", typeof(HyperMetadataSet));

            await LoadTotalPages();
            await LoadHyperTags();

            
        }

        public async Task LoadTotalPages()
        {
            var countArgs = new CountHyperDocumentsArgs(typeof(HyperTag));

            var totalTags = await CountHyperDocumentsArgs.CountAsync<HyperTag>(this._store, countArgs);

            TotalPages = totalTags % PageSize == 0 ? totalTags / PageSize : totalTags / (PageSize + 1);
        }

        public async Task LoadHyperTags()
        {
            var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));

            var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(_store, _hyperDocumentId);
            findArgs.DescriptorConditions.AddCondition(conditions);
            findArgs.Skip = PageSize * (PageNumber - 1);
            findArgs.Limit = PageSize;

            var docs = await _store.ExecuteAsync(findArgs);

            var hyperTags = new List<HyperTag>();
            foreach (var doc in docs)
            {
                hyperTags.Add(doc.GetPayload<HyperTag>());
            }

            HyperTags = hyperTags;
        }

        public async Task ChangePage(int pageNumber)
        {
            PageNumber.Value = pageNumber;

            await LoadHyperTags();
        }
    }
}
