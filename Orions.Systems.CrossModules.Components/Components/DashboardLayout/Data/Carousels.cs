using Orions.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Orions.Systems.CrossModules.Components
{
    public class Carousels
    {
        [UniBrowsable(UniBrowsableAttribute.EditTypes.ImageFile)]
        public byte[] Source { get; set; }
        public string Alt { get; set; }
        public string Caption { get; set; }
        public string Header { get; set; }
        public string ActionLink { get; set; }

        [HelpText("_blank:Load in a new windows, _self:Load in the same frame as it was clicked")]
        public ActionLinkTarget ActionLinkTarget { get; set; }
    }

    public enum ActionLinkTarget
    {
        _blank,
        _self
    }
}
