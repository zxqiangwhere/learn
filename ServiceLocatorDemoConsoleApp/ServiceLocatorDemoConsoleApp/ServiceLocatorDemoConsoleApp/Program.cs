using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLocatorDemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceLocator.RegisterType<IService, ServiceA>();
            IService service = ServiceLocator.Resolve<IService>();
            Console.WriteLine(service.GetName());
            IService servicea = ServiceLocator.Resolve<IService>();
            Console.WriteLine(servicea.GetName());
            Console.Read();
        }
    }

    public interface IService
    {
        string GetName();
    }
    public class ServiceA : IService
    {
        public string GetName()
        {
            return "ServiceA";
        }
    }
}
