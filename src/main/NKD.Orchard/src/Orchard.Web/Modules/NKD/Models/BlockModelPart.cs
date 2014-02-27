using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System.Linq;
using System;

namespace NKD.Models {

    public class BlockModelPartRecord : ContentPartRecord
    {
        public virtual string BmFileName { get; set; }
        public virtual string FormatFileName { get; set; }
        public virtual Guid ProjectID { get; set; }
        public virtual string ProjectName { get; set; }
        public virtual string Alias { get; set; }
        public virtual Guid UserID { get; set; }
        [StringLengthMax]
        public virtual string Notes { get; set; }
        public virtual string Stage { get; set; }
        public virtual Guid StageMetaID { get; set; }
        public virtual Guid BmGuid { get; set; }
        public virtual string ColumnNameToAdd { get; set; }
        public virtual int ColumnIndexToAdd { get; set; }
        [StringLengthMax]
        public virtual string Recipients { get; set; }
        public virtual DateTime? Processed { get; set; }
        public virtual DateTime? Completed { get; set; }
    }

    public class BlockModelPart : ContentPart<BlockModelPartRecord> 
    {
        public string BmFileName { get { return Record.BmFileName; } set { Record.BmFileName = value; } }
        public string FormatFileName { get { return Record.FormatFileName; } set { Record.FormatFileName = value; } }
        public Guid ProjectID { get { return Record.ProjectID; } set { Record.ProjectID = value; } }
        public string ProjectName { get { return Record.ProjectName; } set { Record.ProjectName = value; } }
        public string Alias { get { return Record.Alias; } set { Record.Alias = value; } }
        public Guid UserID { get { return Record.UserID; } set { Record.UserID = value; } }
        public string Notes { get { return Record.Notes; } set { Record.Notes = value; } }
        public string Stage { get { return Record.Stage; } set { Record.Stage = value; } }
        public Guid StageMetaID { get { return Record.StageMetaID; } set { Record.StageMetaID = value; } }
        public Guid BmGuid { get { return Record.BmGuid; } set { Record.BmGuid = value; } }
        public string ColumnNameToAdd { get { return Record.ColumnNameToAdd; } set { Record.ColumnNameToAdd = value; } }
        public int ColumnIndexToAdd { get { return Record.ColumnIndexToAdd; } set { Record.ColumnIndexToAdd = value; } }
        public string Recipients { get { return Record.Recipients; } set { Record.Recipients = value; } }
        public DateTime? Processed { get { return Record.Processed; } set { Record.Processed = value; } }
        public DateTime? Completed { get { return Record.Completed; } set { Record.Completed = value; } }

        public BlockModelPart()
        { }

    }

}