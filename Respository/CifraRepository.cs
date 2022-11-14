using MongoDB.Driver;
using MinisterioLouvor.Interfaces;
using MinisterioLouvor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MinisterioLouvor.Respository
{
    public class CifraRepository : BaseRepository<Cifra>, ICifraRepository
    {
        public CifraRepository(IMongoContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Cifra>> GetByConteudo(string conteudo)
        {
            var data = await GetByFilter(x => x.Conteudo.ToLower().Contains(conteudo.ToLower()));

            return data;
        }

        public async Task<Cifra> GetByMusicaId(Guid musicaId)
        {
            var data = await GetByFilter(x => x.MusicaId == musicaId.ToString());

            return data?.FirstOrDefault();
        }
    }
}
