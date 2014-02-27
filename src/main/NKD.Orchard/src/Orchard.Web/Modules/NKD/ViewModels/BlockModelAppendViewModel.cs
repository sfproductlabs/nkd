using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NKD.Helpers;
using System;

namespace NKD.ViewModels
{
    public class BlockModelAppendViewModel
    {
        [Required, DisplayName("Block Model ID:")]
        public Guid BlockModelID { get; set; }
        [DisplayName("Block Model Alias:")]
        public string BlockModelAlias { get; set; }
        [DisplayName("Version:")]
        public int? Version { get; set; }
        [DisplayName("File Name:")]
        public string FileName { get; set; }
        [DisplayName("Files on server:")]
        public SelectList FileNames { get; set; }

        [DisplayName("Append column:")]
        public string ColumnName { get; set; }
        [DisplayName("Columns in file:")]
        public SelectList FileColumnNames { get; set; }

    }
}