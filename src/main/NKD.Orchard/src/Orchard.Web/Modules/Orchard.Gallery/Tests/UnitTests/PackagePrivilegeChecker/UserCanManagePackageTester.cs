using Moq;
using NUnit.Framework;
using Orchard.Gallery.Interfaces;
using Orchard.Security;

namespace Orchard.Gallery.UnitTests.PackagePrivilegeChecker
{
    [TestFixture]
    public class UserCanManagePackageTester
    {
        private IPackagePrivilegeChecker _packagePrivilegeChecker;

        private Mock<IAdminPackagePrivilegeChecker> _mockedAdminPackagePrivilegeChecker;
        private Mock<IUserkeyPackageService> _mockedUserkeyPackageService;

        [SetUp]
        public void SetUp()
        {
            _mockedAdminPackagePrivilegeChecker = new Mock<IAdminPackagePrivilegeChecker>();
            _mockedUserkeyPackageService = new Mock<IUserkeyPackageService>();
            _packagePrivilegeChecker = new Impl.PackagePrivilegeChecker(_mockedUserkeyPackageService.Object,
                _mockedAdminPackagePrivilegeChecker.Object);
        }

        [Test]
        public void ShouldReturnFalseWhenGivenNullUser()
        {
            IUser nullUser = null;

            bool userCanManagePackage = _packagePrivilegeChecker.UserCanManagePackage(nullUser, "packageId");

            Assert.IsFalse(userCanManagePackage, "Null user should not be able to manage any Packages.");
        }

        [Test]
        public void ShouldReturnTrueWhenGivenUserCanManageAllPackages()
        {
            IUser user = new Mock<IUser>().Object;
            _mockedAdminPackagePrivilegeChecker.Setup(appc => appc.UserCanManageAllPackages(user)).Returns(true);

            bool userCanManagePackage = _packagePrivilegeChecker.UserCanManagePackage(user, "packageId");

            Assert.IsTrue(userCanManagePackage, "Admins should be able to manage all Packages.");
        }

        [Test]
        public void ShouldReturnTrueWhenUserCanAccessGivenPackageId()
        {
            const string packageId = "packageId";
            IUser user = new Mock<IUser>().Object;
            _mockedAdminPackagePrivilegeChecker.Setup(appc => appc.UserCanManageAllPackages(user)).Returns(false);
            _mockedUserkeyPackageService.Setup(ups => ups.UserCanAccessPackage(packageId, user.Id)).Returns(true);

            bool userCanManagePackage = _packagePrivilegeChecker.UserCanManagePackage(user, packageId);

            Assert.IsTrue(userCanManagePackage, "User with access to given PackageId should be able to manage associated Package.");
        }

        [Test]
        public void ShouldReturnFalseWhenUserIsNotAdminAndCannotManageGivenPackageId()
        {
            const string packageId = "packageId";
            IUser user = new Mock<IUser>().Object;
            _mockedAdminPackagePrivilegeChecker.Setup(appc => appc.UserCanManageAllPackages(user)).Returns(false);
            _mockedUserkeyPackageService.Setup(ups => ups.UserCanAccessPackage(packageId, user.Id)).Returns(false);

            bool userCanManagePackage = _packagePrivilegeChecker.UserCanManagePackage(user, packageId);

            Assert.IsFalse(userCanManagePackage, "Non-admin user with no access to given PackageId should not be able to manage associated Package.");
        }
    }
}