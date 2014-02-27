using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Machine.Specifications;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;
using It = Machine.Specifications.It;
using System.Linq;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    [Subject(Subjects.ManagePackageIds)]
    public class When_current_user_has_no_access_key : with_package_id_in_use_checker {
        static Exception thrownException;

        Establish context = () => MockedUserkeyService.Setup(uks => uks.GetAccessKeyForUser(Moq.It.IsAny<int>(), Moq.It.IsAny<bool>())).Returns((Userkey)null);

        Because of = () => thrownException = Catch.Exception(() => Checker.IsPackageIdInUse(PackageId));

        It Should_throw_exception = () => thrownException.ShouldBeOfType(typeof (UserDoesNotHaveAccessToPackageException));
    }

    [Subject(Subjects.ManagePackageIds)]
    public class When_package_Id_exists_in_Orchard : with_package_id_in_use_checker {
        Establish context = () => MockedPackageService.Setup(ps => ps.PackageIdExists(PackageId)).Returns(true);

        It Should_return_true = () => Checker.IsPackageIdInUse(PackageId).ShouldBeTrue();
    }

    [Subject(Subjects.ManagePackageIds)]
    public class When_package_Id_exists_in_feed : with_package_id_in_use_checker {
        Establish context = () => MockedODataContext.SetupGet(odc => odc.Packages).Returns(new[] { new PublishedPackage { Id = PackageId } }.AsQueryable());

        It Should_return_true = () => Checker.IsPackageIdInUse(PackageId).ShouldBeTrue();
    }

    [Subject(Subjects.ManagePackageIds)]
    public class When_package_Id_is_used_on_an_unfinished_submission : with_package_id_in_use_checker {
        Establish context = () => MockedGalleryPackageService.Setup(gps => gps
            .GetUnfinishedPackages(Moq.It.IsAny<IEnumerable<string>>(), Moq.It.IsAny<Guid>())).Returns(new[] { new Package { Id = PackageId } });

        It Should_return_true = () => Checker.IsPackageIdInUse(PackageId).ShouldBeTrue();
    }

    [Subject(Subjects.ManagePackageIds)]
    public class When_package_Id_is_not_used_anywhere : with_package_id_in_use_checker {
        It Should_be_false = () => Checker.IsPackageIdInUse(PackageId).ShouldBeFalse();
    }
}