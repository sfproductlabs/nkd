using System;
using System.Linq;
using System.Transactions;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using NKD.Models;
using Orchard.Themes;
using DevExpress.Web.ASPxGridView;
using DevExpress.Web.Mvc;
using DevExpress.Data;
using Orchard.Logging;
using Orchard.Core.Contents.Controllers;
using Orchard.Mvc;
using NKD.ViewModels;
using NKD.Services;
using NKD.Helpers;
using NKD.Reports;
using System.Threading.Tasks;
using ImpromptuInterface;
using NKD.Module.BusinessObjects;

namespace NKD.Controllers {

    [Themed]
    public class UserController : Controller {
        public string Name { get { return "User"; } }
        public IOrchardServices Services { get; set; }
        public IBlockModelService BlockModelService { get; set; }
        public IAssayService AssayService { get; set; }
        public IGeophysicsService GeophysicsService { get; set; }
        public IProjectsService ProjectService { get; set; }
        public IParametersService ParameterService { get; set; }
        public IUsersService UserService { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public IPrivateDataService PrivateService { get; set; }
        public IWorkflowService WorkflowService { get; set; }

        public static readonly string OLAP_XSTRING = global::System.Configuration.ConfigurationManager.ConnectionStrings["NKDOLAP_RM"].ConnectionString;

        public UserController(
            IOrchardServices services, 
            IBlockModelService blockModelService, 
            IAssayService assayService,
            IGeophysicsService geophysicsService,
            IProjectsService projectService, 
            IParametersService parameterService,
            IUsersService userService,
            IPrivateDataService privateService,
            IWorkflowService workflowService
            ) {
            
            Services = services;
            UserService = userService;
            BlockModelService = blockModelService;
            AssayService = assayService;
            GeophysicsService = geophysicsService;
            ParameterService = parameterService;
            ProjectService = projectService;
            PrivateService = privateService;
            WorkflowService = workflowService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            
        }

        private Guid getCurrentUserID()
        {
            return UserService.GetContactID(Services.WorkContext.CurrentUser.UserName).Value;
        }

        [HttpGet]
        public ActionResult CompareModel()
        {
            var model = new BlockModelCompareViewModel
            {
                Models = BlockModelService.GetModelListCurrent(),
                Report = AllReports.GetReport(AllReports.ReportType.CompareModel)
            };
            model.ParametersModel1 = model.Models.Any() ? BlockModelService.GetModelParameterList(new Guid(model.Models.First().Value)) : new SelectList(new SelectListItem[] { });
            model.DomainsModel1 = model.Models.Any() ? BlockModelService.GetModelDomainsList(new Guid(model.Models.First().Value)) : new SelectList(new SelectListItem[] { });
            model.DomainsModel2 = model.DomainsModel1;
            model.ParametersModel2 = model.ParametersModel1;
            model.ParametersIntersectionBothModels = model.ParametersModel1;
            return View("CompareModel", model);
        }

        [HttpGet]
        public ActionResult GetModelDomains(string modelID)
        {
            return Json(BlockModelService.GetModelDomainsList(new Guid(modelID))
                , JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetModelParameters(string modelID)
        {
            return Json(BlockModelService.GetModelParameterList(new Guid(modelID))
                , JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetModelIntersectionParameters(string modelID1, string modelID2)
        {
            
            var j = Json(
                ((new SelectListItem[] { new SelectListItem { Text = "", Value = "" } })
                .Union((from o in BlockModelService.GetModelParameterList(new Guid(modelID1))
                    join m in BlockModelService.GetModelParameterList(new Guid(modelID2))
                    on o.Text equals m.Text where o.Text != "" && o.Text != null
                        select o))
                        .OrderBy(f => f.Text))                    
                , JsonRequestBehavior.AllowGet);
            return j;
        }

        [HttpPost]
        public ActionResult CompareModelResult(BlockModelCompareViewModel m)
        {
            if (!ModelState.IsValid)
                return CompareModel();
            
            m.ReportID = (uint)AllReports.ReportType.CompareModel;
            m.SelectedDomainsModel1Compact = m.SelectedDomainsModel1 != null ? string.Join(";", m.SelectedDomainsModel1.ToArray()) : null;
            m.SelectedDomainsModel2Compact = m.SelectedDomainsModel2 != null ? string.Join(";", m.SelectedDomainsModel2.ToArray()) : null;
            m.Model1Name = BlockModelService.GetModelAlias(m.Model1);
            m.Model2Name = BlockModelService.GetModelAlias(m.Model2);
            m.ReportExecutedByUserName = Services.WorkContext.CurrentUser.UserName;
            return View(m);
        }

        //public async Task<ActionResult> CompareModelResultPartial(BlockModelCompareViewModel m)
        //{
        //    return PartialView(await BlockModelService.CompareModelsAsync(m));
        //}


        [HttpPost]
        public ActionResult CompareModelResultPartial(BlockModelCompareViewModel m)
        {
            IReport r = BlockModelService.CompareModels(m);
            return PartialView(new BlockModelCompareViewModel { 
                Model1 = m.Model1,
                Model1Name = m.Model1Name,
                Model2 = m.Model2,
                Model2Name = m.Model2Name,
                DomainsModel1 = m.DomainsModel1,
                DomainsModel2 = m.DomainsModel2,
                GradeTonnageFieldID = m.GradeTonnageFieldID,
                GradeTonnageFieldName = m.GradeTonnageFieldName,
                GradeTonnageIncrement = m.GradeTonnageIncrement,
                ParametersIntersectionBothModels = m.ParametersIntersectionBothModels,
                ParametersModel1 = m.ParametersModel1,
                ParametersModel2 = m.ParametersModel2,
                ParametersView = r.ParametersView,
                Report = r.Report,
                ReportID = r.ReportID,
                ReportName = r.ReportName,
                SerializedChild = m.SerializedChild,
                SelectedDomainsModel1 = m.SelectedDomainsModel1,
                SelectedDomainsModel2 = m.SelectedDomainsModel2,
                SliceFieldID = m.SliceFieldID,
                SliceFilterFieldID = m.SliceFilterFieldID,
                SliceWidthX = m.SliceWidthX,
                SliceWidthY = m.SliceWidthY,
                SliceWidthZ = m.SliceWidthZ,
                FilterString = m.FilterString,
                ReportExecutedByUserName = Services.WorkContext.CurrentUser.UserName
            });
        }


        public ActionResult ReportViewerExportTo(BlockModelCompareViewModel m)
        {
            return ReportViewerExtension.ExportTo(AllReports.CreateModel(m).Report);
        }

        
        [HttpGet]
        public ActionResult UploadModel() { 
            // this is called when the view is cerated
            var model = new BlockModelUploadViewModel{
                Models = BlockModelService.GetModelList(),
                ProjectList = ProjectService.GetProjectList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult UploadModelResult(BlockModelUploadViewModel m)
        {
            string ss = BlockModelService.GetBlockModelInfo(m.Model1);
            ss = ss + ".MMM";
            m.Test = ss;
            return View(m);
        }

        [HttpPost,  ValidateInput(true)]
        public ActionResult ImportModelProcessStart(BlockModelViewModel m) {

            if (!ModelState.IsValid)
                return ImportModel();

            // TODO do some processing here
            string bmFile = m.FileName;

            string formatSpecFile = m.FormatFileName;// "gf_compact_bm_format.xml";
            double xOrigin = m.XOrigin;
            double yOrigin = m.YOrigin;
            double zOrigin = m.ZOrigin; 
            string projID = m.Project;
            string alias = m.Alias;
            string notes = m.Notes;
            string stage = m.Stage;
            Guid gg = PrivateService.NKD_BM_STAGE;
            BlockModelService.ProcessModelAsync(bmFile, formatSpecFile, projID, alias, this.getCurrentUserID(), notes, stage, gg, 60);
            return View(m);
        }

        [HttpGet]
        public ActionResult EditModel()
        {
            var model = new BlockModelViewModel
            {
                FileNames = BlockModelService.GetFileNameList(),
                FormatFileNames = BlockModelService.GetFormatFileNameList(),
                Projects = ProjectService.GetProjectList()
            };
            model.Stages = ProjectService.GetStagesList(new Guid(model.Projects.First().Value));
            return View(model);
        }

        [HttpGet]
        public ActionResult ReportAssays()
        {
            var model = new AssayReportViewModel
            {
                Report = AllReports.GetReport(AllReports.ReportType.AssayReport),
                Projects = ProjectService.GetProjectList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult ReportAssays(AssayReportViewModel m)
        {
            if (!ModelState.IsValid)
                return ReportAssays();
            m.ReportID = (uint)AllReports.ReportType.AssayReport;
            m.ReportExecutedByUserName = Services.WorkContext.CurrentUser.UserName;
            IReport r = AssayService.ReportAssays(m);
            m.Report = r.Report;
            //m.ReportID = r.ReportID;
            m.ParametersView  = r.ParametersView;
            m.ReportName = r.ReportName;
            m.SerializedChild = r.SerializedChild;
            m.FilterString = r.FilterString;
            return new NKD.Handlers.FileGeneratingResult(string.Format("{0}-{1}-{2}.csv", m.Project, m.ProjectID, DateHelper.NowInOnlineFormat).Trim(), "text/csv", stream => m.Report.ExportToCsv(stream));
        }

        [HttpGet]
        public ActionResult ReportGeophysics()
        {
            var model = new GeophysicsReportViewModel
            {
                Report = AllReports.GetReport(AllReports.ReportType.GeophysicsReport),
                Projects = ProjectService.GetProjectList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult ReportGeophysics(GeophysicsReportViewModel m)
        {
            if (!ModelState.IsValid)
                return ReportGeophysics();
            m.ReportID = (uint)AllReports.ReportType.GeophysicsReport;
            m.ReportExecutedByUserName = Services.WorkContext.CurrentUser.UserName;
            IReport r = GeophysicsService.ReportGeophysics(m);
            m.Report = r.Report;
            //m.ReportID = r.ReportID;
            m.ParametersView = r.ParametersView;
            m.ReportName = r.ReportName;
            m.SerializedChild = r.SerializedChild;
            m.FilterString = r.FilterString;
            return new NKD.Handlers.FileGeneratingResult(string.Format("{0}-{1}-{2}.csv", m.Project, m.ProjectID, DateHelper.NowInOnlineFormat).Trim(), "text/csv", stream => m.Report.ExportToCsv(stream));
        }


        [HttpGet]
        public ActionResult DeleteModel(string id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageProjects, T("Couldn't update the model.")))
                return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(id))
                throw new NotImplementedException();
            var guid = new Guid(id);
            BlockModelService.DeleteModel(guid, getCurrentUserID());
            return RedirectToAction("ModelList");
        }

        [HttpGet]
        public ActionResult Unauthorized()
        {
            return new HttpUnauthorizedResult();
        }

        [HttpGet]
        public ActionResult UnauthorizedRedirect()
        {
            return new RedirectResult("~/Users/Account/AccessDenied?ReturnUrl=" + Url.Encode(Request.Path));            
        }

        [HttpGet]
        public ActionResult ImportModel() {
            var model = new BlockModelViewModel
            {
                FileNames = BlockModelService.GetFileNameList(),
                FormatFileNames = BlockModelService.GetFormatFileNameList(),
                Projects = ProjectService.GetProjectListCurrent()
            };
            model.Stages = model.Projects.Any() ? ProjectService.GetStagesList(new Guid(model.Projects.First().Value)) : new SelectList(new SelectListItem[] { });
            return View("ImportModel", model);
        }

        [HttpGet]
        public ActionResult EditProject(string id = null, string verb = null)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageProjects, T("Couldn't edit the project.")))
                return new HttpUnauthorizedResult();
            //is new
            if (string.IsNullOrEmpty(id))
            {
                var model = new ProjectViewModel
                {
                    User = getCurrentUserID()
                };
                model.Contacts = UserService.GetContactList();
                model.Creator = model.User;
                return View(model);
            }
            //delete
            else if (verb == "delete")
            {
                ProjectService.DeleteProject(new Guid(id), getCurrentUserID());
                return RedirectToAction("ProjectList");
            }
            //edit
            else
            {
                var model = new ProjectViewModel
                {
                    Projects = ProjectService.GetProjectList(),
                    User = UserService.GetContactID(Services.WorkContext.CurrentUser.UserName).Value
                };
                model.Project = new Guid(id);
                model.Stages = ProjectService.GetStagesList(model.Project);
                model.Contacts = UserService.GetContactList();
                model.Creator = model.User;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditProject(ProjectViewModel m)
        {
            //Only process new projects atm
            if (m.Project != default(Guid))
                throw new NotImplementedException();

            //Do save project here
            if (ModelState.IsValid)
            {
                try
                {
                    if (!Services.Authorizer.Authorize(Permissions.ManageProjects, T("Couldn't create project.")))
                        return new HttpUnauthorizedResult();

                    //Validate
                    if (string.IsNullOrEmpty(m.ProjectName))
                        ModelState.AddModelError("ProjectName", T("Project name is required.").ToString());

                    if (ModelState.IsValid)
                    {
                        ProjectService.UpdateProject(m);
                        return RedirectToAction("ProjectList");
                    }

                    return View(m);
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            if (m.Project == default(Guid))
                return EditProject();
            else
                return EditProject(m.Project.ToString());
        }

        public ActionResult GetStages(string projectID)
        {
            return Json(ProjectService.GetStagesList(new Guid(projectID))
                //.Select(x => new { Id = x.Value, Name = x.Text}) 
                , JsonRequestBehavior.AllowGet);
        }


        public ActionResult AppendToModel(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new NotImplementedException();
            var guid = new Guid(id);        
            return View(BlockModelService.GetBlockModelToAppend(guid));
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AppendToModelProcessStart(BlockModelAppendViewModel m)
        {

            // get the currently mapped model columns
            List<string> columnNames = BlockModelService.GetImportFileColumnsAsList(m.BlockModelID, m.FileName, m.BlockModelAlias);
            SelectList sl = new SelectList(columnNames);
            // get the next available column in the database to write update to, requires a search in the meta data for column names
            m.FileColumnNames = sl;
            return View(m);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AppendToModelProcessGo(BlockModelAppendViewModel m)
        {
            // Call into the block model library to append the given column using the column map provided.
            // this will attempt to match the BlockModelID of target model, and XC, YC and ZC  coordinates of every record and insert the value
            List<string> columnNames = BlockModelService.GetImportFileColumnsAsList(m.BlockModelID, m.FileName, m.BlockModelAlias);            
            string columnToAdd = columnNames[3];
            int columnIndexToAdd = 3;
            BlockModelService.AppendModelAsync(m.BlockModelID, m.FileName, m.BlockModelAlias, columnToAdd, columnIndexToAdd, getCurrentUserID(), 60);
            return View(m);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AppendToModelUpload()
        {
            UploadControlExtension.GetUploadedFiles("ucCallbacks", UIHelper.AppendModelValidationSettings, UIHelper.ucCallbacks_AppendComplete);
            return null;
        }

        public ActionResult ModelsToAuthoriseList()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult ModelsToAuthoriseListPartial()
        {
            return PartialView("ModelsToAuthoriseListPartial");
        }

        public ActionResult ModelList()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult ModelListPartial()
        {
            return PartialView("ModelListPartial");
        }


        public ActionResult ProjectList()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult ProjectListPartial()
        {
            return PartialView("ProjectListPartial");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ProjectListNewPartial(Project o)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!Services.Authorizer.Authorize(Permissions.ManageProjects, T("Couldn't create project.")))
                        return new HttpUnauthorizedResult();

                    //Validate
                    if (string.IsNullOrEmpty(o.ProjectName))
                        ModelState.AddModelError("ProjectName", T("Project name is required.").ToString());

                    if (ModelState.IsValid)
                    {
                        ProjectService.CreateProject(o);
                        return PartialView("ProjectListPartial");
                    }

                    return View(o);
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("ProjectListPartial");
        }

        public ActionResult ModelParameters()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult ModelParametersPartial()
        {
            return PartialView("ModelParametersPartial");
        }

        [HttpGet, ValidateInput(false)]
        public ActionResult ModelParametersEdit(string id)
        {
            var model = ParameterService.GetParameter(new Guid(id));
            model.Units = ParameterService.GetUnitsList();
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ModelParametersEdit(BlockModelParameterViewModel m)
        {
            if (!ModelState.IsValid)
                return ModelParametersEdit(string.Format("{0}", m.BlockModelMetadataID));


            try
            {
                if (!Services.Authorizer.Authorize(Permissions.ManageProjects, T("Couldn't update parameter.")))
                    return new HttpUnauthorizedResult();


                //Validate
                if (!m.UnitID.HasValue)
                    ModelState.AddModelError("UnitID", T("Unit is required.").ToString());

                if (ModelState.IsValid)
                {
                    BlockModelService.UpdateModelParameter(m);
                    return RedirectToAction("ModelParameters");
                }

            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }        
            return View(m);
        }

        [HttpGet, ValidateInput(false)]
        public ActionResult AuthoriseModel(string id)
        {
            var model = ParameterService.GetApproval(new Guid(id));
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AuthoriseModel(BlockModelApproveViewModel m, string submit)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    if (!Services.Authorizer.Authorize(Permissions.ManageProjects, T("Couldn't update model.")))
                        return new HttpUnauthorizedResult();

                    if (submit == "Approve")
                    {

                        if (ModelState.IsValid)
                        {
                            var note = string.Format("Model [Name: {0} ID: ({1})] was approved by ({2}).", m.BlockModelAlias, m.BlockModelID, Services.WorkContext.CurrentUser.UserName);
                            BlockModelService.ApproveModel(m.BlockModelID.Value, getCurrentUserID(), note);
                            Logger.Information(note);                            
                        }
                    }
                    else if (submit == "Notify")
                    {
                        var error = string.Format("Model [Name: {0} ID: ({1})] was not approved by ({2}).", m.BlockModelAlias, m.BlockModelID, Services.WorkContext.CurrentUser.UserName);
                        BlockModelService.DenyModel(m.BlockModelID.Value, getCurrentUserID(), error);
                        Logger.Information(error);
                    }
                    return RedirectToAction("ModelsToAuthoriseList");
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return RedirectToAction("AuthoriseModel", new { id = m.BlockModelID });
            
        }

        public ActionResult ModelParametersDelete(string  id)
        {            
            //TODO: Delete parameter
            return RedirectToAction("ModelParameters");
        }

        

        public ActionResult ProjectInfo()
        {
            dynamic packageDisplay = Services.New.ProjectInfo(
                ProjectCount: 10
                );
            return new ShapeResult(this, packageDisplay);
        }

        public ActionResult Index()
        {
            return View();            
        }

        public ActionResult ModelPivot()
        {
            return View("ModelPivot");
        }
        public ActionResult ModelPivotPartial()
        {
            return PartialView("ModelPivotPartial");
        }
        public ActionResult ModelPivotChartPartial()
        {
            return PartialView("ModelPivotChartPartial", PivotGridExtension.GetDataObject(ModelPivotSettings(), OLAP_XSTRING));
        }
       
        public static PivotGridSettings ModelPivotSettings() {
            PivotGridSettings pivotGridSettings = new PivotGridSettings();
            pivotGridSettings.Name = "pivotGrid";
            pivotGridSettings.CallbackRouteValues = new { Area = "NKD", Controller = "User", Action = "ModelPivotPartial" };
            pivotGridSettings.OptionsChartDataSource.DataProvideMode = DevExpress.XtraPivotGrid.PivotChartDataProvideMode.UseCustomSettings;
            pivotGridSettings.OptionsChartDataSource.ProvideDataByColumns = true;
            pivotGridSettings.OptionsView.ShowFilterHeaders = false;
            pivotGridSettings.OptionsView.ShowHorizontalScrollBar = true;
            pivotGridSettings.OptionsView.ShowHorizontalScrollBar = true;
            pivotGridSettings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            pivotGridSettings.CustomizationFieldsLeft = 600;
            pivotGridSettings.CustomizationFieldsTop = 400;
            pivotGridSettings.OLAPDataProvider = DevExpress.XtraPivotGrid.OLAPDataProvider.Adomd;
            pivotGridSettings.OptionsCustomization.CustomizationFormStyle = DevExpress.XtraPivotGrid.Customization.CustomizationFormStyle.Excel2007;


            pivotGridSettings.CustomCallback = (sender, e) =>
            {
                //DevExpress.Web.ASPxPivotGrid.PivotGridField field = ((MVCxPivotGrid)sender).Fields["[Product].[Product Categories].[Product]"];
                //if (field != null)
                //    ((MVCxPivotGrid)sender).Fields.Remove(field);
            };

             pivotGridSettings.PreRender = (sender, e) =>
             {
                 MVCxPivotGrid pivot = ((MVCxPivotGrid)sender);
                 pivot.RetrieveFields( DevExpress.XtraPivotGrid.PivotArea.FilterArea, false);
                 //pivot.BeginUpdate();

                 //pivot.Fields["[Product].[Product Categories].[Category]"].Area = PivotArea.RowArea;
                 //pivot.Fields["[Product].[Product Categories].[Category]"].Visible = true;
                 //pivot.Fields["[Date].[Calendar].[Calendar Year]"].Area = PivotArea.ColumnArea;
                 //pivot.Fields["[Date].[Calendar].[Calendar Year]"].Visible = true;
                 //pivot.Fields["[Date].[Calendar].[Calendar Year]"].ExpandedInFieldsGroup = false;
                 //pivot.Fields["[Measures].[Sales Amount]"].Visible = true;

                 //pivot.EndUpdate();
             };
            //pivotGridSettings.Fields.Add(field => {
            //    field.FieldName = "Extended_Price";
            //    field.Caption = "Extended Price";
            //    field.Area = PivotArea.DataArea;
            //    field.AreaIndex = 0;
            //});
            //pivotGridSettings.Fields.Add(field => {
            //    field.FieldName = "CategoryName";
            //    field.Caption = "Category Name";
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 0;
            //});
            //pivotGridSettings.Fields.Add(field => {
            //    field.FieldName = "OrderDate";
            //    field.Caption = "Order Month";
            //    field.Area = PivotArea.ColumnArea;
            //    field.AreaIndex = 0;
            //    field.UnboundFieldName = "fieldOrderDate";
            //    field.GroupInterval = PivotGroupInterval.DateMonth;
            //});
            //pivotGridSettings.PreRender = (sender, e) => {
            //    int selectNumber = 4;
            //    var field = ((MVCxPivotGrid)sender).Fields["Category Name"];
            //    object[] values = field.GetUniqueValues();
            //    List<object> includedValues = new List<object>(values.Length / selectNumber);
            //    for (int i = 0; i < values.Length; i++) {
            //        if (i % selectNumber == 0)
            //            includedValues.Add(values[i]);
            //    }
            //    field.FilterValues.ValuesIncluded = includedValues.ToArray();
            //};
            pivotGridSettings.ClientSideEvents.BeginCallback = "OnBeforePivotGridCallback";
            pivotGridSettings.ClientSideEvents.EndCallback = "UpdateChart";

            return pivotGridSettings;
        }

        public ActionResult Workflow()
        {
            WorkflowService.AssignResponsibility(Guid.NewGuid(), Guid.NewGuid());
            return View("Workflow");
        }
    }
}










    

