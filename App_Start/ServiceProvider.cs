using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using Ninject;

using SQLExerciser.Models;
using SQLExerciser.Models.DB;

namespace SQLExerciser
{
    public class ServiceProvider : IDependencyResolver
    {
        readonly IKernel kernel = new StandardKernel();

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        public ServiceProvider() => RegisterServices();

        void RegisterServices()
        {
            kernel.Bind<IExercisesContext>().To<ExercisesContext>();
            kernel.Bind<IStorage>().To<Storage>();
            kernel.Bind<IQueryExecutor>().To<QueryExecutor>().InTransientScope();
            kernel.Bind<IQueryTester>().To<QueryTester>();
            kernel.Bind< IExerciser>().To<Exerciser>();
            kernel.Bind<IHasher>().To<Hasher>();
            kernel.Bind<IIdentintyProvider>().To<IdentityProvider>();
            kernel.Bind<IMailService>().To<MailService>();
            kernel.Bind<RoleProvider>().To<Authorizator>();
        }
    }
}