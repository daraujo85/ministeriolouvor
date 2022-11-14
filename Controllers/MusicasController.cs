using Microsoft.AspNetCore.Mvc;
using MinisterioLouvor.Interfaces;
using MinisterioLouvor.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MinisterioLouvor.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MusicasController : ControllerBase
  {
    private readonly IMusicaRepository _musicaRepository;

    public MusicasController(IMusicaRepository musicaRepository)
    {
      _musicaRepository = musicaRepository;
    }

    [HttpGet("GetYoutubeInfo/{titulo}")]
    public async Task<IActionResult> GetYoutubeInfo(string titulo)
    {
      var result = await _musicaRepository.YoutubeSearch(titulo);

      if (result.Any())
      {
        return Ok(result);
      }

      return BadRequest("Não foi possível encontrar nenhum vídeo");

    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {

      var result = await _musicaRepository.GetAll();

      return Ok(result);
    }

    [HttpGet("Tom/{tom}")]
    public async Task<IActionResult> GetByTom(string tom)
    {
      var result = await _musicaRepository.GetByTom(tom.ToUpper());

      return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
      var result = await _musicaRepository.GetById(id);

      return Ok(result);
    }
    /// <summary>
    /// Método de busca que verifica se encontra alguma música que contenha pelo menos uma parte do titulo
    /// </summary>
    /// <param name="titulo"></param>
    /// <returns></returns>
    [HttpGet("Titulo/{titulo}")]
    public async Task<IActionResult> GetByTitulo(string titulo)
    {
      var result = await _musicaRepository.GetByTitulo(titulo);

      return Ok(result);
    }

    /// <summary>
    /// Método de busca que verifica se encontra alguma música que contenha pelo menos uma parte de alguma tag
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    [HttpGet("Tags/{tag}")]
    public async Task<IActionResult> GetByTag(string tag)
    {
      var result = await _musicaRepository.GetByTag(tag);

      return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Musica musica)
    {

      var checkResult = await _musicaRepository.GetByTitulo(musica.Titulo.Trim());

      if (checkResult.Any())
      {
        return BadRequest($"Não foi possível realizar o cadastro da música {musica.Titulo}, pois ela já existe!");
      }

      await _musicaRepository.AddAsync(musica);


      return StatusCode(201, musica);
    }
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Put(string id, [FromBody] Musica musica)
    {

      var checkResult = await _musicaRepository.GetById(id);

      if (checkResult == null)
      {
        return NotFound($"Não foi possível encontrar a música {musica.Titulo}");
      }

      _musicaRepository.Update(id, musica);

      //if (result.MatchedCount == 0)
      //{
      //    return BadRequest($"Não foi possível alterar a música {musica.Titulo}");
      //}

      return StatusCode(204);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      var checkResult = await _musicaRepository.GetById(id);

      if (checkResult == null)
      {
        return NotFound($"Não foi possível encontrar a música com o Id informado {id}");
      }

      _musicaRepository.Remove(id);

      //if (result.DeletedCount == 0)
      //{
      //    return BadRequest($"Não foi possível deletar a música com o Id informado {id}");
      //}

      return StatusCode(204);
    }

  }
}
