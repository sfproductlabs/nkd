using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NKD.Helpers;
using System;

namespace NKD.ViewModels
{
    public class BlockModelUploadViewModel
    {
        [Required, DisplayName("Model 1:")]
        public Guid Model1 { get; set; }
        [Required, DisplayName("Model 2:")]
        public Guid Model2 { get; set; }
        [Required, DisplayName("Test:")]
        public string Test { get; set; }

        [Required, DisplayName("Project:")]
        public Guid Project { get; set; }


        public SelectList Models { get; set; }
        public SelectList ProjectList { get; set; }

    }
}