using System;
using System.Data;
using JetBrains.Annotations;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Gallery.Models;

namespace Orchard.Gallery {
    [UsedImplicitly]
    public class Migrations : DataMigrationImpl {
        private readonly IOrchardServices _services;

        public Migrations(IOrchardServices services) {
            _services = services;
        }

        [UsedImplicitly]
        public int Create() {
            const string packagePartRecordTableName = "PackagePartRecord";
            const string userTableName = "UserPartRecord";
            const string userkeyTableName = "Userkey";
            const string userkeyPackageTableName = "UserkeyPackage";

            SchemaBuilder.CreateTable(packagePartRecordTableName, table => table
                .ContentPartRecord()
                .Column<string>("PackageId", col => col.NotNull())
                .Column<string>("PackageVersion", col => col.NotNull())
                .Column<string>("Description", col => col.Unlimited())
                .Column<string>("Summary", col => col.Unlimited())
                .Column<string>("Authors")
                .Column<int>("DownloadCount")
                .Column<string>("Copyright")
                .Column<string>("ProjectUrl")
                .Column<string>("LicenseUrl")
                .Column<string>("IconUrl")
                .Column<string>("DownloadUrl")
                .Column<string>("PackageHashAlgorithm")
                .Column<string>("PackageHash")
                .Column<long>("PackageSize")
                .Column<double>("RatingAverage")
                .Column<int>("RatingsCount")
                .Column<string>("ExternalPackageUrl")
                .Column<string>("ReportAbuseUrl")
                .Column<decimal>("Price")
                .Column<DateTime>("Published", col => col.Nullable())
                .Column<DateTime>("LastUpdated", col => col.Nullable())
            );

            //TODO:CHECK
            ContentDefinitionManager.AlterTypeDefinition("Package", cfg => cfg
                .WithPart("PackagePart")
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart")
                .WithPart("TagsPart")
                .WithSetting("TypeIndexing.Included", "true")
            );

            SchemaBuilder.CreateTable("ScreenshotPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("PackageId", col => col.NotNull())
                .Column<string>("PackageVersion", col => col.NotNull())
                .Column<string>("ScreenshotUri")
            );

            ContentDefinitionManager.AlterTypeDefinition("Screenshot", cfg => cfg
                .WithPart("ScreenshotPart")
                .WithPart("CommonPart")
            );

            SchemaBuilder.CreateTable(userkeyTableName, table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<int>("UserId", col => col.NotNull().Unique())
                .Column("AccessKey", DbType.Guid, col => col.NotNull().Unique())
            );
            string userkeyForeignKey = string.Format("FK_{0}_{1}", userTableName, userkeyTableName);
            SchemaBuilder.CreateForeignKey(userkeyForeignKey, userkeyTableName, new[] { "userId" }, "Orchard.Users", userTableName, new[] { "Id" });

            SchemaBuilder.CreateTable(userkeyPackageTableName, table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("PackageId", col => col.NotNull())
                .Column<int>(string.Format("{0}Id", userkeyTableName), col => col.NotNull()));

            string userKeyPackageForeignKey = string.Format("FK_{0}_{1}", userkeyTableName, userkeyPackageTableName);
            SchemaBuilder.CreateForeignKey(userKeyPackageForeignKey, userkeyPackageTableName, new[] {string.Format("{0}Id", userkeyTableName)}, userkeyTableName, new[] {"Id"});

            SchemaBuilder.CreateTable("GallerySettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("ServiceRoot")
                    .Column<string>("FeedUrl")
                    .Column<int>("LastPackageLogId", c => c.Nullable())
                    .Column<string>("ReportAbuseUserName")
                );

            return 1;
        }

        [UsedImplicitly]
        public int UpdateFrom1() {
            const string packagePartRecordTableName = "PackagePartRecord";

            SchemaBuilder.AlterTable(packagePartRecordTableName,
                table => {
                    table.AddColumn<bool>("IsRecommendedVersion",
                        c => {
                            c.NotNull();
                            c.WithDefault(false);
                        });
                    table.AddColumn<int>("TotalDownloadCount",
                        c => {
                            c.NotNull();
                            c.WithDefault(0);
                        });
                });

            return 2;
        }

        [UsedImplicitly]
        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("Package", cfg => cfg
                .WithPart("ReviewsPart")
            );

            SchemaBuilder.AlterTable("GallerySettingsPartRecord",
                table => table
                    .AddColumn<DateTime>("LastRatingSyncTime", column => column.Nullable())
            );
            return 3;
        }

        [UsedImplicitly]
        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("PackagePartRecord",
            table => table
                .AddColumn<DateTime>("Created", column => column.Nullable())
            );
            return 4;
        }

        [UsedImplicitly]
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("GallerySettingsPartRecord",
                table => {
                    table.AddColumn<int>("MaxNumberOfAllowedPreregisteredPackageIds", column => column.Nullable());
                    table.AddColumn<int>("NumberOfDaysUntilPreregisteredPackageIdExpires", column => column.Nullable());
                    table.AddColumn<int>("DaysInAdvanceToWarnUserOfExpiration", column => column.Nullable());
                    table.AddColumn<DateTime>("LastPackageIdExpirationCheckTime", column => column.Nullable());
                });

            SchemaBuilder.AlterTable("UserkeyPackage",
                table => table.AddColumn<DateTime>("RegisteredUtc", column => column.Nullable()));
            return 5;
        }

        [UsedImplicitly]
        public int UpdateFrom5() {
            ContentDefinitionManager.AlterPartDefinition("ProfilePart", builder => builder.WithField("OptOut", cfg => cfg.OfType("EmailOptOutField")));
            return 6;
        }

        [UsedImplicitly]
        public int UpdateFrom6() {
            CreateOrchardGalleryWidget<MostPopularPackageWidgetPart>("MostPopularPackageWidget");
            CreateOrchardGalleryWidget<HighestRatedPackageWidgetPart>("HighestRatedPackageWidget");
            CreateOrchardGalleryWidget<RecentPackageWidgetPart>("RecentPackageWidget");
            CreateOrchardGalleryWidget<GallerySummaryWidgetPart>("GallerySummaryWidget");
            CreateOrchardGalleryWidget<GallerySearchWidgetPart>("GallerySearchWidget");
            return 7;
        }

        private void CreateOrchardGalleryWidget<T>(string name) {
            ContentDefinitionManager.AlterPartDefinition(typeof(T).Name,
               builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(name, cfg => cfg
                .WithPart(name + "Part")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));
        }
    }
}