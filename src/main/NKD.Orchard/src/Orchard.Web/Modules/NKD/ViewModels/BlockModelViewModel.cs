using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NKD.Helpers;

namespace NKD.ViewModels
{
    public class BlockModelViewModel
    {
        [Required, DisplayName("Project Name:")]
        public string Project { get; set; }
        [Required, DisplayName("Block Model Stage:")]
        public string Stage { get; set; }
        [Required, DisplayName("Block Model Alias:")]
        public string Alias { get; set; }
        [Required, DisplayName("Available File Names:")]
        public string FileName { get; set; }

        [DisplayName("Model Format:")]
        public string FormatFileName { get; set; }

        [DisplayName("Notes:")]
        public string Notes { get; set; }
        [DisplayName("Extra notes:")]
        public string ExtraNotes { get; set; }

        
        [DisplayName("X origin:")]
        public double XOrigin { get; set; }
        [DisplayName("Y origin:")]
        public double YOrigin { get; set; }
        [DisplayName("Z origin:")]
        public double ZOrigin { get; set; }


        public SelectList FileNames { get; set; }
        public SelectList FormatFileNames { get; set; }
        public SelectList Projects { get; set; }
        public SelectList Stages { get; set; }

    }
}