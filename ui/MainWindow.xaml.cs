﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fast_Jira.api;
using Fast_Jira.core;

namespace Fast_Jira.ui
{
    public static class Constants
    {
        public const string DateTimeUiFormat = "dd.MM.yyyy HH:mm";
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DataVault _vault = new DataVault();
        private SearchWindow _searchWindow;
        private readonly Config _appConfig = new Config();
        private int _updateTicker = 0;
        private bool _selectionActive = true;

        public MainWindow()
        {
            _vault.InitFromDisk();
            ConfigureClient();

            InitializeComponent();
            ConfigureUi();

            RefreshDisplayedIssue();
            UrlTextbox.Text = _vault.DisplayedIssue?.Key ?? "";

            Thread watchdog = new Thread(WatchClipboard)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            if (watchdog.TrySetApartmentState(ApartmentState.STA))
            {
                watchdog.Start();
            }
            else
            {
                Logger.Error("Unable to set thread state for clipboard watcher");
            }

            Logger.Debug("Application startup done.");
        }

        private void RefreshIssueHistory()
        {
            HistoryList.Items.Clear();
            int k = 1;
            foreach (Issue item in _vault.GetAllIssuesSorted())
            {
                string hotkey = k <= 9 ? "(" + k + ")" : "";
                k++;
                HistoryEntry entry = new HistoryEntry(hotkey, item.Key, item.Summary, _vault.GetWrappedImage(item.Type.IconUrl));
                HistoryList.Items.Add(entry);

                if (item.Key == _vault.DisplayedIssue.Key)
                {
                    _selectionActive = false;
                    HistoryList.SelectedItem = entry;
                    _selectionActive = true;
                }
            }
        }

        private void ConfigureUi()
        {
            Style = (Style)FindResource(typeof(Window));
            ProgressBar.IsIndeterminate = false;

            buttonSettings.Command = new RelayCommand(SettingsCommand_Executed);
            buttonBrowser.Command = new RelayCommand(BrowserCommand_Executed);

            UrlErrorText.Text = "";
            StatusText.Text = "";
            UrlTextbox.Focus();

            KeyDown += MainWindow_KeyDown;
            MouseWheel += MainWindow_MouseWheel;
            MarkdownViewer.MouseWheel += MainWindow_MouseWheel;
            HistoryList.SelectionChanged += HistoryList_SelectionChanged;
            UrlTextbox.KeyDown += UrlTextboxKeyDownHandler;
            DescriptionScrollView.ScrollChanged += DescriptionScrollView_ScrollChanged;
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta != 0)
            {
                double currentOffset = DescriptionScrollView.VerticalOffset;
                double targetScroll = Math.Clamp(currentOffset - e.Delta, 0, DescriptionScrollView.ScrollableHeight);
                DescriptionScrollView.ScrollToVerticalOffset(targetScroll);
                e.Handled = true;
            }
        }

