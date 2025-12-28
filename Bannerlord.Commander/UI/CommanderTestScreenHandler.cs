using System;
using Bannerlord.GameMaster.Information;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.Commander.UI
{
    /// <summary>
    /// Simplified handler for the Commander Test Screen
    /// </summary>
    public class CommanderTestScreenHandler
    {
        private GauntletLayer _layer;
        private GauntletMovieIdentifier _movie;
        private CommanderTestVM _viewModel;
        private ScreenBase _screen;

        public bool IsActive { get; private set; }

        public void Show(ScreenBase screen)
        {
            if (IsActive)
            {
                InfoMessage.Log("Test screen already active");
                return;
            }

            if (screen == null)
            {
                InfoMessage.Error("Cannot show screen: screen is null");
                return;
            }

            try
            {
                _screen = screen;
                InfoMessage.Log($"Screen type: {_screen.GetType().Name}");

                // Create ViewModel first
                _viewModel = new CommanderTestVM();
                _viewModel.OnCloseRequested += Close;
                InfoMessage.Log("ViewModel created");

                // Create layer - (categoryId, localOrder, shouldClear)
                _layer = new GauntletLayer("GauntletLayer", 10000, false);
                InfoMessage.Log("Layer created");

                // Add layer to screen FIRST
                _screen.AddLayer(_layer);
                InfoMessage.Log($"Layer added. Layer count: {_screen.Layers.Count}");

                // THEN load the movie
                _movie = _layer.LoadMovie("CommanderTestScreen", _viewModel);
                InfoMessage.Log($"Movie loaded: MovieName={GetMovieName()}, IsDefault={_movie.Equals(default(GauntletMovieIdentifier))}");

                // Enable input
                _layer.InputRestrictions.SetInputRestrictions();
                _layer.IsFocusLayer = true;
                ScreenManager.TrySetFocus(_layer);

                IsActive = true;
                InfoMessage.Log("Commander Test Screen show complete");
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"Failed to open screen: {ex.Message}\n{ex.StackTrace}");
                Close();
            }
        }

        private string GetMovieName()
        {
            try
            {
                // Use reflection to get movie name for debugging
                var movieField = _movie.GetType().GetField("MovieName");
                return movieField?.GetValue(_movie)?.ToString() ?? "unknown";
            }
            catch
            {
                return "error";
            }
        }

        public void Close()
        {
            if (!IsActive)
                return;

            InfoMessage.Log("Closing Commander Test Screen...");

            try
            {
                // Remove layer first (safest order)
                if (_layer != null && _screen != null)
                {
                    try
                    {
                        _screen.RemoveLayer(_layer);
                        InfoMessage.Log("Layer removed from screen");
                    }
                    catch (Exception ex)
                    {
                        InfoMessage.Error($"Error removing layer: {ex.Message}");
                    }
                }

                // Cleanup ViewModel
                if (_viewModel != null)
                {
                    _viewModel.OnCloseRequested -= Close;
                    _viewModel.OnFinalize();
                    InfoMessage.Log("ViewModel finalized");
                }
            }
            catch (Exception ex)
            {
                InfoMessage.Error($"Error during cleanup: {ex.Message}");
            }
            finally
            {
                _movie = default;
                _layer = null;
                _viewModel = null;
                _screen = null;
                IsActive = false;
                InfoMessage.Log("Commander Test Screen closed");
            }
        }

        public void OnTick(float dt)
        {
            if (!IsActive || _layer == null)
                return;

            // Handle ESC key
            if (Input.IsKeyPressed(InputKey.Escape))
            {
                Close();
            }
        }
    }
}
