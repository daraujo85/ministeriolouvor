using MinisterioLouvor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinisterioLouvor.Interfaces
{
    public interface IMusicaRepository : IRepository<Musica>
    {
        Task<IEnumerable<Musica>> GetByTom(string tom);
        Task<IEnumerable<Musica>> GetByTitulo(string titulo);
        Task<IEnumerable<Musica>> GetByTag(string tag);
    }
}
