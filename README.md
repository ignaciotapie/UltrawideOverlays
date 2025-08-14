# UltrawideOverlays
[![Ask DeepWiki](https://devin.ai/assets/askdeepwiki.png)](https://deepwiki.com/ignaciotapie/UltrawideOverlays)

UltrawideOverlays is a Windows desktop application designed to enhance the gaming experience on ultrawide monitors. It allows you to create and display custom static overlays to cover the black bars that appear when playing games that do not support native ultrawide resolutions.

![Demo](https://github.com/ignaciotapie/UltrawideOverlays/blob/master/Demo/kh3%20demo.gif)

## Key Features

*   **Visual Overlay Editor**: A powerful, fullscreen editor to design your overlays. Drag, drop, resize, scale, and mirror multiple images to create your perfect design.
*   **Game Library**: Add games to a personal library, either from a list of running processes or by selecting an executable file.
*   **Automatic Overlay Activation**: Link your custom overlays to specific games. The application automatically detects when a linked game is in focus and displays the corresponding overlay.
*   **Global Hotkey Support**:
    *   Toggle the current overlay's visibility on and off.
    *   Increase or decrease the overlay's opacity in real-time.
    *   Open a mini-manager to quickly assign an overlay to the currently focused application.
*   **Customizable Editor Grid**: The editor features a snapping grid with configurable size, color, and opacity to help with precise image placement.
*   **Clipping Masks**: Use clipping masks to define areas where overlay images should not be rendered, perfect for creating complex layouts.

## How It Works

1.  **Create an Overlay**: Use the **Overlay Editor** to add images, arrange them, and save the final composite as a new overlay.
2.  **Add a Game**: Navigate to the **Games** page to add a game to your library. You can select a running process or browse for the game's `.exe` file.
3.  **Link Overlay to Game**: While adding or editing a game, select one of your saved overlays from the dropdown menu to link it.
4.  **Play**: Launch your game. UltrawideOverlays runs in the background, monitors the active window, and automatically displays the correct overlay when it detects a linked game. You can use global hotkeys to adjust the overlay while you play.

## Technical Overview

*   **Framework**: The application is built using **Avalonia UI**, a cross-platform UI framework for .NET.
*   **Architecture**: It follows the **Model-View-ViewModel (MVVM)** pattern, leveraging `CommunityToolkit.Mvvm` for view model implementation and data binding.
*   **Data Storage**: All user data, including overlays, game links, and settings, are stored locally as JSON files in the `My Documents/UltrawideOverlays` directory.
*   **Services**:
    *   `FocusMonitorService`: Uses the `UIAutomationClient` library to listen for window focus changes across the system, enabling automatic overlay activation.
    *   `HotKeyService`: Employs P/Invoke calls to `user32.dll` to register and handle global hotkeys, allowing for in-game control without needing to alt-tab.
    *   `Data Services`: A suite of services (`OverlayDataService`, `GamesDataService`, `SettingsDataService`) manage the loading and saving of application data.

## Q&A

*  **Q:** My overlay isn't shown on top of the game/window. 
*  **A:** Game needs to be set to Borderless Windowed, it won't work wih Exclusive Fullscreen. You can use a utility like Borderless Gaming to make all games borderless

## Issues
*  There's some memory leaks happening when changing windows, Overlay Editor seems to be the only one that closes after a while correctly. If the RAM usage gets high, restart the app.
