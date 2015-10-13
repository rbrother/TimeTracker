using System;
using System.IO;
using System.Diagnostics;
using System.Timers;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Saxon.Api;

namespace KeyLoggerGui
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// 
    /// TODO:
    /// - Adding previous activities manually. (eg adding for some book reading when not using computer)
    /// </summary>
    public partial class TimeTrackerWindow : Window
    {        
        private Timer checkTimer;
        bool trackerRestarted = false;
        uint keyStrokes = 0;
        DateTime lastSaveTime = DateTime.Now;
        TimeTrackerData data;

        public TimeTrackerWindow() {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(TimeTrackerWindow_Loaded);
        }

        void TimeTrackerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Title = "GoodTimeTracker by RJB " + this.GetType().Assembly.GetName().Version.ToString();
                foreach (ActivityType activityType in Enum.GetValues(typeof(ActivityType)))
                {
                    this.activityType.Items.Add(activityType);
                }
                this.data = TimeTrackerData.Load();
                try
                {
                    KeyLoggerHook.SetKBHook();
                }
                catch (BadImageFormatException ex)
                {
                    // We are in 64-bit OS, cannot run 32-bit DLL
                    MessageBox.Show("Failed to load kbhook.dll. Copy kbhook32.dll or kbhook64.dll to kbhook.dll depending on your OS");
                    throw;
                }
                InitializeTimer();
                this.events.ItemsSource = data.Activities;
                this.data.Activities.CollectionChanged += new NotifyCollectionChangedEventHandler(Activities_CollectionChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void Activities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.events.ScrollIntoView(e.NewItems[0]);
                this.events.SelectedItem = e.NewItems[0];
            }
        }

        private void InitializeTimer()
        {
            this.checkTimer = new Timer(1*1000);
            this.checkTimer.Elapsed += new ElapsedEventHandler(checkTimer_Elapsed);
            this.checkTimer.Start();
        }

        private void checkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SimpleDelegate(CheckStatus));
        }

        private delegate void SimpleDelegate();

        /// <summary>
        /// This is run periodically in UI thread, so GUI can be accessed
        /// </summary>
        private void CheckStatus()
        {
            TimeSpan fromPrevCheck = DateTime.Now - data.PrevProgramRunningTime;
            if (this.trackerRestarted) {
                data.SoftwareRestarted();
                Notification("Tracker restarted - please specify activities during shut-down time");
                this.trackerRestarted = false;
            }
            else {
                if (fromPrevCheck.TotalSeconds > 5.0) {
                    this.trackerRestarted = true;
                    return; // give some delay after startup to prevent screen errors
                }
            }
            TimeSpan fromPrevActivity = DateTime.Now - data.PrevKeyPressTime;
            if (data.CurrentActivity.Type != ActivityType.Idle && fromPrevActivity.TotalMinutes > data.MinimumIdleMinutes)
            {
                data.StartIdle();
            }
            if (KeyLoggerHook.GetKeyStrokes() > this.keyStrokes)
            {
                ActivityOccurring();
            }
            data.CurrentActivity.EndTime = DateTime.Now;
            data.PrevProgramRunningTime = DateTime.Now;
            this.prevKeyStrokeLabel.Content = fromPrevActivity.ToString().Substring(0,8);
            SaveIfNeeded();
        }

        private void SaveIfNeeded()
        {
            TimeSpan fromLastSave = DateTime.Now - this.lastSaveTime;
            if (fromLastSave.TotalMinutes > 1.0)
            {
                data.Save();
                this.lastSaveTime = DateTime.Now;
            }
        }

        private void ActivityOccurring()
        {
            if (data.CurrentActivity.Type == ActivityType.Idle)
            {
                // End idle when activity starts again
                data.StartNewActivity(); 
                Notification("Activity resumed after idle period - please specify current activity");
            }
            this.keyStrokes = KeyLoggerHook.GetKeyStrokes();
            data.PrevKeyPressTime = DateTime.Now;
        }

        private void Notification(string request)
        {
            // Bring users attention to this window so that he can specify details of the idle time and new activity
            this.Topmost = true;
            if (this.WindowState == WindowState.Minimized) this.WindowState = WindowState.Maximized;
            this.notification.Content = request;
            this.notificationPanel.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //KeyLoggerHook.KillKBHook();
            this.data.Save();
        }

        private void dismissNotificationButton_Click(object sender, RoutedEventArgs e)
        {
            this.notificationPanel.Visibility = Visibility.Collapsed;
            this.Topmost = false;
        }

        private void deleteActivity_Click(object sender, RoutedEventArgs e) {
            // TODO: Selected items might not be in order! Still indexes must be processed in order!
            this.data.RemoveActivities(this.events.SelectedItems);
        }

        /// <summary>
        /// Returns indexes of selected activities in increasing order
        /// </summary>
        private List<int> SelectedActivityIndexes {
            get {
                List<int> indexes = new List<int>();
                foreach(Activity selectedActivity in this.events.SelectedItems) {
                    indexes.Add(this.events.Items.IndexOf(selectedActivity));
                }
                indexes.Sort();
                return indexes;
            }
        }

        private void mergeSelected_Click(object sender, RoutedEventArgs e) {
            List<int> selectedIndexes = this.SelectedActivityIndexes;
            if (selectedIndexes.Count < 2) {
                Notification("Selected at least 2 consequent activities to merge");
                return;
            }
            Trace.WriteLine("Items for merge: " + selectedIndexes.First() + ", " + selectedIndexes.Last());
            if (selectedIndexes.Last() - selectedIndexes.First() + 1 != selectedIndexes.Count) {
                Notification("Selected items to merge must be consequent");
                return;
            }
            data.MergeActivities(selectedIndexes);
        }

        private void startActivity_Click(object sender, RoutedEventArgs e) {
            if (data.CurrentActivity.Type == ActivityType.Idle) {
                ActivityOccurring();
            }
            else {
                data.StartNewActivity();
                data.PrevKeyPressTime = DateTime.Now;
            }
        }

        private void events_KeyUp(object sender, KeyEventArgs e) {
            try
            {
                switch (e.Key)
                {
                    case Key.Delete: this.data.RemoveActivities(this.events.SelectedItems); break;
                    case Key.Insert: continueActivity_Click(null, null); break;
                }
            }
            catch (Exception ex)
            {
                Notification("KeyUp failed: " + ex.Message);
            }
        }

        private void continueActivity_Click(object sender, RoutedEventArgs e) 
        {
            Activity selected = (Activity)this.events.SelectedItem;
            data.StartNewActivity();
            data.CurrentActivity.CopyValuesFrom(selected);
            data.PrevKeyPressTime = DateTime.Now;            
        }

        public string AppDir
        {
            get { return new FileInfo(new Uri(this.GetType().Assembly.CodeBase).LocalPath).Directory.FullName; }
        }

        private static Processor XsltProcessor = new Processor(false);

        private XsltTransformer _reportTransformer;
        private XsltTransformer ReportTransformer
        {
            get
            {
                if (_reportTransformer == null)
                {
                    XsltCompiler compiler = XsltProcessor.NewXsltCompiler();
                    string xsltFile = (AppDir + @"/data-to-xhtml.xslt").Replace('\\', '/');
                    XsltExecutable exec = compiler.Compile(new Uri("file://" + xsltFile));
                    _reportTransformer = exec.Load();
                }
                return _reportTransformer;
            }
        }

        private void generateReport_Click(object sender, RoutedEventArgs e)
        {
            data.Save();           
            DocumentBuilder builder = XsltProcessor.NewDocumentBuilder();
            XdmNode sourceData = builder.Build(TimeTrackerData.AppDataUri);
            ReportTransformer.InitialContextNode = sourceData;
            string reportFile = AppDir + "/report.html";
            ReportTransformer.Run(new TextWriterDestination(new System.Xml.XmlTextWriter(reportFile, Encoding.UTF8)));
            Process.Start("file://" + reportFile);
        }

        private void archiveSelected_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented...");

        }

        private void projectConfig_Click(object sender, RoutedEventArgs e)
        {
            data.Save();
            Process.Start(TimeTrackerData.AppDataFile);
            this.Close();
        }

        
    } // class

} // namespace
