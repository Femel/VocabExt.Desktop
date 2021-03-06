//-------------------------------------------------------------------------------------
// Author:   Murray Foxcroft - April 2009
// Comments: code behind for the main WPF popup window 
//-------------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media.Animation;
using ExtendedWindowsControls;
using VX.Desktop.Controls.LoadingControl;
using VX.Desktop.Controls.TaskPanelControl;
using VX.Desktop.Infrastructure;

namespace VX.Desktop.Windows
{
    public partial class MainNotifyWindow
    {
        private readonly ExtendedNotifyIcon extendedNotifyIcon; // global class scope for the icon as it needs to exist foer the lifetime of the window
        private readonly Storyboard gridFadeInStoryBoard;
        private readonly Storyboard gridFadeOutStoryBoard;

        private readonly TaskPanel currentTask;
        private readonly LoadingAnimation loadingAnimation;

        private bool alreadyMoving;
        private const double Epsilon = .0000000001;

        /// <summary>
        /// Sets up the popup window and instantiates the notify icon
        /// </summary>
        public MainNotifyWindow()
        {
            // Create a manager (ExtendedNotifyIcon) for handling interaction with the notification icon and wire up events. 
            extendedNotifyIcon = new ExtendedNotifyIcon();
            extendedNotifyIcon.MouseLeave += extendedNotifyIcon_OnHideWindow;
            extendedNotifyIcon.MouseMove += extendedNotifyIcon_OnMouseMove;
            extendedNotifyIcon.MouseMove += extendedNotifyIcon_OnShowWindow;
            // TODO: localize
            extendedNotifyIcon.targetNotifyIcon.Text = "Vocabulary Extender";
            SetNotifyIcon();

            InitializeComponent();

            // Set the startup position and the startup state to "not visible"
            SetWindowToBottomRightOfScreen();
            Opacity = 0;
            uiGridMain.Opacity = 0;

            // Locate these storyboards and "cache" them - we only ever want to find these once for performance reasons
            gridFadeOutStoryBoard = (Storyboard)TryFindResource("gridFadeOutStoryBoard");
            gridFadeOutStoryBoard.Completed += GridFadeOutStoryBoardCompleted;
            gridFadeInStoryBoard = (Storyboard)TryFindResource("gridFadeInStoryBoard");
            gridFadeInStoryBoard.Completed += GridFadeInStoryBoardCompleted;
            InitTimers();

            currentTask = new TaskPanel();
            loadingAnimation = new LoadingAnimation();
        }

        /// <summary>
        /// Pulls an icon from the packed resource and applies it to the NotifyIcon control
        /// </summary>
        private void SetNotifyIcon()
        {
            var resourceInfo = Application.GetResourceStream(new Uri("pack://application:,,/Icons/tray.ico"));
            if (resourceInfo == null)
                throw new ArgumentException("Resource not found", "iconPrefix"); // TODO: localize

            var iconStream = resourceInfo.Stream;
            extendedNotifyIcon.targetNotifyIcon.Icon = new System.Drawing.Icon(iconStream);
        }

        /// <summary>
        /// Does what it says on the tin - ensures the popup window appears at the bottom right of the screen, just above the task bar
        /// </summary>
        private void SetWindowToBottomRightOfScreen()
        {
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height;
        }

        /// <summary>
        /// When the notification manager requests the popup to be displayed through this event, perform the below actions
        /// </summary>
        private void extendedNotifyIcon_OnShowWindow()
        {
            gridFadeOutStoryBoard.Stop();
            Opacity = 1; // Show the window (backing)
            Topmost = true; // Very rarely, the window seems to get "buried" behind others, this seems to resolve the problem
            if (uiGridMain.Opacity > 0 && uiGridMain.Opacity < 1) // If its animating, just set it directly to visible (avoids flicker and keeps the UX slick)
            {
                uiGridMain.Opacity = 1;
            }
            else if (Math.Abs(uiGridMain.Opacity - 0) < Epsilon)
            {
                if (alreadyMoving)
                {
                    return;
                }

                HandleLoadingDelay();   
                alreadyMoving = true;
                gridFadeInStoryBoard.Begin();  // If it is in a fully hidden state, begin the animation to show the window
            }
        }

        /// <summary>
        /// When the notification manager requests the popup to be hidden through this event, perform the below actions
        /// </summary>
        void extendedNotifyIcon_OnHideWindow()
        {
            gridFadeInStoryBoard.Stop(); // Stop the fade in storyboard if running.

            // Only start fading out if fully faded in, otherwise you get a flicker effect in the UX because the animation resets the opacity
            if (Math.Abs(uiGridMain.Opacity - 1) < Epsilon && Math.Abs(Opacity - 1) < Epsilon)
                gridFadeOutStoryBoard.Begin();
            else // Just hide the window and grid
            {
                uiGridMain.Opacity = 0;
                Opacity = 0;
                alreadyMoving = false;
            }
        }

        /// <summary>
        /// When the mouse enters the popup window's bounds, cancel any pending closing actions and immediately show the popup. 
        /// This is primarily to handle the case where the mouse termporarily leaves the popup window and returns again - 
        /// a UX / usability enhancement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UiWindowMainNotificationMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Cancel the mouse leave event from firing, stop the fade out storyboard from running and enusre the grid is fully visible
            extendedNotifyIcon.StopMouseLeaveEventFromFiring(); 
            gridFadeOutStoryBoard.Stop(); 
            uiGridMain.Opacity = 1;
            AutomaticPopupDispatcher.Instance.Stop();
        }

        /// <summary>
        /// If the mouse leaves the popup, start the process to close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UiWindowMainNotificationMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            extendedNotifyIcon_OnHideWindow();
            AutomaticPopupDispatcher.Instance.StartCountForHide();
        }

        /// <summary>
        /// Once the grid fades out, set the backing window to "not visible"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GridFadeOutStoryBoardCompleted(object sender, EventArgs e)
        {
            Opacity = 0;
            alreadyMoving = false;
        }

        /// <summary>
        /// Once the grid fades out, set the backing window to "visible"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GridFadeInStoryBoardCompleted(object sender, EventArgs e)
        {
            Opacity = 1;
        }

        /// <summary>
        /// When the pin button is pressed/unpressed, switch the icon appropriately. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            currentTask.Refresh();
        }

        /// <summary>
        /// Shut down the popup window and dispose the notify icon (otherwise it hangs around in the task bar until you mouse over) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            extendedNotifyIcon.Dispose();
            Close();
        }

        private void InitTimers()
        {
            AutomaticPopupDispatcher.Instance.Hide += (sender, args) => extendedNotifyIcon_OnHideWindow();
            AutomaticPopupDispatcher.Instance.Show += (sender, args) => extendedNotifyIcon_OnShowWindow();
            AutomaticPopupDispatcher.Instance.ShowTimer.Interval = TimeSpan.FromMinutes(
                ApplicationSettings.Instance.PoputIntervalMinutes);
            AutomaticPopupDispatcher.Instance.HideTimer.Interval = TimeSpan.FromSeconds(5);
            AutomaticPopupDispatcher.Instance.StartCountForShow();
        }

        private void extendedNotifyIcon_OnMouseMove()
        {
            if (AutomaticPopupDispatcher.Instance.IsTriggeredAutomatically)
            {
                AutomaticPopupDispatcher.Instance.IsTriggeredAutomatically = false;
            }
        }

        private void HandleLoadingDelay()
        {
            MainContent.Children.Clear();
            if (currentTask.Refresh())
            {
                MainContent.Children.Add(currentTask);
            }
            else
            {
                MainContent.Children.Add(loadingAnimation);
            }
        }
    }
}