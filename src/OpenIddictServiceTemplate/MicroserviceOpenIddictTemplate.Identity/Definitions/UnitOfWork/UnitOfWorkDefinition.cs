using MicroserviceOpenIddictTemplate.DAL.Database;
using Pepegov.MicroserviceFramerwork.Patterns.Definition;
using Pepegov.MicroserviceFramerwork.Patterns.UnitOfWork;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.UnitOfWork
{
    public class UnitOfWorkDefinition : Definition
    {
        public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddUnitOfWork<ApplicationDbContext>();
        }
    }
}
