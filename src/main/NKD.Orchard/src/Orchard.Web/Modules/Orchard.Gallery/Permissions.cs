using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Gallery {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManagePackages = new Permission { Description = "Manage Package", Name = "ManagePackages" };
        public static readonly Permission ManageOwnPackages = new Permission { Description = "Manage Own Packages", Name = "ManageOwnPackages", ImpliedBy = new[] { ManagePackages }};

        public Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManagePackages,
                ManageOwnPackages,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManagePackages }
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] { ManageOwnPackages }
                    
                }
            };
        }
    }
}