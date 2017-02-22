using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotSystem.Jobs;
using DataContract;
using log4net;

namespace PerformanceTesting {
    class Program {
        static void Main(string[] args) {
            ILog log = LogManager.GetLogger("Grabber");
            var postId = 4820359;
            using (var db = new DataContext()) {
                try {
                    new PostCommentsGrabber().GragPost(postId);
                } catch (Exception e) {
                    log.Error("Post #" + postId + " not processed.", e);
                }

            }

        }
    }
}
