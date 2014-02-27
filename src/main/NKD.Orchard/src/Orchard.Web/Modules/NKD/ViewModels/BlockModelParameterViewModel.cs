using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NKD.Helpers;
using System;
namespace NKD.ViewModels
{
    public class BlockModelParameterViewModel
    {
        [HiddenInput, Required, DisplayName("Block Model Parameter ID:")]
        public Guid? BlockModelMetadataID { get; set; }
        [DisplayName("Block Model:")]
        public string BlockModelAlias { get; set; }
        [DisplayName("Parameter Name:")]
        public string ParameterName { get; set; }
        [DisplayName("Parameter Description:")]
        public string ParameterDescription { get; set; }
        [HiddenInput, Required, DisplayName("Parameter ID:")]
        public Guid? ParameterID { get; set; }
        [Required, DisplayName("Unit:")]
        public Guid? UnitID { get; set; }

        public SelectList Units { get; set; }

    }
}