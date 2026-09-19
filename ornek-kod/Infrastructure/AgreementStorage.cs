namespace VincYonetim.Infrastructure
{
    /// <summary>
    /// Sözleşme görsellerini wwwroot DIŞINDA (herkese açık olmayan) bir klasöre kaydeder.
    /// Dosya adı istemciden alınmaz; uzantı ve dosya imzası (magic bytes) doğrulanır.
    /// </summary>
    public class AgreementStorage
    {
        public const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        private static readonly Dictionary<string, byte[][]> AllowedSignatures = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } },
        };

        private readonly string _root;

        public AgreementStorage(IWebHostEnvironment environment)
        {
            _root = Path.Combine(environment.ContentRootPath, "App_Data", "agreements");
            Directory.CreateDirectory(_root);
        }

        /// <returns>Kaydedilen dosyanın adı; dosya geçersizse null.</returns>
        public async Task<string?> SaveAsync(IFormFile file)
        {
            if (file.Length == 0 || file.Length > MaxFileSize)
                return null;

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedSignatures.TryGetValue(extension, out var signatures))
                return null;

            await using var input = file.OpenReadStream();
            var header = new byte[8];
            var read = await input.ReadAsync(header.AsMemory(0, header.Length));
            if (!signatures.Any(sig => read >= sig.Length && header.AsSpan(0, sig.Length).SequenceEqual(sig)))
                return null;
            input.Position = 0;

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            await using var output = new FileStream(Path.Combine(_root, fileName), FileMode.CreateNew);
            await input.CopyToAsync(output);
            return fileName;
        }
    }
}
