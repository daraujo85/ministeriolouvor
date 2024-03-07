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

        [HttpGet("GetYoutubeInfoV2/{titulo}")]
        public async Task<IActionResult> GetYoutubeInfoV2(string titulo)
        {
            var links = await ListarResultadosGoogle("youtube.com", titulo);

            var result = links.Select(x=> new Video{
              Id = GetVideoIdFromUrl(x.Url),
              Titulo = x.Title,
              Descricao = x.Title,
              LargeThumbnail = $"https://img.youtube.com/vi/{GetVideoIdFromUrl(x.Url)}/0.jpg",
              SmallThumbnail = $"https://i.ytimg.com/vi/{GetVideoIdFromUrl(x.Url)}/default.jpg",
              Url = $"https://www.youtube.com/watch?v={GetVideoIdFromUrl(x.Url)}"
            });

            return Ok(result);

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

        [HttpGet("GetTagsWithGPT/{link}")]
        public async Task<IActionResult> GetTagsWithGPT(string link)
        {
            string letra = ObterLetraDaMusica(link);
            var tags = await GerarTags(letra);

            return Ok(tags);
        }
        [HttpGet("GetTags/{link}")]
        public async Task<IActionResult> GetTags(string link)
        {
            string letra = ObterLetraDaMusica(link);
            var tags = IdentifyTags(letra);

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
        [HttpGet("GetResults/{site}/{titulo}")]
        public async Task<IActionResult> GetResults(string site, string titulo)
        {
            var links = await ListarResultadosGoogle(site, titulo);

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
                        .Where(d => d.GetAttributeValue("class", "").Equals("lyric-original"));

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

                            var decodedUrl = Uri.UnescapeDataString(url).Replace("/url?q=", string.Empty).Replace("&amp", string.Empty).Split(";")?.FirstOrDefault();
                            
                            
                            searchResults.Add(new { Title = title, Url = decodedUrl });
                        }
                    }
                }

                return searchResults;
            }
        }

        private string GetVideoIdFromUrl(string url)
        {
            var queryStartIndex = url.IndexOf("youtube.com/watch?v=");
            if (queryStartIndex != -1)
            {
                var videoIdStartIndex = queryStartIndex + "youtube.com/watch?v=".Length;
                var videoIdEndIndex = url.IndexOf('&', videoIdStartIndex);
                if (videoIdEndIndex == -1)
                    videoIdEndIndex = url.Length;

                var videoIdLength = videoIdEndIndex - videoIdStartIndex;
                if (videoIdLength > 0)
                    return url.Substring(videoIdStartIndex, videoIdLength);
            }

            return null;
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

        private readonly Dictionary<string, List<string>> TagSynonyms = new Dictionary<string, List<string>>
        {
            { "Natal", new List<string> {
        "Natal",
        "Nascimento",
        "Jesus",
        "Cristo",
        "Presépio",
        "Estrela",
        "Belém",
        "Manjedoura",
        "Anunciação",
        "Noite",
        "Paz",
        "Renascimento",
        "Presentes",
        "Família",
        "Comemoração",
        "menino",
        "Celebração",
        "Alegria",
        "Amor",
        "Paz",
        "Harmonia",
        "Solidariedade",
        "Esperança",
        "Luz",
        "Criança",
        "Sagrado",
        "Milagre",
        "Fé",
        "Gratidão",
        "Reconciliação",
        "Reencontro",
        "Bondade",
        "Perdão",
        "Compaixão",
        "Generosidade",
        "Renovação",
        "Cantata",
        "NFamília",
        "Confraternização",
        "Encontro",
        "Anjos",
        "Cantam",
        "Estrela Guia",
        "Aurora",
        "Feliz",
        "Sinos",
        "Magia",
        "Festividade",
        "Esperança Renovada",
        "Redentor",
        "Milagre",
        "Agradecer",
        "maravilhoso",
        "príncipe",
        "Emanuel",
        "Compartilhar",
        "Amar"
    } },
            { "Páscoa", new List<string> {
        "Páscoa",
        "Ressurreição",
        "Cruz",
        "Jesus Cristo",
        "Crucificação",
        "Salvação",
        "Redenção",
        "Vida eterna",
        "Renascimento",
        "Morte e Ressurreição",
        "Sepultura vazia",
        "Túmulo vazio",
        "Sangue de Cristo",
        "Sacrifício",
        "Amor",
        "Perdão",
        "Graça",
        "Fé",
        "Esperança",
        "Vitória",
        "Aleluia",
        "Cordeiro de Deus",
        "O Cordeiro pascal",
        "Renovação",
        "Caminho para o Céu",
        "Cristo vive",
        "Vida nova",
        "Reconciliação",
        "Comunhão",
        "Ressuscitou",
        "Triunfo sobre a morte",
        "Alegria",
        "Cantos Pascal",
        "Pão e Vinho",
        "Ceia do Senhor",
        "Memorial",
        "Unção",
        "Santo Sepulcro",
        "Amanhecer de Páscoa",
        "Hosana",
        "Encontro com o Ressuscitado",
        "Novo começo",
        "Poder da Cruz",
        "Páscoa Eterna",
        "Nascer de novo",
        "Paz",
        "Esperança renovada",
        "Promessa cumprida",
        "Triunfo da vida",
        "Libertação",
        "Vida abundante"
    } },
            { "Crucificação", new List<string> {
        "Crucificação",
        "Cruz",
        "Jesus Cristo",
        "Sacrifício",
        "Sofrimento",
        "Dor",
        "Amor",
        "Perdão",
        "Redenção",
        "Graça",
        "Salvação",
        "Cristo na cruz",
        "Martírio",
        "Paixão",
        "Morte",
        "Ressurreição",
        "Arrependimento",
        "Misericórdia",
        "Compaixão",
        "Piedade",
        "Armação",
        "Lenho",
        "Cordeiro de Deus",
        "Sangue de Cristo",
        "Caminho para o Céu",
        "Sacrifício supremo",
        "incondicional",
        "Expiação",
        "Substituição",
        "Prego",
        "Coração partido",
        "Dívida paga",
        "Doloroso",
        "Paciência",
        "Piedade",
        "Santo",
        "Justiça",
        "Redentor",
        "Santo Cordeiro",
        "Imolação",
        "Ressurreição de Jesus",
        "Prova de amor",
        "Caminho da salvação",
        "Perfeição",
        "Sofrer com Cristo",
        "Glória na cruz",
        "Sacrifício redentor",
        "Pecado expiado",
        "Sangue derramado"
    } },
            { "Batismo", new List<string>   {
        "Batismo",
        "Batizar",
        "Águas",
        "Ritual",
        "Sacramento",
        "Imersão",
        "Renascimento",
        "Arrependimento",
        "Confissão",
        "Simbolismo",
        "Lavagem",
        "Redenção",
        "Graça",
        "Novo nascimento",
        "Mergulho",
        "Consagração",
        "Compromisso",
        "Renovação",
        "Transformação",
        "Aliança",
        "Cristão",
        "Seguimento de Jesus",
        "Iniciação",
        "Separação",
        "Consagração",
        "Novo começo",
        "Unidade",
        "Arrependimento",
        "Perdão",
        "Comunidade",
        "Renovação de vida",
        "Testemunho",
        "Identificação com Cristo",
        "Adoção",
        "Fé pública",
        "Compromisso com Deus",
        "Símbolo de fé",
        "Compromisso eterno",
        "Aceitação",
        "Nascer de novo",
        "Filiação divina",
        "Liberdade",
        "Ressurreição com Cristo",
        "Mudança de vida",
        "Rito sagrado",
        "Discipulado",
        "Revestimento espiritual",
        "Purificação",
        "Conversão",
        "União com Cristo",
        "Morte para o pecado",
        "Vida em Cristo"
    } },
            { "Arrependimento", new List<string>   {
        "Arrependimento",
        "Confissão",
        "Perdão",
        "Mudança de vida",
        "Renovação",
        "Retorno",
        "Contrição",
        "Compunção",
        "Conversão",
        "Reconciliação",
        "Remorso",
        "Retratação",
        "Pesar",
        "Reparação",
        "Restauração",
        "Reflexão",
        "Despertar",
        "Humildade",
        "Correção",
        "Expiação",
        "Remissão",
        "Resgate",
        "Emenda",
        "Renascimento",
        "Reformação",
        "Regeneração",
        "Redenção",
        "Consciência",
        "Consciencialização",
        "Transformação",
        "Renovar",
        "Modificar",
        "Reavaliar",
        "Reconsiderar",
        "Rever",
        "Comover",
        "Sinceridade",
        "Compromisso",
        "Novo começo",
        "Novo caminho",
        "Recorrer",
        "Reverter",
        "Reparar",
        "Refazer",
        "Reajustar",
        "Reaprender",
        "Reestruturar",
        "Reavivar",
        "Recuperar",
        "Retomar",
        "Reorganizar"
    } },
            { "Evangelismo", new List<string>    {
        "Evangelismo",
        "Evangelizar",
        "Proclamar",
        "Testemunhar",
        "Pregar",
        "Anunciar",
        "Divulgar",
        "Propagar",
        "Difundir",
        "Compartilhar",
        "Ensinar",
        "Espalhar",
        "Alcançar",
        "Conquistar",
        "Converter",
        "Convencer",
        "Missionário",
        "Missão",
        "Mensagem",
        "Evangelho",
        "Alma",
        "Salvação",
        "Fé",
        "Discipulado",
        "Ação social",
        "Compassivo",
        "Compaixão",
        "Comunidade",
        "Igreja",
        "Cristianismo",
        "Cristão",
        "Reino de Deus",
        "Serviço",
        "Ministério",
        "Impacto",
        "Influência",
        "Transformação",
        "Reavivamento",
        "Desperta",
        "Chamado",
        "Desafio",
        "Compromisso",
        "Discipular",
        "Ensinamento",
        "Crescimento",
        "Relacionamento",
        "Relacionar",
        "Encorajar",
        "Motivar",
        "Inspirar",
        "Equipar",
        "Capacitar"
    } },
            { "Adoração", new List<string> {
        "Adoração",
        "Louvor",
        "Adorar",
        "Culto",
        "Espírito",
        "Santo",
        "Entoar",
        "Glorificar",
        "Exaltar",
        "Magnificar",
        "Honrar",
        "Render",
        "Celebração",
        "Gratidão",
        "Devoção",
        "Deus",
        "Jesus",
        "Santo",
        "Santidade",
        "Presença",
        "Glória",
        "Sacrifício",
        "Oferta",
        "Oração",
        "Veneração",
        "Hosana",
        "Aleluia",
        "Aclamar",
        "Unção",
        "Bendizer",
        "Prostrar",
        "Cântico",
        "Espontâneo",
        "Inspirado",
        "Íntimo",
        "Espiritual",
        "Íntegro",
        "Rendição",
        "Coro",
        "Harmonia",
        "Melodia",
        "Melodioso",
        "Piano",
        "Violão",
        "Guitarra",
        "Bateria",
        "Sons",
        "Celestial",
        "Angelical",
        "Serenata",
        "Alegria",
        "Contentamento",
        "Experiência"
    } },
            { "Gratidão", new List<string>     {
        "Gratidão",
        "Agradecimento",
        "Obrigado",
        "Reconhecimento",
        "Bênçãos",
        "Deus",
        "Jesus",
        "Louvor",
        "Adoração",
        "Benção",
        "Favores",
        "Generosidade",
        "Misericórdia",
        "Provisão",
        "Fé",
        "Amor",
        "Abundância",
        "Proteção",
        "Divina",
        "Sustento",
        "Dádiva",
        "Alegria",
        "Contentamento",
        "Harmonia",
        "Paz",
        "Recompensa",
        "Divino",
        "Favor",
        "Milagre",
        "Conquistas",
        "Sabedoria",
        "Cuidado",
        "Compaixão",
        "Esperança",
        "Confiança",
        "Perdão",
        "Graça",
        "Humildade",
        "Salvação",
        "Vida",
        "Entrega",
        "Restauração",
        "Renovação",
        "Redenção",
        "Livramento",
        "Unção",
        "Amizade",
        "Família",
        "Comunhão",
        "Eternidade",
        "Harpa"
    } },
            { "Reconciliação", new List<string>     {
        "Reconciliação",
        "Perdão",
        "Unidade",
        "Paz",
        "Relacionamentos",
        "Restauração",
        "Harmonia",
        "Amor",
        "Misericórdia",
        "Compaixão",
        "Renovação",
        "Redenção",
        "Cura",
        "Transformação",
        "Divino",
        "Aceitação",
        "Compreensão",
        "Tolerância",
        "Respeito",
        "Reciprocidade",
        "Solidariedade",
        "Fraternidade",
        "Empatia",
        "Bondade",
        "Generosidade",
        "Honestidade",
        "Conciliação",
        "Paciência",
        "Confiança",
        "Cuidado",
        "Compromisso",
        "Perseverança",
        "mútuo",
        "Comunhão",
        "incondicional",
        "Reencontro",
        "Restabelecimento",
        "Reatamento",
        "Reaproximação",
        "Reconstrução",
        "Reintegração",
        "Reunião",
        "Reinício",
        "Reformação",
        "Reunião",
        "Reconciliar",
        "Reconciliado",
        "Reconciliação",
        "Divina",
        "Restaurada",
        "Relação",
        "Vida Nova",
        "Chance",
        "Segunda",
        "Renascimento"
    } },
            { "Amor", new List<string>    {
        "Amor",
        "Amar",
        "Caridade",
        "Compaixão",
        "Misericórdia",
        "Bondade",
        "Generosidade",
        "Ternura",
        "Afeto",
        "Cuidado",
        "Devoção",
        "Paixão",
        "Romance",
        "Fidelidade",
        "Sinceridade",
        "Harmonia",
        "União",
        "Respeito",
        "Gratidão",
        "Empatia",
        "Solidariedade",
        "Perdão",
        "Compreensão",
        "Sacrifício",
        "Paz",
        "Altruísmo",
        "Nobreza",
        "Amizade",
        "Cumplicidade",
        "Fraternal",
        "Incondicional",
        "Divino",
        "Universal",
        "ao Próximo",
        "Eterno",
        "Verdadeiro",
        "Celestial",
        "Profundo",
        "Infinito",
        "Puro",
        "Pleno",
        "Transcendental",
        "Além das Palavras",
        "Tudo Suporta",
        "Transforma",
        "Liberta",
        "Cura",
        "Salva",
        "Conquista",
        "Ilumina",
        "Inspira",
        "Renova"
    } },
            { "Pai", new List<string>    {
        "Pai",
        "Papai",
        "Pado Céu",
        "Deus Pai",
        "Paternidade",
        "Proteção",
        "Cuidado",
        "Paterno",
        "Carinho",
        "Compreensão",
        "Sabedoria",
        "Força",
        "Exemplo",
        "Herança",
        "Bênção",
        "Apoio",
        "Consolo",
        "Fé",
        "Esperança",
        "Confiança",
        "Refúgio",
        "Abraço",
        "Companheirismo",
        "Guia",
        "Prover",
        "Educar",
        "Disciplinar",
        "Instruir",
        "Corrigir",
        "Perdão",
        "Relacionamento",
        "Celestial",
        "Amoroso",
        "Bondoso",
        "Misericordioso",
        "Presente",
        "Fiel",
        "Protetor",
        "Forte",
        "Sabe",
        "Pode",
        "Vê",
        "Entende",
        "Perdoa",
        "Supre",
        "Proverá",
        "Conhece",
        "Governará",
        "Eterno",
        "Criação"
    } },

            { "Filho", new List<string>    {
        "Filho",
        "Filho de Deus",
        "Filho Amado",
        "Herdeiro",
        "Redenção",
        "Salvação",
        "Protegido",
        "Amado",
        "Escolhido",
        "Seguidor",
        "Crente",
        "Discípulo",
        "Servo",
        "Benção",
        "Promessa",
        "Novo",
        "Nascimento",
        "Transformação",
        "Restauração",
        "Reconciliação",
        "Fé",
        "Esperança",
        "Vida",
        "União",
        "Comunhão",
        "Incondicional",
        "Misericórdia",
        "Perdão",
        "Graça",
        "Caminho",
        "Verdade",
        "Vida Eterna",
        "Libertação",
        "Ressurreição",
        "Vitória",
        "Ovelha",
        "Segurança",
        "Cuidado",
        "Guiado",
        "Ensinado",
        "Iluminado",
        "Chamado",
        "Equipado",
        "Fortalecido",
        "Capacitado",
        "Renovado",
        "Alegria",
        "Paz",
        "Conhecimento",
        "Sabedoria",
        "Identidade"
    } },
            { "Alegria", new List<string>     {
        "Alegria",
        "Felicidade",
        "Contentamento",
        "Regozijo",
        "Júbilo",
        "Gratidão",
        "Louvor",
        "Celebração",
        "Exultação",
        "Entusiasmo",
        "Satisfação",
        "Vivacidade",
        "Diversão",
        "Animado",
        "Radiante",
        "Festejar",
        "Festejo",
        "Júbilo",
        "Brilho",
        "Sorriso",
        "Leveza",
        "Divertimento",
        "Festejar",
        "Regozijar",
        "Comemorar",
        "Jovialidade",
        "Entusiasmo",
        "Fervor",
        "Regozijar",
        "Deliciar",
        "Exultar",
        "Regozijar-se",
        "Esbanjar",
        "Apreciar",
        "Rebentar",
        "Brindar",
        "Rebentar",
        "Resplandecer",
        "Rir",
        "Saltar",
        "Vibrar",
        "feliz",
        "Euforia",
        "Contente",
        "Desfrutar",
        "Reluzir",
        "Radiante",
        "Esplendor",
        "Festejo",
    } },
            { "Comunhão", new List<string> {
        "Comunhão",
        "Comunidade",
        "Família",
        "Irmandade",
        "Unidade",
        "Harmonia",
        "Convivência",
        "Relacionamento",
        "Amizade",
        "Participação",
        "Cooperação",
        "Solidariedade",
        "Conexão",
        "Interação",
        "Coexistência",
        "Parceria",
        "Cumplicidade",
        "Confraternização",
        "Concordância",
        "Vínculo",
        "Associação",
        "Sintonia",
        "Congregação",
        "Convívio",
        "Colaboração",
        "Integrar",
        "Coletividade",
        "Equipe",
        "Troca",
        "Empatia",
        "Aliança",
        "fraterno",
        "Compartilhamento",
        "Reciprocidade",
        "Intimidade",
        "Integração",
        "Reunião",
        "Culto",
        "Reunião",
        "oração",
        "Grupo",
        "Conselho",
        "Conselho",
        "Confiança",
        "Consagração",
        "Congregar",
        "Coletivo",
        "Intercâmbio",
        "Encontro",
        "Coexistir",
        "Conviver",
        "Integrado",
        "Relacionar"
    } },
            { "Ressurreição", new List<string> {
        "Ressurreição",
        "Vida eterna",
        "Vitória",
        "Renascimento",
        "Reconstrução",
        "Transformação",
        "Renovação",
        "Restauração",
        "Redenção",
        "Triunfo",
        "Revivificação",
        "Sobrevivência",
        "Reerguimento",
        "Recuperação",
        "Reabilitação",
        "Recomeço",
        "Reavivamento",
        "Retorno",
        "Reanimação",
        "Reanimação",
        "Reencarnação",
        "Regeneração",
        "Reencarnação",
        "Ressuscitar",
        "Renovar",
        "Reanimar",
        "Reviver",
        "Restabelecer",
        "Revitalizar",
        "Reformar",
        "Reestruturar",
        "Recompor",
        "Revigorar",
        "Revitalizar",
        "Reconstituir",
        "Refazer",
        "Reintegrar",
        "Recriar",
        "Reavivar",
        "Reavivar",
        "Reabilitar",
        "Revigorar",
        "Reviver",
        "Reencarnar",
        "Renovar",
        "Ressuscitar",
        "Resgatar",
        "Reviver",
        "Recomeçar",
        "Renovar",
        "Reerguer",
        "Restaurar",
        "Reacender"
    } },
            { "Esperança", new List<string> {
        "Esperança",
        "Fé",
        "Confiança",
        "Promessas",
        "Ânimo",
        "Expectativa",
        "Otismo",
        "Crença",
        "Certeza",
        "Desejo",
        "Perspectiva",
        "Antecipação",
        "Otimismo",
        "Prospecção",
        "Expectação",
        "Prosperidade",
        "Paciência",
        "Resiliência",
        "Motivação",
        "Coragem",
        "Alegria",
        "Sonhos",
        "Oportunidade",
        "Renovação",
        "Inspiração",
        "Determinação",
        "Resistência",
        "Vitalidade",
        "Força",
        "Empenho",
        "Alento",
        "Persistência",
        "Ambição",
        "Encorajamento",
        "Entusiasmo",
        "Confiança",
        "Futuro",
        "Proteção",
        "Segurança",
        "Alívio",
        "Consolo",
        "Tranquilidade",
        "Apoio",
        "Firmeza",
        "Sustentação",
        "Fidelidade",
        "Contentamento",
        "Harmonia",
        "Luz",
        "Direção",
        "Prosperidade",
        "Promessa",
        "Amanhã"
    } },
            { "Fé", new List<string>     {
        "Fé",
        "Crer",
        "Confiança",
        "Certeza",
        "Deus",
        "Religiosidade",
        "Devoção",
        "Crença",
        "Esperança",
        "Espiritualidade",
        "Pai",
        "Filho",
        "Espírito",
        "Santo",
        "Religião",
        "Adoração",
        "Culto",
        "Oração",
        "Bênçãos",
        "Milagre",
        "Palavra",
        "Bíblia",
        "Sacramento",
        "Sacralidade",
        "Divindade",
        "Piedade",
        "Salvação",
        "Redenção",
        "Graça",
        "Comunhão",
        "Comunidade",
        "Ritual",
        "Fidelidade",
        "Promessas",
        "Renovação",
        "Confiança",
        "Proteção",
        "Unção",
        "Divino",
        "Misericórdia",
        "Esperança",
        "Interior",
        "Eterna",
        "Elevação",
        "Devoção",
        "Profunda",
        "Divina",
        "Guiamento",
        "Conexão",
        "Inabalável"
    } },
            { "Salvação", new List<string>     {
        "Salvação",
        "Redenção",
        "Perdão",
        "eterna",
        "Graça",
        "Cruz",
        "Sacrifício",
        "Ressurreição",
        "Jesus Cristo",
        "Renascimento",
        "Misericórdia",
        "Libertação",
        "Paz",
        "Caminho",
        "Verdade",
        "Vida",
        "Fé",
        "Arrependimento",
        "Transformação",
        "nascimento",
        "Novo",
        "novidade",
        "de Deus",
        "Divina",
        "Caminho",
        "Livramento",
        "Vitória",
        "Restauração",
        "Justificação",
        "Reconciliação",
        "Herança eterna",
        "Cura",
        "Esperança",
        "Vida",
        "abundante",
        "Unção",
        "Palavra",
        "Promessas",
        "Regeneração",
        "espiritual",
        "mente",
        "Renovação",
        "Liberdade",
        "Segurança",
        "Resgate",
        "Adoção",
        "Filho",
        "incondicional",
        "Confiança",
        "Entrega",
        "Comunhão",
        "Chamado",
        "Serviço",
        "Reino",
        "céus"
    } },
            { "Transformação", new List<string>     {
        "Transformação",
        "Mudança",
        "Renovação",
        "Metamorfose",
        "Novidade de vida",
        "Renascimento",
        "Restauração",
        "Crescimento",
        "Evolução",
        "Reformação",
        "Reconstrução",
        "Reabilitação",
        "Refazimento",
        "Reconstrução",
        "Reorganização",
        "Reestruturação",
        "Melhoria",
        "Aprimoramento",
        "Desenvolvimento",
        "Progresso",
        "Amadurecimento",
        "Maturidade",
        "Transfiguração",
        "Regeneração",
        "Recriação",
        "Transição",
        "Metanoia",
        "Recuperação",
        "Reconquista",
        "Conversão",
        "Reviravolta",
        "Renovação espiritual",
        "Renovação da mente",
        "Novo começo",
        "Renovação interior",
        "pessoal",
        "Reinvenção",
        "emocional",
        "física",
        "mental",
        "Reinício",
        "Recomeço",
        "Reformulação",
        "Reestruturação",
        "Requalificação",
        "Reorientação",
        "Readequação",
        "Reaprendizado",
        "Reeducação",
        "Resiliência",
        "Reajuste",
        "Reposicionamento"
    } },
            { "Graça", new List<string>    {
        "Graça",
        "Misericórdia",
        "Perdão",
        "incondicional",
        "Benevolência",
        "Compaixão",
        "Favor",
        "Bondade",
        "Generosidade",
        "Indulgência",
        "Clemência",
        "Piedade",
        "Tolerância",
        "Paciência",
        "Benignidade",
        "Redenção",
        "Salvação",
        "Livramento",
        "divina",
        "infinita",
        "sem fim",
        "gratuito",
        "abundante",
        "imerecido",
        "divino",
        "eterno",
        "infinito",
        "sem limites",
        "sem condições",
        "compassivo",
        "inabalável",
        "salvador",
        "redentor",
        "restaurador",
        "perdoador",
        "transformador",
        "reconciliador",
        "libertador",
        "acolhe",
        "restaura",
        "cura",
        "transforma",
        "perdoa",
        "renova",
        "resgata",
        "sustenta",
        "fortalece",
        "renova",
        "renasce",
        "ilumina",
        "guia"
    } },
    { "Ceia", new List<string> { "Ceia", "Santa", "Comunhão", "Partir", "Pão", "Cálice", "Eucaristia", "Banquete", "Vinho", "Corpo","Sangue", "Memorial", "Renovação", "Refeição", "Sacramento", "Instituição", "Liturgia", "Participação", "Ressurreição", "eterna", "Sacrifício", "Amor" } }
        };

        private string[] IdentifyTags(string lyrics)
        {
            var tagRelevance = new Dictionary<string, int>();

            foreach (var tagEntry in TagSynonyms)
            {
                var tag = tagEntry.Key;
                var synonyms = tagEntry.Value;
                var relevance = 0;

                foreach (var synonym in synonyms)
                {
                    if (lyrics.Contains(synonym, StringComparison.OrdinalIgnoreCase))
                    {
                        relevance++;
                    }
                }

                tagRelevance[tag] = relevance;
            }

            // Ordenar as tags por relevância (maior para menor)
            var sortedTags = tagRelevance.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();

            // Verificar se a palavra-chave do grupo está presente no top 5 das tags relevantes
            foreach (var tagEntry in TagSynonyms)
            {
                var tag = tagEntry.Key;
                var synonyms = tagEntry.Value;

                if (synonyms.Any(synonym => sortedTags.Contains(synonym, StringComparer.OrdinalIgnoreCase)))
                {
                    sortedTags.Insert(0, tag); // Inserir a tag no início do top 5
                    break;
                }
            }

            // Retornar as top 5 tags relevantes
            var relevantTags = sortedTags.Take(5).Distinct().ToArray();
            
            return relevantTags;
        }


    }

}

