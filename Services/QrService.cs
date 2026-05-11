using QRCoder;

namespace AirConServicingManagementSystem.Services
{
    public class QrService
    {
        public byte[] GenerateQr(string url)
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            return qr.GetGraphic(20);
        }
    }
}