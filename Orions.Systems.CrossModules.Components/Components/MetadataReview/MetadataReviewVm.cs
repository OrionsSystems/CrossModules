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
        private NetStore _store;
        private HyperDocumentId _metadataSetId;

        public ViewModelProperty<List<HyperTag>> HyperTags = new ViewModelProperty<List<HyperTag>>(new List<HyperTag>());
        public ViewModelProperty<int> PageNumber { get; set; } = new ViewModelProperty<int>(1);
        public ViewModelProperty<int> PageSize { get; set; } = new ViewModelProperty<int>(8);
        public ViewModelProperty<long> TotalPages { get; set; } = new ViewModelProperty<long>();

        public bool PlayerOpened { get; set; } = false;
        public string PlayerUri { get; set; }
        public string PlayerId { get; set; }

        public async Task Initialize(NetStore store)
        {
            this._store = store;

            //_metadataSetId = new HyperDocumentId("d1e73018-e487-4d90-b8e0-8bcd530ed3d9", typeof(HyperMetadataSet));
            _metadataSetId = new HyperDocumentId("101fa54d-361c-4631-8540-dde128e08205", typeof(HyperMetadataSet));

            await LoadTotalPages();
            await LoadHyperTags();
        }        

        public async Task LoadTotalPages()
        {
            var countArgs = new CountHyperDocumentsArgs(typeof(HyperTag));

            var totalTags = await CountHyperDocumentsArgs.CountAsync<HyperTag>(this._store, countArgs);

            TotalPages.Value = totalTags % PageSize == 0 ? totalTags / PageSize : totalTags / (PageSize + 1);
        }

        public async Task LoadHyperTags()
        {
            var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag));

            var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(_store, _metadataSetId);
            findArgs.DescriptorConditions.AddCondition(conditions);
            findArgs.Skip = PageSize * (PageNumber - 1);
            findArgs.Limit = PageSize;

            var docs = await _store.ExecuteAsync(findArgs);

            var hyperTags = new List<HyperTag>();
            foreach (var doc in docs)
            {
                hyperTags.Add(doc.GetPayload<HyperTag>());
            }

            HyperTags.Value = hyperTags;
        }

        public async Task ChangePage(int pageNumber)
        {
            PageNumber.Value = pageNumber;

            HyperTags.Value = new List<HyperTag>();

            await LoadHyperTags();
        }
    }
}
