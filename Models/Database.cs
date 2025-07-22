using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UltrawideOverlays.Converters.Comparers;
using UltrawideOverlays.Utils;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace UltrawideOverlays.Models
{
    public enum DatabaseFiles
    {
        Overlays,
        Games,
        Settings,
        Images,
        Activities
    }

    internal static class DatabaseSettings
    {
        public static readonly int MaxActivitiesStored = 50;
    }

    internal static class DatabasePaths
    {
        public readonly static string BasePath =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            System.IO.Path.DirectorySeparatorChar + "UltrawideOverlays" +
            System.IO.Path.DirectorySeparatorChar;

        public readonly static string OverlaysModelPath = BasePath + "Overlays";
        public readonly static string SettingsPath = BasePath + "Settings";
        public readonly static string ImagesPath = BasePath + "Images";
        public readonly static string GamesPath = BasePath + "Games";
        public readonly static string ActivitiesPath = SettingsPath;
    }
    public class Database
    {
        public HashSet<OverlayDataModel> Overlays { get; }
        public HashSet<GamesModel> Games { get; }
        public SettingsDataModel? Settings { get; set; }
        public Stack<ActivityLogModel> Activities { get; set; }

        public bool isInitialized = false;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        private Database()
        {
            Overlays = new HashSet<OverlayDataModel>(new OverlayDataModelComparer());
            Games = new HashSet<GamesModel>(new GamesModelComparer());
            Settings = new SettingsDataModel();
            Activities = new Stack<ActivityLogModel>();
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
            var activitiesTask = LoadActivities();

            await Task.WhenAll([overlayTask, settingsTask, gamesTask, activitiesTask]);
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
            var settingsFile = Path.Combine(DatabasePaths.SettingsPath, FileHandlerUtil.AddJSONFileExtension("Settings"));
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

        public async Task LoadActivities()
        {
            var activityFiles = Directory.GetFiles(DatabasePaths.ActivitiesPath, FileHandlerUtil.AddJSONFileExtension("Activities"));
            foreach (var file in activityFiles)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        string json = await reader.ReadToEndAsync();
                        var activities = JsonSerializer.Deserialize<HashSet<ActivityLogModel>>(json);
                        if (activities != null)
                        {
                            // Add activities to the list, ensuring we don't exceed the max limit
                            foreach (var activity in activities)
                            {
                                AddActivity(activity, false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading activities from file {file}: {ex.Message}");
                }
            }
        }

        private void EnsureFoldersExist()
        {
            var paths = new[]
            {
                DatabasePaths.BasePath,
                DatabasePaths.OverlaysModelPath,
                DatabasePaths.SettingsPath,
                DatabasePaths.GamesPath,
                DatabasePaths.ImagesPath,
                DatabasePaths.ActivitiesPath
            };

            foreach (var path in paths)
            {
                if (!FileHandlerUtil.IsValidFolderPath(path))
                    FileHandlerUtil.CreateDirectory(path);
            }
        }
        public async Task SaveAsync(object? data, DatabaseFiles fileType, bool recordActivity = false)
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
                        if (recordActivity) AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Overlays, ActivityLogAction.Updated, overlay.Name));
                    }
                    else
                    {
                        if (recordActivity) AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Overlays, ActivityLogAction.Added, overlay.Name));
                    }

                    Overlays.Add(overlay);
                    break;
                case DatabaseFiles.Settings:
                    Settings = null;
                    Settings = data as SettingsDataModel ?? new SettingsDataModel();
                    if (recordActivity) AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Settings, ActivityLogAction.Updated, "Settings"));
                    break;
                case DatabaseFiles.Games:
                    var game = data as GamesModel;
                    // Check if the game already exists in the collection
                    if (Games.Contains(game))
                    {
                        Games.Remove(game);
                        if (recordActivity) AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Games, ActivityLogAction.Updated, game.Name));
                    }
                    else
                    {
                        AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Games, ActivityLogAction.Added, game.Name));
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

            if (recordActivity)
            {
                await SaveActivitiesAsync();
            }
        }

        private async Task SaveActivitiesAsync()
        {
            var serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(Activities, serializerOptions);
            await FileHandlerUtil.WriteToJSON(GetPathFromFileType(null, DatabaseFiles.Activities), json);
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
                    path = Path.Combine(DatabasePaths.SettingsPath, FileHandlerUtil.AddJSONFileExtension("Settings"));
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
                case DatabaseFiles.Games:
                    if (data is GamesModel game)
                    {
                        name = game.Name;
                    }
                    else
                    {
                        throw new ArgumentException("Data must be of type GamesModel", nameof(data));
                    }
                    path = Path.Combine(DatabasePaths.GamesPath, FileHandlerUtil.AddJSONFileExtension(name));
                    break;
                case DatabaseFiles.Activities:
                    path = Path.Combine(DatabasePaths.ActivitiesPath, FileHandlerUtil.AddJSONFileExtension("Activities"));
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
                DatabaseFiles.Settings => Path.Combine(DatabasePaths.SettingsPath, FileHandlerUtil.AddJSONFileExtension(fileName)),
                DatabaseFiles.Images => throw new InvalidOperationException("Cannot load images as JSON"),
                DatabaseFiles.Activities => Path.Combine(DatabasePaths.ActivitiesPath, FileHandlerUtil.AddJSONFileExtension(fileName)),
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
                    AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Overlays, ActivityLogAction.Removed, overlay.Name));
                }
                // Also delete the image created
                if (!string.IsNullOrEmpty(overlay.Path) && FileHandlerUtil.IsValidFilePath(overlay.Path))
                {
                    await Task.Run(() => { File.Delete(overlay.Path); });
                }
                // Find if any games are using this overlay and remove the association
                foreach (var game in Games)
                {
                    if (game.OverlayName == overlay.Name)
                    {
                        game.OverlayName = "<Removed Overlay>"; // Clear the overlay association
                        AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Games, ActivityLogAction.Updated, game.Name));
                    }
                }
            }
            else if (fileType == DatabaseFiles.Settings)
            {
                Settings = new SettingsDataModel();
                AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Settings, ActivityLogAction.Removed, "Settings"));
            }
            else if (fileType == DatabaseFiles.Games && obj is GamesModel game)
            {
                if (Games.Contains(game))
                {
                    Games.Remove(game);
                    AddActivity(new ActivityLogModel(DateTime.Now, ActivityLogType.Games, ActivityLogAction.Removed, game.Name));
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

        public ICollection<ProcessDataModel> LoadProcesses()
        {
            var processList = Process.GetProcesses();

            var filteredProcessList = ProcessFilterUtils.FilterProcesses(processList);

            var processDataModels = new List<ProcessDataModel>();
            foreach (var process in filteredProcessList)
            {
                var processPath = GetProcessPath(process);
                var icon = String.IsNullOrEmpty(processPath) ? SystemIcons.Application : Icon.ExtractAssociatedIcon(processPath);

                var processData = new ProcessDataModel
                {
                    Name = process.ProcessName,
                    Path = processPath,
                    ProcessId = (uint)process.Id
                };

                using (var memoryStream = new MemoryStream())
                {
                    icon?.ToBitmap()?.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0;
                    processData.Icon = new Avalonia.Media.Imaging.Bitmap(memoryStream);
                }

                processDataModels.Add(processData);
            }

            return processDataModels;
        }

        private string GetProcessPath(Process p)
        {
            var outString = "";
            try
            {
                outString = p.MainModule.FileName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading process {p.ProcessName}: {ex.Message}");
            }

            return outString;
        }

        public void AddActivity(ActivityLogModel activity, bool saveAfter = true)
        {
            if (Activities.Count >= DatabaseSettings.MaxActivitiesStored)
            {
                Activities.Pop(); // Remove the oldest activity
            }

            Debug.WriteLine($"Activity: {activity.ToString()}");

            Activities.Push(activity);

            if (saveAfter)
            {
                SaveActivitiesAsync();
            }
        }
    }
}