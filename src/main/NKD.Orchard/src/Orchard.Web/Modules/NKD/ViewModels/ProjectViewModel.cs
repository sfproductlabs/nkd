using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NKD.Helpers;
using System;

namespace NKD.ViewModels
{
    public class ProjectViewModel
    {
        [Required, DisplayName("Current User ID:")]
        public Guid User { get; set; }
        [Required, DisplayName("Creator:")]
        public Guid Creator { get; set; }
        [Required, DisplayName("Project:")]
        public Guid Project { get; set; }
        [Required, DisplayName("Project Name:")]
        public string ProjectName { get; set; }
        [Required, DisplayName("Stage:")]
        public Guid Stage { get; set; }
        [Required, DisplayName("Stage Name:")]
        public string StageName { get; set; }
        [Required, DisplayName("Designated Reviewer:")]
        public Guid Reviewer { get; set; }
        [DisplayName("Comments:")]
        public string Comment { get; set; }

        public SelectList Projects { get; set; }
        public SelectList Stages { get; set; }
        public SelectList Contacts { get; set; }
        public SelectList Creators { get { return Contacts; } }
        public SelectList Reviewers { get { return Contacts; } }

    }
}