        private void DescriptionScrollView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_vault.DisplayedIssue != null)
            {
                _vault.DisplayedIssue.DescriptionScrollPosition = e.VerticalOffset;
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && Clipboard.ContainsText())
            {
                string content = Clipboard.GetText().Trim();
                UrlTextbox.Text = content;
                UrlTextCommitted();
                e.Handled = true;
            }
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (_searchWindow == null)
                {
                    _searchWindow = new SearchWindow()
                    {
                        Vault = _vault,
                        Owner = this
                    };
                    _searchWindow.SearchResultSelected += SearchWindow_SearchResultSelected;
                }
                _searchWindow.Display();
                e.Handled = true;
            }

            for (int i = 0; i < 9; i++)
            {
                Key key = (Key)(i + (int)Key.D1);
                if (e.Key == key && HistoryList.Items.Count >= i + 1)
                {
                    HistoryList.SelectedIndex = i;
                }
            }

            if (e.Key == Key.F5 && _vault.DisplayedIssue?.Key != null)
            {
                IssueDisplayRequested(_vault.DisplayedIssue?.Key, true);
            }
        }

        private void SearchWindow_SearchResultSelected(string selectedIssueKey)
        {
            if (!_vault.HasIssueCached(selectedIssueKey)) return;
            _vault.DisplayedIssue = _vault.GetCachedIssue(selectedIssueKey);
            UrlTextbox.Text = selectedIssueKey;
            RefreshDisplayedIssue();
            Focus();
        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (e.AddedItems.Count == 1 && _selectionActive)
            {
                HistoryEntry selected = e.AddedItems[0] as HistoryEntry;
                if (selected.IssueKey != _vault.DisplayedIssue?.Key)
                {
                    UrlTextbox.Text = selected.IssueKey;
                    IssueDisplayRequested(selected.IssueKey);
                }
            }
        }

        private void ConfigureClient()
        {
            ClientCredentials credentials = _vault.JiraApi.Credentials as ClientCredentials;
            credentials.Username = _appConfig.JiraUser;
            credentials.Password = _appConfig.JiraPassword;
            _vault.JiraApi.BaseUri = new Uri(_appConfig.JiraServer);
        }

        private string FormatTime(DateTime? input)
        {
            if (!input.HasValue)
            {
                return "";
            }
            DateTime time = input.Value;
            string timeString = time.ToLocalTime().ToString(Constants.DateTimeUiFormat);
            double daysAgo = DateTime.Now.Subtract(time).TotalDays;
            string daysAgoString = daysAgo <= 1 ? "today" : Math.Floor(daysAgo) + " days ago";
            return timeString + " (" + daysAgoString + ")";
        }

        private void RefreshComments()
        {
            List<Comment> comments = _vault.DisplayedIssue?.Comments;
            if (comments == null || comments.Count == 0)
            {
                CommentPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommentPanel.Visibility = Visibility.Visible;
                CommentList.Items.Clear();
                foreach (Comment comment in comments)
                {
                    CommentDetails details = new CommentDetails
                    {
                        AuthorName = comment.Author?.DisplayName,
                        Body = comment.Body,
                        Created = FormatTime(comment.Created),
                        AuthorIcon = _vault.GetWrappedImage(comment.Author?.AvatarUrl)
                    };
                    CommentList.Items.Add(details);
                }
            }
        }

        public void RefreshDisplayedIssue()
        {
            Stopwatch watch = Stopwatch.StartNew();

            DetailsStatus.Text = _vault.DisplayedIssue?.Status?.Name;
            DetailsStatus.Background = GetStatusColor(_vault.DisplayedIssue?.Status?.Color);
            DetailsPriority.Text = _vault.DisplayedIssue?.Priority?.Name;
            DetailsResolution.Text = _vault.DisplayedIssue?.Resolution;
            DetailsType.Text = _vault.DisplayedIssue?.Type?.Name;
            DetailsAssignee.Text = _vault.DisplayedIssue?.Assignee?.DisplayName;
            DetailsReporter.Text = _vault.DisplayedIssue?.Reporter?.DisplayName;
            DetailsCreated.Text = FormatTime(_vault.DisplayedIssue?.Created);
            DetailsUpdated.Text = FormatTime(_vault.DisplayedIssue?.Updated);

            UpdateImage(TypeImage, _vault.DisplayedIssue?.Type?.IconUrl);
            UpdateImage(AssigneeImage, _vault.DisplayedIssue?.Assignee?.AvatarUrl);
            UpdateImage(ReporterImage, _vault.DisplayedIssue?.Reporter?.AvatarUrl);

            RefreshComments();
            RefreshAttachments();

            SubtaskGroup.Visibility = _vault.DisplayedIssue?.Subtasks?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            SummaryText.Text = _vault.DisplayedIssue?.Summary ?? "- issue summary missing -";
            MarkdownViewer.Markdown = _vault.DisplayedIssue?.Description;
            DescriptionScrollView.ScrollToVerticalOffset(_vault.DisplayedIssue?.DescriptionScrollPosition ?? 0);

            RefreshIssueHistory();

            Logger.Trace("UI refresh done in {0}ms", watch.ElapsedMilliseconds);
        }

        private void RefreshAttachments()
        {
            List<Attachment> attachments = _vault.DisplayedIssue?.Attachments;
            if (attachments == null || attachments.Count == 0)
            {
                AttachmentsGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                AttachmentsGroup.Visibility = Visibility.Visible;
                AttachmentList.Items.Clear();
                foreach (Attachment attachment in attachments)
                {
                    ImageSource source;
                    if (!string.IsNullOrWhiteSpace(attachment.Thumbnail) && _vault.HasImageCached(attachment.Thumbnail))
                    {
                        source = _vault.GetWrappedImage(attachment.Thumbnail);
                    }
                    else
                    {
                        // look at that resource path, who thought that this was a good idea?
                        source = new BitmapImage(new Uri("pack://application:,,,/Images/file.png"));
                    }
                    AttachmentDetails details = new AttachmentDetails
                    {
                        AttachmentName = attachment.FileName,
                        Id = attachment.Id,
                        AttachmentThumbnail = source
                    };
                    AttachmentList.Items.Add(details);
                }
            }
        }

        private void UpdateImage(Image element, string iconUrl)
        {
            if (_vault.HasImageCached(iconUrl))
            {
                element.Source = _vault.GetWrappedImage(iconUrl);
                element.Visibility = Visibility.Visible;
            }
            else
            {
                element.Source = null;
                element.Visibility = Visibility.Collapsed;
            }
        }

        private static Brush GetStatusColor(string color)
        {
            if ("green".Equals(color))
            {
                return Brushes.PaleGreen;
            }
            if ("blue-gray".Equals(color))
            {
                return Brushes.LightBlue;
            }
            if ("yellow".Equals(color))
            {
                return Brushes.PaleGoldenrod;
            }
            return Brushes.Transparent;
        }

        private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show($"URL: {e.Parameter}");
        }

        private void ClickOnImage(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show($"Image: {e.Parameter}");
        }

        private void UrlTextboxKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !string.IsNullOrWhiteSpace(UrlTextbox.Text))
            {
                UrlTextCommitted();
            }
        }

        private void UrlTextCommitted()
        {
            UrlErrorText.Text = "";
            string issueName = UrlTextbox.Text.Trim();
            if (issueName.StartsWith("http"))
            {
                if (!issueName.StartsWith(_appConfig.JiraServer))
                {
                    UrlErrorText.Text = "You can only see issues from your configured server (" + _appConfig.JiraServer + ").";
                    return;
                }
                string[] segments = new Uri(issueName).Segments;
                if (segments.Length == 0)
                {
                    UrlErrorText.Text = "Whatever that is, it's not a valid url to a jira issue.";
                    return;
                }
                issueName = segments[^1];
            }

            if (!Regex.IsMatch(issueName, @"^[^-]+-\d+$"))
            {
                UrlErrorText.Text = "Mate, that does not look like the name of a jira issue.";
                return;
            }
            UrlTextbox.Text = issueName;

            IssueDisplayRequested(issueName);
        }

        private void IssueDisplayRequested(string issueName, bool forceUpdate = false)
        {
            if (_vault.HasIssueCached(issueName))
            {
                // check if the issue has become stale (after 1 day) and refresh it if that is the case
                Issue cachedIssue = _vault.GetCachedIssue(issueName);
                DateTime lastAccessed = cachedIssue.LastAccess;
                if (forceUpdate || DateTime.Now.Subtract(lastAccessed).TotalDays >= 1) //TODO: put amount in config
                {
                    LoadIssue(issueName);
                }
                else
                {
                    _vault.DisplayedIssue = cachedIssue;
                    RefreshDisplayedIssue();
                }
            }
            else
            {
                LoadIssue(issueName);
            }
        }

        private void LoadIssue(string issueName)
        {
            ProgressBar.IsIndeterminate = true;
            StatusText.Text = "Loading issue " + issueName + "...";

            _updateTicker++;
            BackgroundWorker worker = new IssueLoader(_updateTicker);
            worker.DoWork += LoadIssueWorker_DoWork;
            worker.RunWorkerCompleted += LoadIssueWorker_RunWorkerCompleted;
            worker.ProgressChanged += LoadIssueWorker_IntermediateResult;
            worker.RunWorkerAsync(issueName);
        }

        private class IssueLoader : BackgroundWorker
        {
            public readonly int WorkerTick;
            public Exception Error;

            public IssueLoader(int updateTicker)
            {
                WorkerTick = updateTicker;
                WorkerReportsProgress = true;
            }
        }

        void LoadIssueWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string issueId = e.Argument as string;
            if (string.IsNullOrWhiteSpace(issueId))
            {
                return;
            }

            try
            {
                if (_vault.HasIssueCached(issueId))
                {
                    _vault.DisplayedIssue = _vault.GetCachedIssue(issueId);
                    ((BackgroundWorker) sender).ReportProgress(0);
                }
                e.Result = _vault.LoadIssue(issueId);
            }
            catch (Exception ex)
            {
                ((IssueLoader) sender).Error = ex;
                Logger.Error("Unable to load issue {0}: {1}", issueId, ex);
            }
        }

        void LoadIssueWorker_IntermediateResult(object sender, ProgressChangedEventArgs e)
        {
            StatusText.Text = "Checking for updates...";
            RefreshDisplayedIssue();
        }

        void LoadIssueWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (((IssueLoader) sender).WorkerTick != _updateTicker) return;

            Exception error = ((IssueLoader) sender).Error;
            if (error != null)
            {
                UrlErrorText.Text = "Error! " + error.GetType().Name + ": " + error.Message;
            }

            ProgressBar.IsIndeterminate = false;
            StatusText.Text = "";

            if (e.Result is Issue result)
            {
                _vault.DisplayedIssue = result;
                RefreshDisplayedIssue();
            }
        }

        private void SettingsCommand_Executed(object parameter)
        {
            Settings settingsWindow = new Settings
            {
                Owner = this,
                AppConfig = _appConfig
            };
            settingsWindow.ShowDialog();
            ConfigureClient();
        }

        private void BrowserCommand_Executed(object parameter)
        {
            if (string.IsNullOrWhiteSpace(_vault.DisplayedIssue?.Key) || !_appConfig.JiraServer.StartsWith("http"))
            {
                return;
            }
            string url = _appConfig.JiraServer + (_appConfig.JiraServer.EndsWith('/') ? "" : "/") + "browse/" + _vault.DisplayedIssue.Key;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void WatchClipboard()
        {
            while (true)
            {
                if (Clipboard.ContainsText())
                {
                    string content = Clipboard.GetText().Trim();
                    try
                    {
                        string issueName = "";
                        if (content.StartsWith(_appConfig.JiraServer))
                        {
                            string[] segments = new Uri(content).Segments;
                            if (segments.Length > 0)
                            {
                                issueName = segments[^1];
                            }
                        }
                        else if (Regex.IsMatch(content, @"^[^-]+-\d+$"))
                        {
                            issueName = content;
                        }

                        _vault.PrefetchIssue(issueName);
                    }
                    catch (Exception e)
                    {
                        Logger.Trace(e, "Clipboard exception");
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }

    public class DetailEntry
    {
        public string Title { get; set; }
        public string Value { get; set; }
    }

    public class HistoryEntry
    {
        public HistoryEntry(string hotkey, string key, string summary, ImageSource issueTypeImage)
        {
            Hotkey = hotkey;
            IssueKey = key;
            IssueSummary = summary;
            IssueTypeImage = issueTypeImage;
        }

        public string Hotkey { get; }
        public string IssueKey { get; }
        public string IssueSummary { get; }
        public ImageSource IssueTypeImage { get; }
    }

    public class CommentDetails
    {
        public string AuthorName { get; set; }
        public ImageSource AuthorIcon { get; set; }
        public string Created { get; set; }
        public string Body { get; set; }
    }

    public class AttachmentDetails
    {
        public string Id { get; set; }
        public string AttachmentName { get; set; }
        public ImageSource AttachmentThumbnail { get; set; }
    }
}
