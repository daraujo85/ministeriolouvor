using MongoDB.Driver;
using MinisterioLouvor.Interfaces;
using MinisterioLouvor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinisterioLouvor.Respository
{
    public class MusicaRepository : BaseRepository<Musica>, IMusicaRepository
    {
        public MusicaRepository(IMongoContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Musica>> GetByTom(string tom)
        {
            var data = await GetByFilter(x=> x.Tom == tom.ToUpper());

            return data;
        }
        public async Task<IEnumerable<Musica>> GetByTitulo(string titulo)
        {
            var data = await GetByFilter(x => x.Titulo.ToLower().Contains(titulo.ToLower()));

            return data;
        }

        public async Task<IEnumerable<Musica>> GetByTag(string tag)
        {
            var data = await GetByFilter(x => x.Tags.Any(t=>t.ToLower().Contains(tag)));

            return data;
        }
    }
}
