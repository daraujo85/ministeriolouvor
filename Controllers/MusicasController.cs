using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using POCMinisterioLouvor.Models;
using System.Threading.Tasks;

namespace POCMinisterioLouvor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusicasController : ControllerBase
    {
        private IMongoCollection<Musica> _musicas;
        private IMongoClient _client;
        private IMongoDatabase _database;        

        public MusicasController()
        {
            _client  = new MongoClient("mongodb+srv://admin:pibvm2020@cluster0.hqtze.mongodb.net/ministeriolouvor?retryWrites=true&w=majority");

            _database = _client.GetDatabase("ministeriolouvor");

            _musicas = _database.GetCollection<Musica>("musicas");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            
            var result = await _musicas.FindAsync(x => true);

            return Ok(result.ToList());
        }

        [HttpGet("Tom/{tom}")]
        public async Task<IActionResult> GetByTom(string tom)
        {
            var result = await _musicas.FindAsync(x => x.Tom == tom.ToUpper());

            return Ok(result.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Musica musica)
        {

            var checkResult = await _musicas.FindAsync(x => x.Titulo == musica.Titulo.Trim());

            if (checkResult.Any())
            {
                return BadRequest($"Não foi possível realizar o cadastro da música {musica.Titulo}, pois ela já existe!");
            }

            await _musicas.InsertOneAsync(musica);


            return StatusCode(201, musica);
        }
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, [FromBody] Musica musica)
        {
            var checkResult = await _musicas.FindAsync(x => x.Id == id);

            if (!checkResult.Any())
            {
                return NotFound($"Não foi possível encontrar a música {musica.Titulo}");
            }

            var result = await _musicas.ReplaceOneAsync(x=> x.Id == id, musica);

            if (result.MatchedCount == 0)
            {
                return BadRequest($"Não foi possível alterar a música {musica.Titulo}");
            }

            return StatusCode(204);
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var checkResult = await _musicas.FindAsync(x => x.Id == id);

            if (!checkResult.Any())
            {
                return NotFound($"Não foi possível encontrar a música com o Id informado {id}");
            }

            var result = await _musicas.DeleteOneAsync(x => x.Id == id);

            if (result.DeletedCount == 0)
            {
                return BadRequest($"Não foi possível deletar a música com o Id informado {id}");
            }

            return StatusCode(204);
        }

    }
}
