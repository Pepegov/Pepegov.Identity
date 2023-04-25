using MicroserviceOpenIddictTemplate.DAL.Database;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;
using MicroserviceOpenIddictTemplate.Identity.Base.UnitOfWork;

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
