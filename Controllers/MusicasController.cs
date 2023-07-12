﻿using System.Runtime.Intrinsics.X86;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using MinisterioLouvor.Interfaces;
using MinisterioLouvor.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MinisterioLouvor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicasController : ControllerBase
    {
        private const string openAiApiKey = "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";

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

        [HttpPost("{link}")]
        public async Task<IActionResult> GetTags(string link)
        {
            string letra = ObterLetraDaMusica(link);
            var tags = await GerarTags(letra);

            return Ok(tags);
        }
        private string ObterLetraDaMusica(string link)
        {
            // Decodifica o link
            string decodedLink = Uri.UnescapeDataString(link);

            // Cria uma instância do HttpClient com a configuração do BaseAddress
            using (var httpClient = new HttpClient { BaseAddress = new Uri("https://www.letras.mus.br/") })
            {
                try
                {
                    // Obtém o conteúdo da página
                    string pagina = httpClient.GetStringAsync(decodedLink).Result;

                    // Carrega o conteúdo da página com o HtmlAgilityPack
                    var documento = new HtmlDocument();
                    documento.LoadHtml(pagina);

                    // Encontra os elementos HTML que contêm a letra da música
                    var elementosLetra = documento.DocumentNode.Descendants("div")
                        .Where(d => d.GetAttributeValue("class", "").Equals("cnt-letra"));

                    if (elementosLetra.Any())
                    {
                        // Cria um StringBuilder para construir a letra da música
                        var stringBuilder = new StringBuilder();

                        foreach (var elemento in elementosLetra)
                        {
                            // Obtém o texto do elemento da letra da música removendo as tags HTML
                            string texto = elemento.InnerHtml;
                            texto = System.Net.WebUtility.HtmlDecode(texto);

                            // Substitui as tags <p> por quebras de linha
                            texto = texto.Replace("<br>", "\n");
                            texto = texto.Replace("</p>", "\n\n");

                            // Remove as tags HTML
                            texto = Regex.Replace(texto, "<.*?>", "");

                            // Remove espaços em branco no início e no final da string
                            texto = texto.Trim();

                            // Adiciona o texto formatado ao StringBuilder
                            stringBuilder.AppendLine(texto);
                        }

                        // Retorna a letra da música
                        return stringBuilder.ToString();
                    }
                    else
                    {
                        throw new Exception("Não foi possível encontrar a letra da música na página.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao obter a letra da música: " + ex.Message);
                }
            }
        }

        private async Task<string[]> GerarTags(string letraMusica)
        {
            string apiUrl = "v1/engines/text-davinci-003/completions";
            string prompt = "Identifique as tags relacionadas ao conteúdo da música a seguir:\n\n" + letraMusica;

            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Add("Authorization", $"Bearer {openAiApiKey}");
                request.Content = new StringContent($"{{ \"prompt\": \"{prompt}\", \"max_tokens\": 64, \"n\": 1 }}", Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient { BaseAddress = new Uri("https://api.openai.com/") })
                {
                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    // Faz o parsing da resposta JSON para obter as tags geradas
                    var jsonObject = JObject.Parse(json);
                    var completions = jsonObject["choices"].First["text"].ToString();
                    var tags = completions.Split(',');

                    // Remove espaços em branco e aspas das tags
                    for (int i = 0; i < tags.Length; i++)
                    {
                        tags[i] = tags[i].Trim().Trim('"');
                    }

                    return tags;
                }
            }
        }


    }
}
