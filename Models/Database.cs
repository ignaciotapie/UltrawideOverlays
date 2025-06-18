using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UltrawideOverlays.Converters.Comparers;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Models
{
    public enum DatabaseFiles
    {
        Overlays,
        Games,
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
        public readonly static string GamesPath = BasePath + "Games";
    }
    public class Database
    {
        public HashSet<OverlayDataModel> Overlays { get; }
        public HashSet<GamesModel> Games { get; }
        public SettingsDataModel Settings { get; set; }

        public bool isInitialized = false;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        private Database()
        {
            Overlays = new HashSet<OverlayDataModel>(new OverlayDataModelComparer());
            Games = new HashSet<GamesModel>(new GamesModelComparer());
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
            var gamesTask = LoadGames();
            var settingsTask = LoadSettings();

            await Task.WhenAll([overlayTask, settingsTask, gamesTask]);
            isInitialized = true;
        }

        private async Task LoadGames()
        {
            var gamesFiles = Directory.GetFiles(DatabasePaths.GamesPath, "*.json");
            foreach (var file in gamesFiles)
            {
                try
                {
                    var game = await LoadAsync<GamesModel>(file, DatabaseFiles.Games);
                    if (game != null)
                    {
                        Games.Add(game);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading overlay from file {file}: {ex.Message}");
                }
            }
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
            var overlayFiles = Directory.GetFiles(DatabasePaths.OverlaysModelPath, "*.json");
            foreach (var file in overlayFiles)
            {
                try
                {
                    var overlay = await LoadAsync<OverlayDataModel>(file, DatabaseFiles.Overlays);
                    if (overlay != null)
                    {
                        Overlays.Add(overlay);
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
                DatabasePaths.GamesPath,
                DatabasePaths.ImagesPath
            };

            foreach (var path in paths)
            {
                if (!FileHandlerUtil.IsValidFolderPath(path))
                    FileHandlerUtil.CreateDirectory(path);
            }
        }

        ///////////////////////////////////////////
        /// GENERIC
        ///////////////////////////////////////////
        public async Task SaveAsync(object data, DatabaseFiles fileType)
        {
            string path = string.Empty;
            path = GetPathFromFileType(data, fileType);

            switch (fileType)
            {
                case DatabaseFiles.Overlays:
                    var overlay = data as OverlayDataModel;
                    // Check if the overlay already exists in the collection
                    if (Overlays.Contains(overlay))
                    {
                        Overlays.Remove(overlay);
                    }

                    Overlays.Add(overlay);
                    break;
                case DatabaseFiles.Settings:
                    Settings = (SettingsDataModel)data;
                    break;
                case DatabaseFiles.Games:
                    var game = data as GamesModel;
                    // Check if the game already exists in the collection
                    if (Games.Contains(game))
                    {
                        Games.Remove(game);
                    }

                    Games.Add(game);
                    break;
                case DatabaseFiles.Images:
                    throw new InvalidOperationException("Cannot save images as JSON");
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }

            var serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(data, typeof(Object), serializerOptions);
            await FileHandlerUtil.WriteToJSON(path, json);
        }

        private string GetPathFromFileType(object data, DatabaseFiles fileType)
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
                        throw new ArgumentException("Data must be of type OverlayDataModel", nameof(data));
                    }
                    path = Path.Combine(DatabasePaths.OverlaysModelPath, FileHandlerUtil.AddJSONFileExtension(name));
                    break;
                case DatabaseFiles.Settings:
                    // Settings file is always named "Settings.json"
                    path = Path.Combine(DatabasePaths.SettingsModelPath, FileHandlerUtil.AddJSONFileExtension("Settings"));
                    break;
                case DatabaseFiles.Images:
                    if (data is OverlayDataModel overlayData)
                    {
                        name = overlayData.Name;
                    }
                    else
                    {
                        throw new ArgumentException("Data must be of type OverlayDataModel", nameof(data));
                    }
                    path = Path.Combine(DatabasePaths.ImagesPath, FileHandlerUtil.AddImageFileExtension(name, ImageExtension.PNG));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }

            return path;
        }

        public async Task<T> LoadAsync<T>(string fileName, DatabaseFiles fileType)
        {
            string path = string.Empty;
            path = fileType switch
            {
                DatabaseFiles.Overlays => Path.Combine(DatabasePaths.OverlaysModelPath, FileHandlerUtil.AddJSONFileExtension(fileName)),
                DatabaseFiles.Games => Path.Combine(DatabasePaths.GamesPath, FileHandlerUtil.AddJSONFileExtension(fileName)),
                DatabaseFiles.Settings => Path.Combine(DatabasePaths.SettingsModelPath, FileHandlerUtil.AddJSONFileExtension(fileName)),
                DatabaseFiles.Images => throw new InvalidOperationException("Cannot load images as JSON"),
                _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null),
            };
            if (!FileHandlerUtil.IsValidFilePath(path))
            {
                throw new FileNotFoundException("File not found", path);
            }

            using (StreamReader reader = new StreamReader(path))
            {
                return await JsonSerializer.DeserializeAsync<T>(reader.BaseStream);
            }
        }


        public async Task DeleteAsync(Object obj, DatabaseFiles fileType)
        {
            var path = GetPathFromFileType(obj, fileType);

            if (!FileHandlerUtil.IsValidFilePath(path))
            {
                throw new FileNotFoundException("File not found", path);
            }

            if (fileType == DatabaseFiles.Overlays && obj is OverlayDataModel overlay)
            {
                if (Overlays.Contains(overlay))
                {
                    Overlays.Remove(overlay);
                }
                // Also delete the image created
                if (!string.IsNullOrEmpty(overlay.Path) && FileHandlerUtil.IsValidFilePath(overlay.Path))
                {
                    await Task.Run(() => { File.Delete(overlay.Path); });
                }
            }
            else if (fileType == DatabaseFiles.Settings)
            {
                Settings = new SettingsDataModel();
            }
            else if (fileType == DatabaseFiles.Games && obj is GamesModel game)
            {
                if (Games.Contains(game))
                {
                    Games.Remove(game);
                }
            }

            try
            {
                await Task.Run(() => { File.Delete(path); });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting file {path}: {ex.Message}");
                throw;
            }
        }

        ///////////////////////////////////////////
        /// BITMAPS
        ///////////////////////////////////////////
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

        public async Task SaveBitmapFromOverlayAsync(OverlayDataModel overlayData)
        {
            string path = Path.Combine(DatabasePaths.ImagesPath, FileHandlerUtil.AddImageFileExtension(overlayData.Name, ImageExtension.PNG)!);
            overlayData.Path = path;

            Dispatcher.UIThread.Post(() => SaveBitmap(overlayData, path));
            await Task.CompletedTask;
        }

        private void SaveBitmap(OverlayDataModel overlayData, string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            // Save the bitmap to the specified path
            if (overlayData.ImageModels == null || overlayData.ImageModels.Count == 0) throw new InvalidOperationException("No images to save");

            var bitmap = ImageRenderer.RenderImagesToBitmap(overlayData);
            bitmap.Save(path);
        }
    }
}