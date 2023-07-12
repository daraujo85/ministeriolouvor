using System.Runtime.Intrinsics.X86;
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

        [HttpGet("GetTags/{link}")]
        public async Task<IActionResult> GetTags(string link)
        {
            string letra = ObterLetraDaMusica(link);
            var tags = await GerarTags(letra);

            return Ok(tags);
        }
        [HttpGet("GetCifra/{link}")]
        public async Task<IActionResult> GetCifra(string link)
        {
            string cifra = ObterCifraDaMusica(link);

            return Ok(cifra);
        }
        [HttpGet("GetLetra/{link}")]
        public async Task<IActionResult> GetLetra(string link)
        {
            string letra = ObterLetraDaMusica(link);

            return Ok(letra);
        }
        [HttpGet("GetCifraLinks/{titulo}")]
        public async Task<IActionResult> GetCifraLinks(string titulo)
        {
            var links = await ListarResultadosGoogle("cifraclub.com.br", titulo);

            return Ok(links);
        }
        [HttpGet("GetLetraLinks/{titulo}")]
        public async Task<IActionResult> GetLetraLinks(string titulo)
        {
            var links = await ListarResultadosGoogle("letras.mus.br", titulo);

            return Ok(links);
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

        private string ObterCifraDaMusica(string link)
        {
            // Decodifica o link
            string decodedLink = Uri.UnescapeDataString(link);

            // Cria uma instância do HttpClient com a configuração do BaseAddress
            using (var httpClient = new HttpClient { BaseAddress = new Uri("https://www.cifraclub.com.br/") })
            {

                // Obtém o conteúdo da página
                string pagina = httpClient.GetStringAsync(decodedLink).Result;

                // Carrega o conteúdo da página com o HtmlAgilityPack
                var doc = new HtmlDocument();
                doc.LoadHtml(pagina);

                var cifraDiv = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'cifra_cnt')]");

                if (cifraDiv != null)
                {
                    // Encontra o conteúdo dentro das tags <pre></pre> para obter a cifra
                    var preTags = cifraDiv.SelectNodes(".//pre");
                    if (preTags != null && preTags.Count > 0)
                    {
                        var cifra = preTags[0].InnerText.Trim();
                        return cifra;
                    }
                }

                // Caso a div ou a cifra não sejam encontradas, retorna uma string vazia
                return string.Empty;

            }
        }

        private async Task<List<dynamic>> ListarResultadosGoogle(string site, string termo)
        {
            string query = Uri.EscapeDataString(termo);
            string searchUrl = $"search?q={query} site: {site}";


            // Cria uma instância do HttpClient com a configuração do BaseAddress
            using (var httpClient = new HttpClient { BaseAddress = new Uri("https://www.google.com/") })
            {
                // Faz a solicitação HTTP para obter o conteúdo da página de pesquisa do Google
                var response = await httpClient.GetAsync(searchUrl);
                response.EnsureSuccessStatusCode();

                // Lê o conteúdo da resposta HTTP
                var htmlContent = await response.Content.ReadAsStringAsync();

                // Encontra os links e títulos dos resultados de pesquisa usando o HtmlAgilityPack
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var searchResults = new List<dynamic>();

                var resultNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (resultNodes != null)
                {
                    foreach (var resultNode in resultNodes)
                    {
                        var url = resultNode.GetAttributeValue("href", string.Empty);
                        var titleNode = resultNode.SelectSingleNode(".//h3");

                        // Verifica se o link é válido e se o nó do título é encontrado
                        if (!string.IsNullOrEmpty(url) && titleNode != null && url.Contains(site))
                        {
                            var title = titleNode.InnerText.Trim();

                            searchResults.Add(new { Title = title, Url = url.Replace("/url?q=", string.Empty).Replace("&amp", string.Empty).Split(";")?.FirstOrDefault() });
                        }
                    }
                }

                return searchResults;
            }
        }


        private async Task<string[]> GerarTags(string letraMusica)
        {
            string apiUrl = "v1/engines/text-davinci-003/completions";
            string prompt = "Crie tags em português mais abrangentes relacionadas a fé cristã que tenham ligação com a mensagem da música. Que seja uma ou duas palavras por tag que consiga funcionar como uma categoria que agrupara futuras musicas similares, como por exemplo: 'Adoração', ou 'Ceia', 'Batismo', 'Evangelistica', 'Nantal', 'Gratidão', 'Família', 'Páscoa', 'Fidelidade', 'Amizade', 'Amor', 'Mãe', 'Pai', 'Filho', 'Espírito Santo', 'Unção', 'Arrependimento', 'Dependência'. Retorne apenas uma palavra em cada item do array e apenas o array sem nada antes e nem depois. E apenas top 5 das mais relevantes em relacao a letra";
            string formattedText = letraMusica.Replace("\n", " ").Trim(); // Remove quebras de linha e espaços extras

            // Limita o tamanho do texto para reduzir o custo de envio
            const int maxTextLength = 512;
            if (formattedText.Length > maxTextLength)
            {
                formattedText = formattedText.Substring(0, maxTextLength);
            }

            prompt += formattedText;

            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Add("Authorization", $"Bearer {openAiApiKey}");
                request.Content = new StringContent($"{{ \"prompt\": \"{prompt}\", \"max_tokens\": 32, \"n\": 1 }}", Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient { BaseAddress = new Uri("https://api.openai.com/") })
                {
                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to obtain the generated text
                    var jsonObject = JObject.Parse(json);
                    var completions = jsonObject["choices"].First["text"].ToString().Replace(",", string.Empty);

                    // Extract the tags from the generated text
                    var tags = ExtractTags(completions);

                    return tags;
                }
            }
        }

        private string[] ExtractTags(string text)
        {
            // Extract tags enclosed in single quotes
            var tagMatches = Regex.Matches(text, @"'([^']*)'");

            // Convert matches to array of tags
            var tags = tagMatches.Cast<Match>().Select(m => m.Groups[1].Value).ToArray();

            return tags;
        }

    }


}

