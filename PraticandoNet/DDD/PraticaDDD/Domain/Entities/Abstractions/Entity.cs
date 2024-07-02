namespace PraticaDDD.Domain.Entities.Abstractions
{
    public abstract class Entity
    {
        public Guid Id { get; init ; }

        protected Entity( Guid id)
        {
           // Id = Guid.NewGuid();
           Id = id;
        }
    }
}
