using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace KeyLoggerGui {
    public class Activity : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ActivityType type;
        public ActivityType Type {
            get { return this.type; }
            set {
                this.type = value;
                NotifyPropertyChanged("Type");
                NotifyPropertyChanged("Project");
                NotifyPropertyChanged("Description");
            }
        }

        private string project;
        public string Project {
            get {
                if (this.Type == ActivityType.Work) { return this.project; }
                else { return null; }
            }
            set {
                this.project = value;
                NotifyPropertyChanged("Project");
            }
        }

        private string description;
        public string Description {
            get {
                if (this.Type == ActivityType.Work) { return this.description; }
                else { return null; }
            }
            set {
                this.description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public string Day { get { return StartTime.ToShortDateString() + " " + StartTime.DayOfWeek.ToString(); } }

        private DateTime startTime;
        public DateTime StartTime {
            get { return this.startTime; }
            set {
                this.startTime = value;
                NotifyPropertyChanged("StartTime");
                NotifyPropertyChanged("StartTimeStr");
                NotifyPropertyChanged("Duration");
            }
        }

        public string StartTimeStr { get { return StartTime.ToLongTimeString(); } }

        private DateTime endTime;
        public DateTime EndTime {
            get { return this.endTime; }
            set {
                this.endTime = value;
                NotifyPropertyChanged("EndTime");
                NotifyPropertyChanged("EndTimeStr");
                NotifyPropertyChanged("Duration");
            }
        }

        public string EndTimeStr { get { return EndTime.ToLongTimeString(); } }

        public double DurationMins { get { return (EndTime - StartTime).TotalMinutes; } }

        public String Duration { get { return (EndTime - StartTime).ToString().Substring(0, 8); } }



        public Activity() {
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            this.Type = ActivityType.Unknown;
            this.Project = "";
        }

        public void CopyValuesFrom(Activity other) {
            Type = other.type;
            Project = other.project;
            Description = other.Description;
        }

    } // class

} // namespace
