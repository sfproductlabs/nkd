using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Mvc.Routes;

namespace Orchard.Gallery
{
   [UsedImplicitly]
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            const string areaName = "Orchard.Gallery";
            var emptyConstraints = new RouteValueDictionary();
            var galleryRouteValueDictionary = new RouteValueDictionary {{"area", areaName}};
            var mvcRouteHandler = new MvcRouteHandler();

            return new[] {
                new RouteDescriptor {
                  Route = new Route (
                      "Contribute/ManagePackageIds",
                      new RouteValueDictionary {
                          {"area", areaName},
                          {"controller", "ManagePackageIds"},
                          {"action", "Index"}
                      },
                      emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler)
                },
                new RouteDescriptor {
                    Route = new Route (
                        "Ratings/AuthorizeUpdate/{nonce}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "RatingsUpdateAuthorization"},
                            {"action", "AuthorizeUpdate"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "Package/ReportAbuse/{packageId}/{packageVersion}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "ReportAbuse"},
                            {"action", "Index"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "Package/ContactOwners/{packageId}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "ContactOwners"},
                            {"action", "Index"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "ManagePackageOwners/{packageId}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "ManagePackageOwners"},
                            {"action", "Index"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "UploadPackageLogoAndScreenshots/{action}/{packageId}/{packageVersion}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "UploadPackageLogoAndScreenshots"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "Contribute/NewSubmission",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "UploadPackage"},
                            {"action", "Index"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "Contribute/{action}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Contribute"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "List/ByAuthor/{authorName}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Packages"},
                            {"action", "ByAuthor"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "List/ByCategory/{packageType}/{categoryName}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Packages"},
                            {"action", "ByCategory"},
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "List/{packageType}/{packageId}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Package"},
                            {"action", "DetailsForId"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "List/{packageType}/{packageId}/{packageVersion}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Package"},
                            {"action", "DetailsForIdAndVersion"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "Package/{action}/{packageId}/{packageVersion}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Package"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "Package/Delete",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Package"},
                            {"action", "Delete"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor{
                    Route = new Route (
                        "PackageAuthentication/{action}/{key}/{packageId}/{packageVersion}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "PackageAuthentication"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor{
                    Route = new Route (
                        "PackageAuthentication/{action}/{key}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "PackageAuthentication"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "List/Search",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Packages"},
                            {"action", "Search"},
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "List/{packageType}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "Packages"},
                            {"action", "List"},
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "PackageCategories/{packageType}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "PackageCategories"},
                            {"action", "Categories"}
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
                new RouteDescriptor {
                    Route = new Route (
                        "SearchResults",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "PackageSearch"},
                            {"action", "Index"},
                        },
                        emptyConstraints, galleryRouteValueDictionary, mvcRouteHandler
                    )
                },
            };
        }
    }
}