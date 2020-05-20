import { isDefined } from '../../../../Orions.Systems.CrossModules.Components/content/WebPack/src/utils'

window.Orions.TagPreviewControl = {
	scrollToTag: function (tagId) {
		let tagPreviewControlContainer = document.querySelector('.tag-preview-control-container');
		let tagContainer = document.querySelector(`.tag-container[data-tag-id='${tagId}']`);

		if (isDefined(tagPreviewControlContainer) && isDefined(tagContainer)) {
			tagPreviewControlContainer.scrollTo(0, tagContainer.getBoundingClientRect().y - tagPreviewControlContainer.getBoundingClientRect().y + tagPreviewControlContainer.scrollTop)
		}
	}
}