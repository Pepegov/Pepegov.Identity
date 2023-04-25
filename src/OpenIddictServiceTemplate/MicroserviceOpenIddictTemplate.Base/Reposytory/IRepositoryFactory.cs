namespace MicroserviceOpenIddictTemplate.Identity.Base.Reposytory
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class;
    }
}