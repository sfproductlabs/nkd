using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using It = Machine.Specifications.It;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    [Subject(Subjects.PackageIdExpirationNotification)]
    public class When_user_of_userkey_does_not_exist : with_expired_package_id_messenger {
        Establish context = () => mockedUserKeyService.Setup(uks => uks.GetUserForUserKey(userKeyPackage.UserkeyId)).Returns((UserPart)null);

        Because of = () => expiredPackageIdMessenger.SendMessage(userKeyPackage, DateTime.Now);

        It Should_not_send_a_message = () => mockedMessageManager.Verify(mm => mm.Send(Moq.It.IsAny<ContentItemRecord>(),
            Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<Dictionary<string, string>>()),
            Times.Never());
    }

    [Subject(Subjects.PackageIdExpirationNotification)]
    public class When_user_of_userkey_exists : with_expired_package_id_messenger {
        static readonly UserPart userPart = new UserPart { ContentItem = new ContentItem(), Record = new UserPartRecord(), UserName = "foo" };
        static DateTime expirationDate = DateTime.Now;

        Establish context = () => mockedUserKeyService.Setup(uks => uks.GetUserForUserKey(userKeyPackage.UserkeyId)).Returns(userPart);

        Because of = () => expiredPackageIdMessenger.SendMessage(userKeyPackage, expirationDate);

        It Should_send_an_expiration_warning_email_message = () => mockedMessageManager.Verify(mm => mm.Send(Moq.It.IsAny<ContentItemRecord>(),
            GalleryMessageTypes.PackageIdExpirationWarning, "email", Moq.It.IsAny<Dictionary<string, string>>()),
            Times.Once());
        It Should_include_username_in_message = () => VerifyMessengerSendsWithProperty("UserName", userPart.UserName);
        It Should_include_sitename_in_message = () => VerifyMessengerSendsWithProperty("SiteName", siteName);
        It Should_include_package_id_in_message = () => VerifyMessengerSendsWithProperty("PackageId", userKeyPackage.PackageId);
        It Should_include_expiration_date_in_message = () => VerifyMessengerSendsWithProperty("ExpirationDate", expirationDate.ToShortDateString());

        static void VerifyMessengerSendsWithProperty(string propertyKey, string expectedValue) {
            mockedMessageManager.Verify(mm => mm.Send(Moq.It.IsAny<ContentItemRecord>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(),
                Moq.It.Is<Dictionary<string, string>>(d => d[propertyKey] == expectedValue)), Times.Once());
        }
    }
}