using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace NKD {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageProjects = new Permission { Description = "Managing Projects", Name = "ManageProjects" };
        public static readonly Permission ManageOwnProjects = new Permission { Description = "Manage Own Projects", Name = "ManageOwnProjects", ImpliedBy = new[] { ManageProjects } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageProjects,
                ManageOwnProjects
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageProjects}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageProjects}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {ManageOwnProjects}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

    }
}