namespace Core;

interface IGenericRepository<T>
{
    Task<T?> GetById(int id, CancellationToken ctx);
    Task<IEnumerable<T>> GetAll(CancellationToken ctx);
    Task<bool> Add(T entity, CancellationToken ctx);
    Task<bool> Update(T entity, CancellationToken ctx);
    Task<bool> Delete(T entity, CancellationToken ctx);
}