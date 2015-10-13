using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

namespace KeyLoggerGui
{
    public class TimeTrackerData
    {
        public static string AppDataFile { get { return AppDataDir + @"\timetrack.xml"; } }

        public static Uri AppDataUri { get { return new Uri("file://" + AppDataFile.Replace('\\', '/')); } }

        public DateTime PrevProgramRunningTime { get; set; } // prev program running time
        public DateTime PrevKeyPressTime { get; set; } // prev key press time
        public double MinimumIdleMinutes { get; set; } 

        public ObservableCollection<Project> Projects { get; set; }

        public ObservableCollection<Activity> Activities { get; set; }

        [XmlIgnore] public Activity CurrentActivity 
        { 
            get 
            {
                if (this.Activities.Count == 0) StartNewActivity();
                return this.Activities.Last();
            } 
        }

        public TimeTrackerData()
        {
            PrevProgramRunningTime = DateTime.Now;
            PrevKeyPressTime = DateTime.Now;
            Activities = new ObservableCollection<Activity>();            
        }

        #region Methods

        [XmlIgnore] 
        public List<String> ProjectNames {
            get {
                List<String> names = new List<string>();
                foreach (Project subProject in Projects) {
                    names.AddRange(subProject.ProjectNames(null));
                }
                return names;
            }
        }

        public void SoftwareRestarted()
        {
            // The fact that there has been not timer elapses for a long time means that
            // computer has been shut down. The end time of current activity remains the previous timer check time.
            this.Activities.Add(new Activity { Type = ActivityType.NotRunning, StartTime = PrevProgramRunningTime, EndTime = DateTime.Now });
            StartNewActivity();
            PrevKeyPressTime = DateTime.Now;
        }

        public void StartIdle()
        {
            // Program has been running, but idle (no keypresses) for long time.
            // Switch current activity to idle (retrospectively)
            CurrentActivity.EndTime = PrevKeyPressTime;
            this.Activities.Add(new Activity { Type = ActivityType.Idle, StartTime = PrevKeyPressTime });
        }

        public void StartNewActivity()
        {
            Activity previousActivity = CurrentActivity;
            Activity newActivity = new Activity();
            this.Activities.Add(newActivity);
            if (previousActivity.DurationMins < 2.0) {
                this.Activities.Remove(previousActivity);
            }
        }

        public void RemoveActivities(System.Collections.IList items) {
            var indexes = new HashSet<int>();
            foreach (Activity activity in items) {
                indexes.Add(this.Activities.IndexOf(activity));
            }
            foreach (int index in indexes.OrderByDescending(i=>i))
            {
                this.Activities.RemoveAt(index);
            }
        }

        public void MergeActivities(List<int> activityIndexes) {
            Activity merged = this.Activities[activityIndexes.First()];
            merged.EndTime = this.Activities[activityIndexes.Last()].EndTime;
            // Iterated thorugh merged items and try to make a reasonable combination of their properties
            foreach (int i in activityIndexes.Reverse<int>()) {
                if (i != activityIndexes.First()) {
                    if (!String.IsNullOrEmpty(this.Activities[i].Project)) {
                        merged.Project = this.Activities[i].Project;
                    }
                    if (this.Activities[i].Type == ActivityType.Work) {
                        // Consider 'work' dominant type, i.e. if there even one 'work' in merged activites, use it.
                        merged.Type = ActivityType.Work;
                    }
                    if (!String.IsNullOrEmpty(this.Activities[i].Description)) {
                        merged.Description = this.Activities[i].Description;
                    }
                    this.Activities.RemoveAt(i);
                }
            }
        }

        private static DirectoryInfo AppDataDir {
            get {
                var url = Assembly.GetCallingAssembly( ).CodeBase;
                var exeFile = url.Replace( "file:///", "" ).Replace( "/", @"\" );
                return new FileInfo( exeFile ).Directory;
            }
        }

        public static TimeTrackerData Load()
        {
            XmlSerializer ser = new XmlSerializer(typeof(TimeTrackerData));
            using (TextReader reader = new StreamReader(AppDataFile))
            {
                return (TimeTrackerData)ser.Deserialize(reader);                
            }
        }

        public void Save()
        {
            if (File.Exists(AppDataFile)) File.Copy(AppDataFile, AppDataFile + ".bak1", true);            
            XmlSerializer ser = new XmlSerializer(typeof(TimeTrackerData));            
            using (TextWriter writer = new StreamWriter(AppDataFile, false))
            {
                ser.Serialize(writer, this);
            }
            File.Copy(AppDataFile, AppDataFile + ".bak2", true);
        }

        #endregion

    }
}
