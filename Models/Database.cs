using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Models
{
    internal static class DatabasePaths : Object
    {
        public readonly static string BasePath =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            System.IO.Path.DirectorySeparatorChar + "UltrawideOverlays" +
            System.IO.Path.DirectorySeparatorChar;

        public readonly static string OverlaysPath = BasePath + "Overlays";
        public readonly static string SettingsPath = BasePath + "Settings";
        public readonly static string ImagesPath = BasePath + "Images";
    }
    public class Database
    {
        public List<OverlayDataModel> Overlays { get; }

        public Database()
        {
            Overlays = [];
        }

        public async Task InitAsync()
        {
            EnsureFoldersExist();
            await LoadOverlaysAsync();
        }

        private void EnsureFoldersExist()
        {
            foreach (var path in new[] {
            DatabasePaths.BasePath,
            DatabasePaths.OverlaysPath,
            DatabasePaths.SettingsPath,
            DatabasePaths.ImagesPath })
            {
                if (!FileHandlerUtil.IsValidFolderPath(path))
                    FileHandlerUtil.CreateDirectory(path);
            }
        }

        private async Task LoadOverlaysAsync()
        {
            if (!Directory.Exists(DatabasePaths.OverlaysPath)) return;

            var files = Directory.GetFiles(DatabasePaths.OverlaysPath, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var overlay = JsonSerializer.Deserialize<OverlayDataModel>(json);
                    if (overlay != null)
                        Overlays.Add(overlay);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading overlay from {file}: {ex.Message}");
                }
            }
        }

        //TODO: change param to List of ImageModels
        public async Task SaveOverlayAsync(OverlayDataModel overlay)
        {
            if (overlay == null) return;

            var filePath = Path.Combine(DatabasePaths.OverlaysPath, FileHandlerUtil.AddJsonFileExtension(overlay.Name));
            var json = JsonSerializer.Serialize(overlay);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task DeleteOverlayAsync(OverlayDataModel overlay)
        {
            if (overlay == null) return;
            var action = new Action(() => DeleteOverlay(overlay));
            await Task.Run(action);
        }

        private void DeleteOverlay(OverlayDataModel overlay)
        {
            var filePath = Path.Combine(DatabasePaths.OverlaysPath, FileHandlerUtil.AddJsonFileExtension(overlay.Name));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Overlays.Remove(overlay);
            }
        }
    }
}
