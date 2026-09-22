using System;
using System.Threading;
using System.Threading.Tasks;
using CreatioChallengeBack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreatioChallengeBack.Controllers
{
    [ApiController]
    [Route("api/test-creatio")]
    public class TestCreatioController : ControllerBase
    {
        private readonly ICreatioApiClient _creatioClient;
        private readonly ILogger<TestCreatioController> _logger;

        public TestCreatioController(ICreatioApiClient creatioClient, ILogger<TestCreatioController> logger)
        {
            _creatioClient = creatioClient;
            _logger = logger;
        }

        // GET api/test-creatio
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            // First, try to obtain $metadata to discover entity sets and find a Contact-like entity.
            var metadataCandidates = new[] { "/odata/$metadata", "/0/odata/$metadata" };
            string metadata = null;
            string metadataRelative = null;

            foreach (var m in metadataCandidates)
            {
                try
                {
                    metadata = await _creatioClient.GetStringAsync(m, cancellationToken).ConfigureAwait(false);
                    metadataRelative = m;
                    break;
                }
                catch (ApiException aex)
                {
                    _logger.LogInformation(aex, "Metadata candidate {Relative} failed with {Status}", m, aex.StatusCode);
                    if (aex.StatusCode != 404)
                        return StatusCode(aex.StatusCode ?? 502, new { success = false, message = aex.Message, attempted = m });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error fetching metadata candidate {Relative}", m);
                    return StatusCode(500, new { success = false, message = "Error interno al obtener metadata", attempted = m });
                }
            }

            if (string.IsNullOrEmpty(metadata))
            {
                return NotFound(new { success = false, message = "No se pudo obtener $metadata desde la API Creatio. Revisa permisos y URL." });
            }

            // Parse metadata XML and find entity sets
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(metadata);
                var entitySetNames = new System.Collections.Generic.List<string>();
                foreach (var el in doc.Descendants())
                {
                    if (el.Name.LocalName.Equals("EntitySet", StringComparison.OrdinalIgnoreCase))
                    {
                        var nameAttr = el.Attribute("Name");
                        if (nameAttr != null && !string.IsNullOrEmpty(nameAttr.Value))
                            entitySetNames.Add(nameAttr.Value);
                    }
                }

                if (entitySetNames.Count == 0)
                {
                    return NotFound(new { success = false, message = "No se encontraron EntitySet en $metadata." });
                }

                // Prefer a Contact-like entity name
                string chosen = null;
                chosen = entitySetNames.Find(n => n.IndexOf("Contact", StringComparison.OrdinalIgnoreCase) >= 0);
                if (string.IsNullOrEmpty(chosen))
                    chosen = entitySetNames[0];

                // Try to call the chosen entity set using both base paths
                var callCandidates = new[] { $"/odata/{chosen}?$top=1", $"/0/odata/{chosen}?$top=1" };
                foreach (var relative in callCandidates)
                {
                    try
                    {
                        var body = await _creatioClient.GetStringAsync(relative, cancellationToken).ConfigureAwait(false);
                        return Ok(new { success = true, metadataSource = metadataRelative, used = relative, preview = body.Length > 1000 ? body.Substring(0, 1000) : body });
                    }
                    catch (ApiException aex)
                    {
                        _logger.LogInformation(aex, "Attempt to call {Relative} failed with {Status}", relative, aex.StatusCode);
                        if (aex.StatusCode != 404)
                            return StatusCode(aex.StatusCode ?? 502, new { success = false, message = aex.Message, attempted = relative });
                    }
                }

                return NotFound(new { success = false, message = "Se detectó $metadata pero no se pudo acceder a la entidad seleccionada en ninguna ruta." });
            }
            catch (System.Xml.XmlException xex)
            {
                _logger.LogError(xex, "Failed to parse metadata XML");
                return StatusCode(500, new { success = false, message = "Error parseando $metadata." });
            }
        }
    }
}
