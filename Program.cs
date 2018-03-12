using Autofac;
using AutoMapper;
using log4net;
using NHibernate;
using System;
using System.Configuration;
using System.Data;
using System.ServiceProcess;
using System.Threading;
using Zetes.ZStock.DB;
using Zetes.ZStock.DB.Domain;
using Zetes.ZStock.DB.Interface;
using Zetes.ZStock.DB.Repository;
using Zetes.ZStock.Service;
using Zetes.ZStock.Service.OData;
using Zetes.ZStock.Service.Tools;
using System.Data.SqlClient;
using System.Linq;


namespace Zetes.ZStock.Email
{
    class Program : ServiceBase
    {
        public static IContainer AFContainer;
        public const string Named = "Zetes.EmailService";
        public static IQueuedEmailService _queuedEmailService;
        public static IRepository<QueuedEmail> _queuedEmailRepo;
        public static IUnitOfWork _unitOfWork;
   

     

        static Program()
        {
        
            ConfigureAutofac();
             InitApp();
           
            _queuedEmailService = AFContainer.Resolve<IQueuedEmailService>();
            _queuedEmailRepo = AFContainer.Resolve<IRepository<QueuedEmail>>();
      
            _unitOfWork = AFContainer.Resolve<IUnitOfWork>();


        }
        private static void InitApp()
        {
            
            Logger.InitLogger();
         
        }
        public Program()
        {
            this.ServiceName = Named;
    

        }

        static void Main(string[] args)
        {


            try
            {
               
                ServiceBase.Run(new Program());

            }
            catch (Exception e)
            {
                Logger.Log.Error(e);
            }
           
        }

        protected override void OnStart(string[] args)
        {
           
            
            Thread MyThread = new Thread(new ThreadStart(MyThreadStarter));
            MyThread.Start();
            base.OnStart(args);
            string connString = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connString);
            Logger.Log.InfoFormat("Application version {0} is started, working with catalog: {1}", AssemblyInfo.Version, builder.InitialCatalog);
        }

        private void MyThreadStarter()
        {
            SendEmails obj = new SendEmails();
            obj.Run(_queuedEmailRepo, _unitOfWork, _queuedEmailService);
        
        }

        protected override void OnStop()
        {

            base.OnStop();
            Logger.Log.Info("Application is ended");
        }
       

        private static void ConfigureAutofac()
        {
            var builder = new ContainerBuilder();

            //Register repositories
            builder.RegisterGeneric(typeof(NHibernateRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(ManageODataQueries<,>)).As(typeof(IManageODataQueries<,>)).InstancePerLifetimeScope();

            //Register api controllers
            builder.Register(
                item =>
                new NHibernateHelper().SessionFactory).As<ISessionFactory>().InstancePerLifetimeScope();

            builder.Register(
               item =>
               item.Resolve<ISessionFactory>().OpenSession()).As<ISession>().InstancePerLifetimeScope();

            builder.Register(item => new NHibernateUnitOfWork(item.Resolve<ISession>(), IsolationLevel.Snapshot))
                    .As<IUnitOfWork>()
                    .InstancePerLifetimeScope();

            builder.Register(item => Mapper.Engine).As<IMappingEngine>().SingleInstance();

            ServiceRegistration.RegisterServices(builder);
            AFContainer = builder.Build();
        }
    }
}
