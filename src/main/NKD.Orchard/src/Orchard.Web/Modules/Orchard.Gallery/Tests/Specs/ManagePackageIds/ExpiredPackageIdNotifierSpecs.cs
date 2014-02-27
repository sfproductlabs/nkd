using System;
using Machine.Specifications;
using Moq;
using Orchard.Gallery.Models;
using It = Machine.Specifications.It;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    [Subject(Subjects.PackageIdExpirationNotification)]
    public class When_site_is_not_configured_to_send_warnings : with_expired_package_id_notifier {
        Establish context = () => gallerySettingsPart.DaysInAdvanceToWarnUserOfExpiration = null;

        Because of = () => notifier.NotifyUserIfPackageIdIsAboutToExpire(null, DateTime.UtcNow, DateTime.UtcNow, false);

        private It Should_not_send_message = () => mockedExpiredPackageIdMessenger.Verify(epim => epim.
            SendMessage(Moq.It.IsAny<UserkeyPackage>(), Moq.It.IsAny<DateTime>()), Times.Never());
    }

    [Subject(Subjects.PackageIdExpirationNotification)]
    public class When_package_id_is_at_warning_date_and_is_not_on_feed : with_expired_package_id_notifier {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage();
        static readonly DateTime expirationDate = utcNow.AddDays(daysInAdvanceToWarnUserOfExpiration);
        const int daysInAdvanceToWarnUserOfExpiration = 5;

        Establish context = () => gallerySettingsPart.DaysInAdvanceToWarnUserOfExpiration = daysInAdvanceToWarnUserOfExpiration;

        Because of = () => notifier.NotifyUserIfPackageIdIsAboutToExpire(userKeyPackage, utcNow, expirationDate, true);

        It Should_send_warning_message_to_user = () => mockedExpiredPackageIdMessenger.Verify(epim => epim.SendMessage(userKeyPackage, expirationDate),
            Times.Once());
    }

    [Subject(Subjects.PackageIdExpirationNotification)]
    public class When_package_id_is_at_warning_date_and_is_on_feed : with_expired_package_id_notifier {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage();
        static readonly DateTime expirationDate = utcNow.AddDays(daysInAdvanceToWarnUserOfExpiration);
        const int daysInAdvanceToWarnUserOfExpiration = 5;

        Establish context = () => gallerySettingsPart.DaysInAdvanceToWarnUserOfExpiration = daysInAdvanceToWarnUserOfExpiration;

        Because of = () => notifier.NotifyUserIfPackageIdIsAboutToExpire(userKeyPackage, utcNow, expirationDate, false);

        It Should_not_send_warning_message_to_user = () => mockedExpiredPackageIdMessenger.Verify(epim => epim.SendMessage(userKeyPackage, expirationDate),
            Times.Never());
    }

    [Subject(Subjects.PackageIdExpirationNotification)]
    public class When_package_id_is_not_at_warning_date_and_is_not_on_feed : with_expired_package_id_notifier
    {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage();
        static readonly DateTime expirationDate = utcNow.AddDays(daysInAdvanceToWarnUserOfExpiration + 10);
        const int daysInAdvanceToWarnUserOfExpiration = 5;

        Establish context = () => gallerySettingsPart.DaysInAdvanceToWarnUserOfExpiration = daysInAdvanceToWarnUserOfExpiration;

        Because of = () => notifier.NotifyUserIfPackageIdIsAboutToExpire(userKeyPackage, utcNow, expirationDate, true);

        It Should_not_send_warning_message_to_user = () => mockedExpiredPackageIdMessenger.Verify(epim => epim.SendMessage(userKeyPackage, expirationDate),
            Times.Never());
    }

    [Subject(Subjects.PackageIdExpirationNotification)]
    public class When_package_id_is_not_at_warning_date_and_is_on_feed : with_expired_package_id_notifier
    {
        static readonly DateTime utcNow = DateTime.UtcNow;
        static readonly UserkeyPackage userKeyPackage = new UserkeyPackage();
        static readonly DateTime expirationDate = utcNow.AddDays(daysInAdvanceToWarnUserOfExpiration - 10);
        const int daysInAdvanceToWarnUserOfExpiration = 5;

        Establish context = () => gallerySettingsPart.DaysInAdvanceToWarnUserOfExpiration = daysInAdvanceToWarnUserOfExpiration;

        Because of = () => notifier.NotifyUserIfPackageIdIsAboutToExpire(userKeyPackage, utcNow, expirationDate, false);

        It Should_not_send_warning_message_to_user = () => mockedExpiredPackageIdMessenger.Verify(epim => epim.SendMessage(userKeyPackage, expirationDate),
            Times.Never());
    }
}