using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NKD.Models {

    public class ProjectPartRecord : ContentPartRecord
    {

    }

    public class ProjectPart : ContentPart<ProjectPartRecord> { 
        public string Name {
            get { return this.As<ITitleAspect>().Title; }
        }     
    }
}