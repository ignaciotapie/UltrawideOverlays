using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Models
{
    public enum DatabaseFiles 
    {
        Overlays,
        Settings,
        Images
    }

    internal static class DatabasePaths
    {
        public readonly static string BasePath =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            System.IO.Path.DirectorySeparatorChar + "UltrawideOverlays" +
            System.IO.Path.DirectorySeparatorChar;

        public readonly static string OverlaysModelPath = BasePath + "Overlays";
        public readonly static string SettingsModelPath = BasePath + "Settings";
        public readonly static string ImagesPath = BasePath + "Images";
    }
    public class Database
    {
        public List<OverlayDataModel> Overlays { get; }
        public SettingsDataModel Settings { get; set; }

        bool isInitialized = false;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        private Database()
        {
            Overlays = [];
            Settings = new SettingsDataModel();
        }
        //Usable asynchronous constructor
        public async static Task<Database> BuildDatabaseAsync()
        {
            var db = new Database();
            await db.Init();
            return db;
        }

        public async Task Init()
        {
            EnsureFoldersExist();
            var overlayTask = LoadOverlays();
            var settingsTask = LoadSettings();

            await Task.WhenAll([overlayTask, settingsTask]);
            isInitialized = true;
        }

        public async Task<bool> WaitUntilInitialized() 
        {
            while (!isInitialized)
            {
                await Task.Delay(100);
            }
            return true;
        }

        private async Task LoadSettings()
        {
            var settingsFile = Path.Combine(DatabasePaths.SettingsModelPath, FileHandlerUtil.AddJSONFileExtension("Settings"));
            if (FileHandlerUtil.IsValidFilePath(settingsFile))
            {
                try
                {
                    Settings = await LoadAsync<SettingsDataModel>(settingsFile, DatabaseFiles.Settings);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading settings from file {settingsFile}: {ex.Message}");
                }
            }
            else
            {
                Settings = new SettingsDataModel();
                await SaveAsync(Settings, DatabaseFiles.Settings);
            }
        }

        private async Task LoadOverlays()
        {
            await Task.Run(() => LoadOverlaysFromFiles());
        }

        private void LoadOverlaysFromFiles()
        {
            var overlayFiles = Directory.GetFiles(DatabasePaths.OverlaysModelPath, "*.json");
            foreach (var file in overlayFiles)
            {
                try
                {
                    var overlay = LoadAsync<OverlayDataModel>(file, DatabaseFiles.Overlays);
                    if (overlay != null)
                    {
                        Overlays.Add(overlay.Result);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading overlay from file {file}: {ex.Message}");
                }
            }
        }

        private void EnsureFoldersExist()
        {
            var paths = new[]
            {
                DatabasePaths.BasePath,
                DatabasePaths.OverlaysModelPath,
                DatabasePaths.SettingsModelPath,
                DatabasePaths.ImagesPath
            };

            foreach (var path in paths)
            {
                if (!FileHandlerUtil.IsValidFolderPath(path))
                    FileHandlerUtil.CreateDirectory(path);
            }
        }

        public async Task SaveAsync(object data, DatabaseFiles fileType)
        {
            string path = string.Empty;
            string name = string.Empty;
            switch (fileType)
            {
                case DatabaseFiles.Overlays:
                    if (data is OverlayDataModel overlay)
                    {
                        name = overlay.Name;
                    }
                    else
                    {
                        throw new ArgumentException("Data must be of type OverlayDataModel");
                    }
                    path = Path.Combine(DatabasePaths.OverlaysModelPath, FileHandlerUtil.AddJSONFileExtension(name));

                    Overlays.Add(overlay);
                    break;
                case DatabaseFiles.Settings:
                    path = Path.Combine(DatabasePaths.SettingsModelPath, FileHandlerUtil.AddJSONFileExtension("Settings"));
                    Settings = (SettingsDataModel)data;
                    break;
                case DatabaseFiles.Images:
                    throw new InvalidOperationException("Cannot save images as JSON");
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }

            var json = JsonSerializer.Serialize(data);
            await FileHandlerUtil.WriteToJSON(path, json);
        }

        public async Task<T> LoadAsync<T>(string fileName, DatabaseFiles fileType)
        {
            string path = string.Empty;
            switch (fileType)
            {
                case DatabaseFiles.Overlays:
                    path = Path.Combine(DatabasePaths.OverlaysModelPath, FileHandlerUtil.AddJSONFileExtension(fileName));
                    break;
                case DatabaseFiles.Settings:
                    path = Path.Combine(DatabasePaths.SettingsModelPath, FileHandlerUtil.AddJSONFileExtension(fileName));
                    break;
                case DatabaseFiles.Images:
                    throw new InvalidOperationException("Cannot load images as JSON");
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }

            if (!FileHandlerUtil.IsValidFilePath(path))
            {
                throw new FileNotFoundException("File not found", path);
            }

            StreamReader reader = new(path);
            return await JsonSerializer.DeserializeAsync<T>(reader.BaseStream);
        }

        public async Task<Bitmap> LoadBitmapAsync(string fileName)
        {
            string path = Path.Combine(DatabasePaths.ImagesPath, FileHandlerUtil.AddImageFileExtension(fileName, ImageExtension.PNG)!);
            if (!FileHandlerUtil.IsValidFilePath(path))
            {
                throw new FileNotFoundException("Image file not found", path);
            }

            //Load bitmap asynchronously
            return await Task.Run(() => new Bitmap(path));
        }
    }
}
