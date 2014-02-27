using System;
using System.Linq;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.Web.Mvc;
using System.Web.Mvc;
using DevExpress.Web.ASPxClasses.Internal;
using DevExpress.XtraReports.Parameters;
using NKD.Helpers;
using System.Data;
using NKD.ViewModels;
using NKD.Services;


namespace NKD.Reports
{
    public partial class CompareModelReport : ReportHelper.TableReport
    {
        public CompareModelReport()
        {
            InitializeComponent();
        }


        public CompareModelReport(ReportHelper.ITableReportDataFiller filler)
            : base(filler)
        {
            InitializeComponent();
            //Name = ReportNames.TableReport;
            //DisplayName = ReportNames.TableReport;
        }

        protected override void BeforeReportPrint()
        {
            base.BeforeReportPrint();
            chartX.Series["Model 1"].Name = string.Format("{0} (*)", this.Parameters["ParameterModel1Name"].Value);
            chartX.Series["Model 2"].Name = string.Format("{0} (**)", this.Parameters["ParameterModel2Name"].Value);
            chartX.Series["Samples 1"].Name = string.Format("{0} Samples (*)", this.Parameters["ParameterModel1Name"].Value);
            chartX.Series["Samples 2"].Name = string.Format("{0} Samples (**)", this.Parameters["ParameterModel2Name"].Value);
            chartY.Series["Model 1"].Name = string.Format("{0} (*)", this.Parameters["ParameterModel1Name"].Value);
            chartY.Series["Model 2"].Name = string.Format("{0} (**)", this.Parameters["ParameterModel2Name"].Value);
            chartY.Series["Samples 1"].Name = string.Format("{0} Samples (*)", this.Parameters["ParameterModel1Name"].Value);
            chartY.Series["Samples 2"].Name = string.Format("{0} Samples (**)", this.Parameters["ParameterModel2Name"].Value);
            chartZ.Series["Model 1"].Name = string.Format("{0} (*)", this.Parameters["ParameterModel1Name"].Value);
            chartZ.Series["Model 2"].Name = string.Format("{0} (**)", this.Parameters["ParameterModel2Name"].Value);
            chartZ.Series["Samples 1"].Name = string.Format("{0} Samples (*)", this.Parameters["ParameterModel1Name"].Value);
            chartZ.Series["Samples 2"].Name = string.Format("{0} Samples (**)", this.Parameters["ParameterModel2Name"].Value);
            chartGT.Series["Model 1"].Name = string.Format("{0} (*)", this.Parameters["ParameterModel1Name"].Value);
            chartGT.Series["Model 2"].Name = string.Format("{0} (**)", this.Parameters["ParameterModel2Name"].Value);
        }
        
        public class DataProvider : ReportHelper.ITableReportDataFiller
        {

            private IReport _r { get; set; }

            public DataProvider()
                : base()
            { }

            public DataProvider(IReport r)
                : base()
            {
                _r = r;
            }

            public void Fill(ReportHelper.TableReport report)
            {
                var o = (BlockModelCompareViewModel)_r;
                var cacheKey = o.ToJson().ComputeHash();
                report.DataSource = CacheHelper.AddToCache<DataSet>(() => { return _r.ReportResult(o); }, cacheKey);
                report.Parameters["ParameterModel1Name"].Value = o.Model1Name;
                report.Parameters["ParameterModel2Name"].Value = o.Model2Name;
                report.Parameters["ParameterGradeTonnageFieldName"].Value = o.GradeTonnageFieldName;
                report.Parameters["ParameterUserName"].Value = o.ReportExecutedByUserName;
              
            }
        }
    }
}
