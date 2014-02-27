using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Impl
{
    [UsedImplicitly]
    public class LastSynchronizedDate : ILastSynchronizedDate
    {
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;

        private readonly string _basePath;

        private const string LastIndexUtcElementName = "lastIndexUtc";
        private const string SettingsElementName = "settings";

        public LastSynchronizedDate(IAppDataFolder appDataFolder, ShellSettings shellSettings) {
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;

            _basePath = _appDataFolder.Combine("Sites", _shellSettings.Name, "Packages");
            CreateBaseDirectoryIfItDoesNotExist();
        }

        public DateTime Get() {
            var settingsFileName = GetSettingsFileName();
            if (_appDataFolder.FileExists(settingsFileName)) {
                return DateTime.Parse(XDocument.Load(settingsFileName).Descendants(LastIndexUtcElementName).First().Value);
            }
            return new DateTime(1980, 1, 1);
        }

        public void Set(DateTime lastSynchronizationDate) {
            var settingsFileName = GetSettingsFileName();
            if (!_appDataFolder.FileExists(settingsFileName)) {
                CreateNewSettingsFile(lastSynchronizationDate);
            }

            XDocument doc = XDocument.Load(settingsFileName);
            XElement settingsElement = doc.Element(SettingsElementName);
            if (settingsElement != null) {
                XElement lastIndexUtcElement = settingsElement.Element(LastIndexUtcElementName);
                if (lastIndexUtcElement != null) {
                    lastIndexUtcElement.Value = lastSynchronizationDate.ToString("s");
                    doc.Save(settingsFileName);
                }
                else {
                    DeleteSettingsFileIfItExists();
                    CreateNewSettingsFile(lastSynchronizationDate);
                }
            }
            else {
                DeleteSettingsFileIfItExists();
                CreateNewSettingsFile(lastSynchronizationDate);
            }
        }

        private void CreateBaseDirectoryIfItDoesNotExist() {
            var directoryInfo = new DirectoryInfo(_appDataFolder.MapPath(_basePath));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }

        private void CreateNewSettingsFile(DateTime lastSyncDate) {
            var lastSyncDateString = lastSyncDate.ToString("s");
            CreateBaseDirectoryIfItDoesNotExist();
            XDocument doc = new XDocument(new XElement(SettingsElementName, new XElement(LastIndexUtcElementName, lastSyncDateString)));
            doc.Save(GetSettingsFileName());
        }

        private void DeleteSettingsFileIfItExists() {
            if (_appDataFolder.FileExists(GetSettingsFileName()))
            {
                _appDataFolder.DeleteFile(GetSettingsFileName());
            }
        }

        private string GetSettingsFileName() {
            return _appDataFolder.MapPath(_appDataFolder.Combine(_basePath, "synchronization.settings.xml"));
        }
    }
}