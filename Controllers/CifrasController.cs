using Microsoft.AspNetCore.Mvc;
using MinisterioLouvor.Interfaces;
using MinisterioLouvor.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MinisterioLouvor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CifrasController : ControllerBase
    {
        private readonly ICifraRepository _cifraRepository;
        private readonly IMusicaRepository _musicaRepository;

        public CifrasController(ICifraRepository cifraRepository, IMusicaRepository musicaRepository)
        {
            _cifraRepository = cifraRepository;
            _musicaRepository = musicaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            
            var result = await _cifraRepository.GetAll();

            return Ok(result);
        }

        [HttpGet("Conteudo/{conteudo}")]
        public async Task<IActionResult> GetByConteudo(string conteudo)
        {
            var result = await _cifraRepository.GetByConteudo(conteudo);

            if (result.Count() == 0)
            {
                return NotFound($"Não foi possível encontrar nenhuma música que contenha [{conteudo}]");
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cifra cifra)
        {
            var guid = new Guid(cifra.MusicaId);

            var checMusicakResult = await _musicaRepository.GetById(guid);

            if (checMusicakResult == null)
            {
                return NotFound($"A música informada não existe");
            }

            var checCifrakResult = await _cifraRepository.GetByMusicaId(guid);

            if (checCifrakResult != null)
            {
                return NotFound($"Não foi possível realizar o cadastro da cifra, pois ela já existe!");
            }

            await _cifraRepository.AddAsync(cifra);

            return StatusCode(201, cifra);
        }
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, [FromBody] Cifra cifra)
        {
            var guid = new Guid(id);

            var musicaId = new Guid(id);

            var checMusicakResult = await _musicaRepository.GetById(musicaId);

            if (checMusicakResult == null)
            {
                return NotFound($"A música informada não existe");
            }

            var checkResult = await _cifraRepository.GetById(guid);

            if (checkResult == null)
            {
                return NotFound($"Não foi possível encontrar a cifra");
            }

            _cifraRepository.Update(cifra);

            //if (result.MatchedCount == 0)
            //{
            //    return BadRequest($"Não foi possível alterar a música {musica.Titulo}");
            //}

            return StatusCode(204);
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var guid = new Guid(id);

            var checkResult = await _cifraRepository.GetById(guid);

            if (checkResult == null)
            {
                return NotFound($"Não foi possível encontrar a cifra com o Id informado {id}");
            }

            _musicaRepository.Remove(guid);

            //if (result.DeletedCount == 0)
            //{
            //    return BadRequest($"Não foi possível deletar a música com o Id informado {id}");
            //}

            return StatusCode(204);
        }
    }
}
