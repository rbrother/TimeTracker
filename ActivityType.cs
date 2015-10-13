using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyLoggerGui {

    public enum ActivityType {
        Work,
        Personal,
        Unknown,
        NotRunning,
        Idle,
    }

    public static class ActivityTypeUtils {
        public static bool IsDefined(this ActivityType activity) {
            return (activity == ActivityType.Work || activity == ActivityType.Personal);
        }
    }

} // namespace
