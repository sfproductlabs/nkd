using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.PackageSynchronization;
using Orchard.Indexing;

namespace Orchard.Gallery.UnitTests.PackageSynchronization.PackageSynchronizer
{
    [TestFixture]
    public class SynchronizeTester
    {
        private IPackageSynchronizer _packageSynchronizer;

        private Mock<IPackagePartCreator> _mockedPackagePartCreator;
        private Mock<IPackagePartUpdater> _mockedPackagePartUpdater;
        private Mock<IPackagePartDeleter> _mockedPackagePartDeleter;
        private Mock<IPackageLogEntryService> _mockedPackageLogEntryService;
        private Mock<IPackagePartPublishingService> _mockedPackagePartUnpublisher;

        private GallerySettingsPart _gallerySettingsPart;

        [SetUp]
        public void SetUp()
        {
            _mockedPackagePartCreator = new Mock<IPackagePartCreator>();
            _mockedPackagePartUpdater = new Mock<IPackagePartUpdater>();
            _mockedPackagePartDeleter = new Mock<IPackagePartDeleter>();
            _mockedPackageLogEntryService = new Mock<IPackageLogEntryService>();
            _mockedPackagePartUnpublisher = new Mock<IPackagePartPublishingService>();
            var mockedOrchardServices = new Mock<IOrchardServices>();
            var mockedTypeCaster = new Mock<ITypeCaster>();

            _packageSynchronizer = new Gallery.PackageSynchronization.PackageSynchronizer(_mockedPackagePartCreator.Object, _mockedPackagePartUpdater.Object,
                _mockedPackagePartDeleter.Object, _mockedPackageLogEntryService.Object, mockedOrchardServices.Object, mockedTypeCaster.Object,
                new Mock<IIndexNotifierHandler>().Object, _mockedPackagePartUnpublisher.Object);

            mockedOrchardServices.SetupGet(os => os.WorkContext).Returns(new Mock<WorkContext>().Object);
            _gallerySettingsPart = new GallerySettingsPart { Record = new GallerySettingsPartRecord() };
            mockedTypeCaster.Setup(tc => tc.CastTo<GallerySettingsPart>(It.IsAny<IContent>())).Returns(_gallerySettingsPart);
        }

        [Test]
        public void ShouldFetchUnprocessedLogEntries()
        {
            _mockedPackageLogEntryService.Setup(ples => ples.GetUnprocessedLogEntries()).Returns(new List<PackageLogEntry>());

            _packageSynchronizer.Synchronize();

            _mockedPackageLogEntryService.Verify(ples => ples.GetUnprocessedLogEntries(), Times.Once(), "Unprocessed log entries were not fetched.");
        }

        [Test]
        public void ShouldCreatePackagePartForCreateLogAction()
        {
            PackageLogEntry entryToProcess = new PackageLogEntry { Action = PackageLogAction.Create };
            _mockedPackageLogEntryService.Setup(ples => ples.GetUnprocessedLogEntries()).Returns(new[] { entryToProcess });

            _packageSynchronizer.Synchronize();

            _mockedPackagePartCreator.Verify(ppc => ppc.CreateNewPackagePart(entryToProcess), Times.Once(), "PackagePart should have been created.");
        }

        [TestCase(PackageLogAction.Update)]
        [TestCase(PackageLogAction.RePublish)]
        public void ShouldUpdatePackagePartAndExtendedInfoForUpdateOrRePublishLogAction(PackageLogAction packageLogAction)
        {
            PackageLogEntry entryToProcess = new PackageLogEntry { Action = packageLogAction };
            _mockedPackageLogEntryService.Setup(ples => ples.GetUnprocessedLogEntries()).Returns(new[] { entryToProcess });

            _packageSynchronizer.Synchronize();

            _mockedPackagePartUpdater.Verify(ppu => ppu.ModifyExistingPackagePart(entryToProcess, true), Times.Once(),
                "PackagePart and extended info should have been updated.");
        }

        [Test]
        public void ShouldDeletePackagePartForDeleteLogAction()
        {
            PackageLogEntry entryToProcess = new PackageLogEntry { Action = PackageLogAction.Delete };
            _mockedPackageLogEntryService.Setup(ples => ples.GetUnprocessedLogEntries()).Returns(new[] { entryToProcess });

            _packageSynchronizer.Synchronize();

            _mockedPackagePartDeleter.Verify(ppd => ppd.DeletePackage(entryToProcess), Times.Once(), "PackagePart should have been deleted.");
        }

        [Test]
        public void ShouldUpdatePackagePartOnlyForDownloadLogAction()
        {
            PackageLogEntry entryToProcess = new PackageLogEntry { Action = PackageLogAction.Download };
            _mockedPackageLogEntryService.Setup(ples => ples.GetUnprocessedLogEntries()).Returns(new[] { entryToProcess });

            _packageSynchronizer.Synchronize();

// ReSharper disable RedundantArgumentDefaultValue
            _mockedPackagePartUpdater.Verify(ppu => ppu.ModifyExistingPackagePart(entryToProcess, false), Times.Once(),
                "PackagePart should have been updated and extended info should not have been updated.");
// ReSharper restore RedundantArgumentDefaultValue
        }

        [Test]
        public void ShouldUnpublishPackagePartForUnpublishLogAction() {
            PackageLogEntry log = new PackageLogEntry {Action = PackageLogAction.Unpublish};
            _mockedPackageLogEntryService.Setup(ples => ples.GetUnprocessedLogEntries()).Returns(new[] { log });

            _packageSynchronizer.Synchronize();

            _mockedPackagePartUnpublisher.Verify(ppu => ppu.Unpublish(log.PackageId, log.PackageVersion), Times.Once());
        }

        [Test]
        public void LastPackageLogIdOfSettingsPartShouldBeSetToLastProcessedLogId()
        {
            PackageLogEntry entry = new PackageLogEntry { Action = PackageLogAction.Download };
            PackageLogEntry entryWithExpectedId = new PackageLogEntry { Id=45, Action = PackageLogAction.Download };
            _mockedPackageLogEntryService.Setup(ples => ples.GetUnprocessedLogEntries()).Returns(new[] { entry, entry, entryWithExpectedId });

            _packageSynchronizer.Synchronize();

            Assert.AreEqual(entryWithExpectedId.Id, _gallerySettingsPart.LastPackageLogId, "LastPackageLogId was not updated correctly.");
        }
    }
}