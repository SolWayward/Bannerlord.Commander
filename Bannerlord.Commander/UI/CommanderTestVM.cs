using TaleWorlds.Library;

namespace Bannerlord.Commander.UI
{
    /// <summary>
    /// Minimal ViewModel for the CommanderTestScreen
    /// </summary>
    public class CommanderTestVM : ViewModel
    {
        private string _titleText;
        private string _infoText;

        // Event to notify the handler that close was requested
        public event System.Action OnCloseRequested;

        public CommanderTestVM()
        {
            TitleText = "Commander Test Screen";
            InfoText = "This is a minimal test UI. Press ESC or click Close to exit.";
        }

        [DataSourceProperty]
        public string TitleText
        {
            get => _titleText;
            set
            {
                if (_titleText != value)
                {
                    _titleText = value;
                    OnPropertyChangedWithValue(value, nameof(TitleText));
                }
            }
        }

        [DataSourceProperty]
        public string InfoText
        {
            get => _infoText;
            set
            {
                if (_infoText != value)
                {
                    _infoText = value;
                    OnPropertyChangedWithValue(value, nameof(InfoText));
                }
            }
        }

        /// <summary>
        /// Called when Close button is clicked (bound in XML via Command.Click="ExecuteClose")
        /// </summary>
        public void ExecuteClose()
        {
            OnCloseRequested?.Invoke();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            OnCloseRequested = null;
        }
    }
}
