using MinisterioLouvor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinisterioLouvor.Interfaces
{
    public interface ICifraRepository : IRepository<Cifra>
    {
        Task<IEnumerable<Cifra>> GetByConteudo(string conteudo);

        Task<Cifra> GetByMusicaId(string musicaId);
    }
}
