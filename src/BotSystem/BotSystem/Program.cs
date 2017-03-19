using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotSystem.Jobs;
using DataContract;
using log4net;
using Quartz;
using Quartz.Impl;

namespace BotSystem {
    class Program {
        static int? GetNextPostId(DataContext db) {
            return db.Database.SqlQuery<int?>($@"UPDATE [Settings]
SET ValueInt = ValueInt + 1
OUTPUT deleted.ValueInt
WHERE Name = 'CurrentPostId'").FirstOrDefault();
        }

        static void Main(string[] args) {
            var a = GCSettings.IsServerGC;
            var b = GCSettings.LatencyMode;
            var c = GCSettings.LargeObjectHeapCompactionMode;
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;

            ILog log = LogManager.GetLogger("Grabber");
//            Console.WriteLine("Press any key to start.");Console.ReadKey();
            int? postId, maxPostId;
            using (var db = new DataContext()) {
                maxPostId = db.Settings.FirstOrDefault(x => x.Name == "MaxPostId").ValueInt;
//                postId = GetNextPostId(db);
            }
            Task task = null;
            //            foreach (var Id in GetEnumerablePosts(maxPostId.Value)) {
            //                try {
            //                    new PostCommentsGrabber().GragPost(Id);
            //                } catch (Exception e) {
            //                    log.Error("Post #" + Id + " not processed.", e);
            //                }
            //            }
            //return;                
            var options = new ParallelOptions {
                MaxDegreeOfParallelism = 1 //Environment.ProcessorCount
            };
            var timer = new Stopwatch();
            timer.Start();
            Parallel.ForEach(GetEnumerablePosts(maxPostId.Value), options, (Id) => {
                try {
                    new PostCommentsGrabber().GragPost(Id);
                } catch (Exception e) {
                    log.Error("Post #" + Id + " not processed.", e);
                }
            });
            timer.Stop();
            Console.WriteLine();
            Console.WriteLine(timer.Elapsed);
            Console.ReadKey();
            return;
            while (maxPostId >= postId) {
                if (task != null)
                    task.Wait();
                
                task = new Task(() => {
                    try {
                        new PostCommentsGrabber().GragPost(postId.Value);
                    }
                    catch (Exception e) {
                        log.Error("Post #" + postId + " not processed.", e);
                    }
                });
                task.Start();
                //                finally {GC.Collect(0);}

                DisplayStatus(postId);

                using (var db = new DataContext()) {
                    postId = GetNextPostId(db);
                }
            }
            task.Wait();

        }

        static IEnumerable<int> GetEnumerablePosts(int maxPostId) {
            int? postId;
            while (true) {
                using (var db = new DataContext()) {
                    postId = GetNextPostId(db);
                }
                DisplayStatus(postId);
                if (maxPostId >= postId)
                    yield return postId.Value;
                else
                    break;
            }
        }

        static void DisplayStatus(int? postId) {
            if (postId % 10 == 0) {
                if (postId % 100 == 0) {
                    Console.WriteLine();
                    Console.Write($"{postId} -> ");
                }
                Console.Write("*");
            }
        }

        static void ProcessLongPost(string[] args) {
            ILog log = LogManager.GetLogger("Grabber");
            var postId = 2;
            using (var db = new DataContext()) {
                try {
                    new PostCommentsGrabber().GragPost(postId);
                }
                catch (Exception e) {
                    log.Error("Post #" + postId + " not processed.", e);
                }

            }
        }


        static void Quartz(string[] args) {
            try {
                NameValueCollection properties = new NameValueCollection();

                properties["quartz.scheduler.instanceName"] = "TestScheduler";
                properties["quartz.scheduler.instanceId"] = "instance_one";
                properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
                properties["quartz.jobStore.useProperties"] = "true";
                properties["quartz.jobStore.dataSource"] = "default";
                properties["quartz.jobStore.tablePrefix"] = "QRTZ_";
                // if running MS SQL Server we need this
                properties["quartz.jobStore.lockHandler.type"] = "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz";

                properties["quartz.dataSource.default.connectionString"] = "Server=.\\sqlexpress;Database=BotSystemDB;Trusted_Connection=True;";
                properties["quartz.dataSource.default.provider"] = "SqlServer-20";

                // First we must get a reference to a scheduler
                ISchedulerFactory sf = new StdSchedulerFactory(properties);
                // Grab the Scheduler instance from the Factory 
                IScheduler scheduler = sf.GetScheduler();
                //                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

                // and start it off
                scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .WithIdentity("job1", "group1")
                    .Build();
                IJobDetail job2 = JobBuilder.Create<NewsChecker>()
                    .WithIdentity("job2", "group1")
                    .Build();
                IJobDetail job3 = JobBuilder.Create<NewsChecker>()//PostCommentsGrabber>()
                    .WithIdentity("job3", "group1")
                    .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever())
                    .Build();
                ITrigger trigger2 = TriggerBuilder.Create()
                    .WithIdentity("trigger2", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(7)
                        .RepeatForever())
                    .Build();
                ITrigger trigger3 = TriggerBuilder.Create()
                    .WithIdentity("trigger3", "group1")
                    .StartNow()
                    //                    .WithSimpleSchedule(x => x
                    //                        .WithIntervalInSeconds(15)
                    //                        .RepeatForever()
                    //                        )
                    .Build();

                // Tell quartz to schedule the job using our trigger
                if (!scheduler.CheckExists(job.Key))
                    scheduler.ScheduleJob(job, trigger);

                if (scheduler.CheckExists(job2.Key))
                    scheduler.DeleteJob(job2.Key);
                scheduler.ScheduleJob(job2, trigger2);
                if (scheduler.CheckExists(job3.Key))
                    scheduler.DeleteJob(job3.Key);
                scheduler.ScheduleJob(job3, trigger3);

                // some sleep to show what's happening
                Thread.Sleep(TimeSpan.FromSeconds(60));

                // and last shut down the scheduler when you are ready to close your program
                scheduler.Shutdown();
            } catch (SchedulerException se) {
                Console.WriteLine(se);
            }

            Console.WriteLine("Press any key to close the application");
            Console.ReadKey();
        }
    }

    public class HelloJob : IJob {
        public void Execute(IJobExecutionContext context) {
            Console.WriteLine("Greetings from HelloJob!");
        }
    }
}
