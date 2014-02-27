using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Activities.Persistence;
using System.Xml.Linq;

namespace NKD.Workflow
{

    public partial class SubmitResponsibility : NativeActivity
    {
        [RequiredArgument]
        public OutArgument<Guid> SubmittedContactID { get; set; }
        [RequiredArgument]
        public OutArgument<Guid> SubmittedCompanyID { get; set; }
        [RequiredArgument]
        public InArgument<string> BookmarkName { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddDefaultExtensionProvider<Helpers.WorkflowInstanceExtensionHelper>(() => new Helpers.WorkflowInstanceExtensionHelper());
        }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var bookmark = context.CreateBookmark(BookmarkName.Get(context), BookmarkResumed);
            var extension = context.GetExtension<Helpers.WorkflowInstanceExtensionHelper>();
            //extension.WaitSome(bookmark);

        }


        private void BookmarkResumed(NativeActivityContext context, Bookmark bookmark, object value)
        {
            var dict = (Dictionary<string,object>)value;
            var company = (Guid)dict["CompanyID"];
            var contact = (Guid)dict["ContactID"];
            this.SubmittedCompanyID.Set(context, company);
            this.SubmittedContactID.Set(context, contact);
            context.GetExtension<ResponsibilityExtension>().CompanyID = company;
            context.GetExtension<ResponsibilityExtension>().ContactID = contact;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine("Bookmark resumed with '{0}'.", value);
            }
            
        }
    }

    public class ResponsibilityExtension : PersistenceParticipant
    {
        public static readonly XNamespace xNS = XNamespace.Get("http://nkd.org/Responsibility");
        public Guid CompanyID { get; set; }
        public Guid ContactID { get; set; }

        protected override void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            readWriteValues = new Dictionary<XName, object>();
            readWriteValues.Add(xNS.GetName("CompanyID"), this.CompanyID);
            readWriteValues.Add(xNS.GetName("ContactID"), this.ContactID);

            writeOnlyValues = null;
        }
    
    }


}
