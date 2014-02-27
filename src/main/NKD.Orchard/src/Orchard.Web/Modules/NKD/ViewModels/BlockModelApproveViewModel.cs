using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NKD.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using NKD.Models;

namespace NKD.ViewModels
{
    public class BlockModelApproveViewModel
    {
        [HiddenInput, Required, DisplayName("Block Model ID:")]
        public Guid? BlockModelID { get; set; }
        [DisplayName("Block Model:")]
        public string BlockModelAlias { get; set; }
        [DisplayName("Parameters:")]
        public Dictionary<Guid, Tuple<string, string>> Parameters { get; set; }
        [DisplayName("Updates:")]
        public List<Occurrence> Updates { get; set; }        
    }
}