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
    public partial class AssaysReport : ReportHelper.TableReport
    {
        public AssaysReport()
        {
            InitializeComponent();
        }


        public AssaysReport(ReportHelper.ITableReportDataFiller filler)
            : base(filler)
        {
            InitializeComponent();
            //Name = ReportNames.TableReport;
            //DisplayName = ReportNames.TableReport;
        }

        protected override void BeforeReportPrint()
        {
            base.BeforeReportPrint();
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
                var o = (AssayReportViewModel)_r;
                var cacheKey = string.Format("{0}-{1}", o.ProjectID, o.ReportName);
                var ds = CacheHelper.AddToCache<DataSet>(() => { return _r.ReportResult(o); }, cacheKey, CacheHelper.DefaultTimeout);
                report.DataSource = ds;
                report.Parameters["ParameterProjectID"].Value = o.ProjectID;
                report.Parameters["ParameterUserName"].Value = o.ReportExecutedByUserName;
              
                var tr1 = new XRTableRow { Name = "tr1" };
                tr1.Cells.AddRange((from k in ds.Tables[0].Columns.Cast<DataColumn>() select new XRTableCell { Name = k.ColumnName, Text = k.ColumnName }).ToArray());
                var h = new XRTable { Name = "h", LocationF = new DevExpress.Utils.PointFloat(0F, 0F), SizeF = new System.Drawing.SizeF(600.0000F, 25F), KeepTogether = true };
                h.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] { tr1 });
                report.Bands["Detail"].Controls.Add(h);
              
                var tr2 = new XRTableRow { Name = "tr2" };
                tr2.Cells.AddRange((from k in ds.Tables[0].Columns.Cast<DataColumn>() select new XRTableCell { Name = string.Format("{0}_data", k.ColumnName), Text = string.Format("[{0}]",k.ColumnName) }).ToArray());
                var t = new XRTable { Name = "t", LocationF = new DevExpress.Utils.PointFloat(0F, 0F), SizeF = new System.Drawing.SizeF(600.0000F, 25F), KeepTogether= true };
                t.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] { tr2 });                
                ((DetailReportBand)report.Bands["DetailReport"]).Bands["DetailBand"].Controls.Add(t);
              
            }
        }
    }
}
