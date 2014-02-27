using System;
using Machine.Specifications;
using Moq;
using Orchard.Logging;
using It = Machine.Specifications.It;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    [Subject(Subjects.PackageIdExpiration)]
    public class When_task_has_never_been_run_before : with_expired_package_id_task_runner {
        Because of = () => TaskRunner.Synchronize();

        It Should_set_last_package_expiration_check_time_to_2AM_today = () => gallerySettingsPart.LastPackageIdExpirationCheckTime
            .ShouldEqual(utcNow.Date.AddHours(2));
        It Should_not_try_to_expire_package_ids = () => mockedExpiredPackageIdDeleter
            .Verify(epid => epid.ProcessExpirations(Moq.It.IsAny<int>(), Moq.It.IsAny<DateTime>()), Times.Never());
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_it_has_already_run_today : with_expired_package_id_task_runner {
        private Establish context = () => {
            gallerySettingsPart.LastPackageIdExpirationCheckTime = utcNow.Date.AddHours(10);
            gallerySettingsPart.NumberOfDaysUntilPreregisteredPackageIdExpires = 15;
        };

        Because of = () => TaskRunner.Synchronize();

        It Should_not_try_to_expire_package_ids = () => mockedExpiredPackageIdDeleter
            .Verify(epid => epid.ProcessExpirations(Moq.It.IsAny<int>(), Moq.It.IsAny<DateTime>()), Times.Never());
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_expiration_is_not_enabled : with_expired_package_id_task_runner {
        private Establish context = () => {
            gallerySettingsPart.LastPackageIdExpirationCheckTime = utcNow.AddDays(-1);
            gallerySettingsPart.NumberOfDaysUntilPreregisteredPackageIdExpires = null;
        };

        Because of = () => TaskRunner.Synchronize();

        It Should_not_try_to_expire_package_ids = () => mockedExpiredPackageIdDeleter
            .Verify(epid => epid.ProcessExpirations(Moq.It.IsAny<int>(), Moq.It.IsAny<DateTime>()), Times.Never());
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_it_has_not_run_for_today_and_expiration_is_enabled : with_expired_package_id_task_runner {
        private Establish context = () =>
        {
            gallerySettingsPart.LastPackageIdExpirationCheckTime = utcNow.AddDays(-1);
            gallerySettingsPart.NumberOfDaysUntilPreregisteredPackageIdExpires = 16;
        };

        Because of = () => TaskRunner.Synchronize();

        It Should_try_to_expire_package_ids = () => mockedExpiredPackageIdDeleter
            .Verify(epid => epid.ProcessExpirations(gallerySettingsPart.NumberOfDaysUntilPreregisteredPackageIdExpires.Value, utcNow), Times.Once());
        It Should_set_last_package_expiration_check_time_to_now = () => gallerySettingsPart.LastPackageIdExpirationCheckTime.Value.ShouldEqual(utcNow);
    }

    [Subject(Subjects.PackageIdExpiration)]
    public class When_deleter_throws_an_exception : with_expired_package_id_task_runner {
        static Exception caughtException;
        static readonly Exception thrownException = new Exception();

        Establish context = () => {
            gallerySettingsPart.LastPackageIdExpirationCheckTime = utcNow.AddDays(-1);
            gallerySettingsPart.NumberOfDaysUntilPreregisteredPackageIdExpires = 16;
            mockedExpiredPackageIdDeleter
                .Setup(epid => epid.ProcessExpirations(Moq.It.IsAny<int>(), Moq.It.IsAny<DateTime>())).Throws(thrownException);
        };

        Because of = () => caughtException = Catch.Exception(() => TaskRunner.Synchronize());

        It Should_log_the_exception = () => mockedLogger
            .Verify(l => l.Log(LogLevel.Error, thrownException, Moq.It.IsAny<string>(), Moq.It.IsAny<object[]>()),
            Times.Once());
        It Should_not_update_the_last_package_expiration_check_time = () => gallerySettingsPart.LastPackageIdExpirationCheckTime.ShouldNotEqual(utcNow);
        It Should_not_rethrow = () => caughtException.ShouldBeNull();
    }
}