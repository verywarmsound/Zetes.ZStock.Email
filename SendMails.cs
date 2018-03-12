using System.Linq;
using Zetes.ZStock.Service.Tools;
using Zetes.ZStock.DB.Domain;
using Zetes.ZStock.DB.Interface;
using Zetes.ZStock.DB.Repository;
using System.Configuration;
using System.Threading;
using System;
using System.Diagnostics;
using log4net;

namespace Zetes.ZStock.Email
{
    class SendEmails
    {
        /// <summary>
        /// Runs the sending email process.
        /// </summary>
        /// <param name="_queuedEmailRepo">The queued email repo.</param>
        /// <param name="_unitOfWork">The unit of work.</param>
        /// <param name="_queuedEmailService">The queued email service.</param>

        public void Run(IRepository<QueuedEmail> _queuedEmailRepo, IUnitOfWork _unitOfWork, IQueuedEmailService _queuedEmailService)
        {
            int interval = int.Parse(ConfigurationManager.AppSettings["interval"] + "") * 1000;

            Stopwatch watch = new Stopwatch();
          
            while (true)
            {
                watch.Start();
                _unitOfWork.Start();
               Send(_queuedEmailRepo, _unitOfWork, _queuedEmailService);
                _unitOfWork.Commit();
                watch.Stop();      
                Thread.Sleep(interval);

            }
        }

        private static void Send(IRepository<QueuedEmail> _queuedEmailRepo, IUnitOfWork _unitOfWork,
            IQueuedEmailService _queuedEmailService)
        {
            var qes = _queuedEmailRepo.FilterBy(k => k.DateSent == null).ToList();

            if (Logger.Log.IsDebugEnabled == true && qes.Count == 0)
            {
                Logger.Log.DebugFormat("there is no emails in the queue");

                }
           else if (qes.Count > 0)
            {
                Logger.Log.InfoFormat("found emails in the queue: {0}", qes.Count);
            }
           
            foreach (var qe in qes)
            {
                try
                {
                    _queuedEmailService.SendMail(qe.ID);
                    Logger.Log.InfoFormat("Sent email to: {0}, Customer ID: {1}, with OriginalDocument: {2}", qe.To, qe.Customer.ID, qe.OriginalDocument);
                }
                catch (Exception e)
                {
                    Logger.Log.Error(e);
                }
            }
           
        }
    }
}
