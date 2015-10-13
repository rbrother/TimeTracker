using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace KeyLoggerGui {

    public class Project {
        [XmlAttribute] public string Name { get; set; }
        public ObservableCollection<Project> SubProjects { get; set; }

        public Project() {
            SubProjects = new ObservableCollection<Project>();
        }

        public List<String> ProjectNames(string parentName) {
            List<String> names = new List<string>();
            string fullName = (parentName == null ? Name : parentName + "-" + Name);
            names.Add(fullName);
            foreach(Project subProject in SubProjects) {
                names.AddRange(subProject.ProjectNames(fullName));
            }
            return names;
        }

        public override string ToString() {
            return this.Name;
        }
        
    } // class

} // namespace
