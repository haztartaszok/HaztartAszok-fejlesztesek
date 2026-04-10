using System;
using DotNetNuke.Services.Scheduling;
using ShoppedTogetherHaztartasok.Dnn.Services;

namespace ShoppedTogetherHaztartasok.Dnn.Components
{
    public class ShoppedTogetherSyncScheduler : SchedulerClient
    {
        public ShoppedTogetherSyncScheduler(ScheduleHistoryItem item)
        {
            ScheduleHistoryItem = item;
        }

        public override void DoWork()
        {
            try
            {
                Progressing();

                ScheduleHistoryItem.AddLogNote("ShoppedTogether scheduler indul.");

                var service = new ShoppedTogetherService();
                service.RunIncrementalSync();

                ScheduleHistoryItem.AddLogNote("RunIncrementalSync lefutott.");
                ScheduleHistoryItem.Succeeded = true;
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("Hiba: " + ex.ToString());
                Errored(ref ex);
            }
        }
    }
}