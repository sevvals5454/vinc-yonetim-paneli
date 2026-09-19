using Microsoft.AspNetCore.Mvc;
using VincYonetim.Data.DataModel;
using VincYonetim.Infrastructure;
using VincYonetim.Services;
using VincYonetim.ViewModels.ApiModels;

namespace VincYonetim.Controllers
{
    // Mobil uygulamanın kullandığı REST API. Her istek X-Api-Key başlığı ile doğrulanır.
    [ApiController]
    [ApiKey]
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]")]
    public class AppController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly UserService _userService;
        private readonly IAgreementService _agreementService;
        private readonly AgreementStorage _storage;
        private readonly ILogger<AppController> _logger;

        public AppController(ICarService carService, UserService userService,
            IAgreementService agreementService, AgreementStorage storage, ILogger<AppController> logger)
        {
            _carService = carService;
            _userService = userService;
            _agreementService = agreementService;
            _storage = storage;
            _logger = logger;
        }

        [HttpGet("cars/getall")]
        public IActionResult GetAllCars() =>
            Ok(new BaseApiModel<List<Cars>> { IsSuccess = true, Data = _carService.GetCars() });

        // Şifre alanı dönmez: sadece operatör seçimi için gereken bilgiler.
        [HttpGet("users/getoperators")]
        public IActionResult GetOperators() =>
            Ok(new BaseApiModel<object>
            {
                IsSuccess = true,
                Data = _userService.GetOperators()
                    .Select(u => new { u.Id, u.Username, u.Firstname, u.Lastname })
            });

        // Operatörün sahadan çektiği sözleşme fotoğrafını kaydeder.
        [HttpPost("agreement")]
        [RequestSizeLimit(AgreementStorage.MaxFileSize + 1024 * 1024)]
        public async Task<IActionResult> UploadAgreement([FromForm] UploadAgreementModel model)
        {
            var file = model.AgreementFile?.FirstOrDefault();
            if (file == null)
                return BadRequest(new BaseApiModel<object> { Message = "Dosya seçilmedi." });

            if (_carService.GetCar(model.CarId) == null)
                return BadRequest(new BaseApiModel<object> { Message = "Araç bulunamadı." });

            try
            {
                var fileName = await _storage.SaveAsync(file);
                if (fileName == null)
                    return BadRequest(new BaseApiModel<object> { Message = "Yalnızca 10 MB'a kadar JPG, PNG veya PDF yüklenebilir." });

                var ok = _agreementService.CreateAgreement(model.CarId, model.OperatorId, fileName);
                return Ok(new BaseApiModel<object> { IsSuccess = ok, Message = ok ? null : "Kayıt oluşturulamadı." });
            }
            catch (Exception ex)
            {
                // Ayrıntı yalnızca sunucu loguna yazılır; istemciye iç hata mesajı dönmez.
                _logger.LogError(ex, "Sözleşme yüklenemedi");
                return StatusCode(500, new BaseApiModel<object> { Message = "Beklenmeyen bir hata oluştu." });
            }
        }
    }
}
