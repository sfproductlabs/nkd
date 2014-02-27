using System;
using Machine.Specifications;
using Moq;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;
using Orchard.Logging;
using It = Machine.Specifications.It;
using System.Linq;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    [Subject(Subjects.PackageIdExpiration)]
    public class When_userkey_package_does_not_have_a_registered_date : with_a_package_id_expiration_coordinator {
        Establish context = () => mockedUserKeyPackageService.Setup(ukps => ukps.GetPackageIdsThatAreNotPackageParts()).Returns(new[] {new UserkeyPackage()});

        Because of = () => coordinator.ProcessExpirations(30, DateTime.UtcNow);

        It Should_not_process_the_package = () => mockedODataContext.VerifyGet(odc => odc.Packages, Times.Never());
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_userkey_package_is_expired_and_is_not_on_the_feed : with_a_package_id_expiration_coordinator {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage { PackageId = "Package ID", RegisteredUtc = utcNow.AddYears(-1) };

        Establish context = () => mockedUserKeyPackageService.Setup(ukps => ukps.GetPackageIdsThatAreNotPackageParts()).Returns(new[] { userKeyPackage });

        Because of = () => coordinator.ProcessExpirations(30, utcNow);

        It Should_delete_unfinished_packages = () => mockedGalleryPackageService.Verify(upd => upd.DeleteUnfinishedPackages(userKeyPackage.PackageId),
            Times.Once());
        It Should_delete_userkey_package = () => mockedUserKeyPackageRepository.Verify(ukpr => ukpr.Delete(userKeyPackage), Times.Once());
        It Should_log_message = () => mockedLogger.Verify(l => l.Log(LogLevel.Information, null, Moq.It.IsAny<string>(), Moq.It.IsAny<object[]>()), Times.Once());
        It Should_not_notify_user_if_package_id_is_about_to_expire = () => mockedExpiredPackageIdNotifier.Verify(epin => epin.
            NotifyUserIfPackageIdIsAboutToExpire(Moq.It.IsAny<UserkeyPackage>(), Moq.It.IsAny<DateTime>(), Moq.It.IsAny<DateTime>(), Moq.It.IsAny<bool>()),
            Times.Never());
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_userkey_package_is_not_expired_and_not_on_the_feed : with_a_package_id_expiration_coordinator {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage { PackageId = "Package ID", RegisteredUtc = utcNow };
        static readonly DateTime expirationDate = userKeyPackage.RegisteredUtc.Value.AddDays(numberOfDaysUntilPackageIdExpires);
        const int numberOfDaysUntilPackageIdExpires = 30;

        Establish context = () => {
            mockedUserKeyPackageService.Setup(ukps => ukps.GetPackageIdsThatAreNotPackageParts()).Returns(new[] { userKeyPackage });
            mockedODataContext.SetupGet(odc => odc.Packages).Returns(new PublishedPackage[0].AsQueryable());
        };

        Because of = () => coordinator.ProcessExpirations(numberOfDaysUntilPackageIdExpires, utcNow);

        It Should_notify_user_if_package_id_is_about_to_expire = () => mockedExpiredPackageIdNotifier.Verify(epin => epin.
            NotifyUserIfPackageIdIsAboutToExpire(userKeyPackage, utcNow, expirationDate, true), Times.Once());
        It Should_not_delete_unfinished_packages = () => mockedGalleryPackageService.Verify(upd => upd.DeleteUnfinishedPackages(Moq.It.IsAny<string>()),
            Times.Never());
        It Should_not_delete_userkey_package = () => mockedUserKeyPackageRepository.Verify(ukpr => ukpr.Delete(Moq.It.IsAny<UserkeyPackage>()), Times.Never());
        It Should_not_log_message = () => mockedLogger.Verify(l => l.Log(Moq.It.IsAny<LogLevel>(), Moq.It.IsAny<Exception>(),
            Moq.It.IsAny<string>(), Moq.It.IsAny<object[]>()), Times.Never());
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_userkey_package_is_expired_and_on_the_feed : with_a_package_id_expiration_coordinator
    {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage { PackageId = "Package ID", RegisteredUtc = utcNow.AddYears(-1) };
        static readonly DateTime expirationDate = userKeyPackage.RegisteredUtc.Value.AddDays(numberOfDaysUntilPackageIdExpires);
        const int numberOfDaysUntilPackageIdExpires = 30;

        Establish context = () =>
        {
            mockedUserKeyPackageService.Setup(ukps => ukps.GetPackageIdsThatAreNotPackageParts()).Returns(new[] { userKeyPackage });
            mockedODataContext.SetupGet(odc => odc.Packages).Returns(new[] { new PublishedPackage { Id = userKeyPackage.PackageId}}.AsQueryable());
        };

        Because of = () => coordinator.ProcessExpirations(numberOfDaysUntilPackageIdExpires, utcNow);

        It Should_notify_user_if_package_id_is_about_to_expire = () => mockedExpiredPackageIdNotifier.Verify(epin => epin.
            NotifyUserIfPackageIdIsAboutToExpire(userKeyPackage, utcNow, expirationDate, false), Times.Once());
        It Should_not_delete_unfinished_packages = () => mockedGalleryPackageService.Verify(upd => upd.DeleteUnfinishedPackages(Moq.It.IsAny<string>()),
            Times.Never());
        It Should_not_delete_userkey_package = () => mockedUserKeyPackageRepository.Verify(ukpr => ukpr.Delete(Moq.It.IsAny<UserkeyPackage>()), Times.Never());
        It Should_not_log_message = () => mockedLogger.Verify(l => l.Log(Moq.It.IsAny<LogLevel>(), Moq.It.IsAny<Exception>(),
            Moq.It.IsAny<string>(), Moq.It.IsAny<object[]>()), Times.Never());
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_userkey_package_is_not_expired_and_on_the_feed : with_a_package_id_expiration_coordinator
    {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage { PackageId = "Package ID", RegisteredUtc = utcNow.AddYears(-1) };
        static readonly DateTime expirationDate = userKeyPackage.RegisteredUtc.Value.AddDays(numberOfDaysUntilPackageIdExpires);
        const int numberOfDaysUntilPackageIdExpires = 30;

        Establish context = () =>
        {
            mockedUserKeyPackageService.Setup(ukps => ukps.GetPackageIdsThatAreNotPackageParts()).Returns(new[] { userKeyPackage });
            mockedODataContext.SetupGet(odc => odc.Packages).Returns(new[] { new PublishedPackage { Id = userKeyPackage.PackageId } }.AsQueryable());
        };

        Because of = () => coordinator.ProcessExpirations(numberOfDaysUntilPackageIdExpires, utcNow);

        It Should_notify_user_if_package_id_is_about_to_expire = () => mockedExpiredPackageIdNotifier.Verify(epin => epin.
            NotifyUserIfPackageIdIsAboutToExpire(userKeyPackage, utcNow, expirationDate, false), Times.Once());
        It Should_not_delete_unfinished_packages = () => mockedGalleryPackageService.Verify(upd => upd.DeleteUnfinishedPackages(Moq.It.IsAny<string>()),
            Times.Never());
        It Should_not_delete_userkey_package = () => mockedUserKeyPackageRepository.Verify(ukpr => ukpr.Delete(Moq.It.IsAny<UserkeyPackage>()), Times.Never());
        It Should_not_log_message = () => mockedLogger.Verify(l => l.Log(Moq.It.IsAny<LogLevel>(), Moq.It.IsAny<Exception>(),
            Moq.It.IsAny<string>(), Moq.It.IsAny<object[]>()), Times.Never());
    }
}