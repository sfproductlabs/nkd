using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using NKD.Helpers;
using System;
using DevExpress.XtraReports.UI;
using System.Data;

 namespace NKD.ViewModels
{
    public class BlockModelCompareViewModel : IReport
    {
        [Required, DisplayName("Model 1:")]
        public Guid Model1 { get; set; }
        [Required, DisplayName("Model 2:")]
        public Guid Model2 { get; set; }
        [DisplayName("Model 1 Name:")]
        public string Model1Name { get; set; }
        [DisplayName("Model 2 Name:")]
        public string Model2Name { get; set; }
        [DisplayName("Domains Model 1:")]
        public IList<string> SelectedDomainsModel1 { get; set; }
        [DisplayName("Domains Model 1 Compact:")]
        public string SelectedDomainsModel1Compact { get; set; }
        [DisplayName("Domains Model 2:")]
        public IList<string> SelectedDomainsModel2 { get; set; }
        [DisplayName("Domains Model 2 Compact:")]
        public string SelectedDomainsModel2Compact { get; set; }
        [DisplayName("Report ID:")]
        public uint ReportID { get; set; }
        [DisplayName("Report Name:")]
        public string ReportName { get; set; }
        [DisplayName("Report:")]
        public XtraReport Report { get; set; }
        [DisplayName("Parameters:")]
        public string ParametersView { get; set; }
        [Required, DisplayName("Grade Field:")]
        public Guid GradeTonnageFieldID { get; set; }
        [DisplayName("Grade Field Name:")]
        public string GradeTonnageFieldName { get; set; }
        [Required, DisplayName("Increment:"), DefaultValue(0.1)]
        public double GradeTonnageIncrement { get; set; }
        [DisplayName("Slice Field:")]
        public Guid SliceFieldID { get; set; }
        [DisplayName("Slice Group By:")]
        public Guid SliceFilterFieldID { get; set; }
        [DisplayName("Slice Group By Field Name:")]
        public string SliceFilterFieldName { get; set; }
        [DisplayName("Width X:")]
        public double SliceWidthX { get; set; }
        [DisplayName("Width Y:")]
        public double SliceWidthY { get; set; }
        [DisplayName("Width Z:")]
        public double SliceWidthZ{ get; set; }
        [DisplayName("Serialized Info:")]
        public string SerializedChild { get; set; }
        [DisplayName("Filter String:")]
        public string FilterString { get; set; }
        [DisplayName("Report Executed By Username:")]
        public string ReportExecutedByUserName { get; set; }
        [DisplayName("Report Executed By User:")]
        public Guid ReportExecutedByUserID { get; set; }
        public Func<object, DataSet> ReportResult { get; set; }

        public SelectList Models { get; set; }
        public SelectList ParametersModel1 { get; set; }
        public MultiSelectList DomainsModel1 { get; set; }
        public SelectList ParametersModel2 { get; set; }
        public MultiSelectList DomainsModel2 { get; set; }
        public SelectList ParametersIntersectionBothModels { get; set; }
    }
}