using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;

namespace Nop.Plugin.Shipping.Correios.Util
{
    public class ImageHelper
    {
        public static string ObterPathImagens()
        {
            string pathImages = CommonHelper.MapPath("~/Plugins/Shipping.Correios/Content/Images/");
            return pathImages;
        }



        public static Image GetDataMatrix(string code)
        {
            var barcodeDatamatrix = new BarcodeDatamatrix();
            barcodeDatamatrix.Options = 0;
            barcodeDatamatrix.Generate(code);
            Image retorno = barcodeDatamatrix.CreateImage();
            return retorno;
        }


        public static Image GetBarCode(string code, PdfWriter pdfWriter)
        {

            if (code.Length == 8)
                return GetBarCode(code, pdfWriter, 1.30f);
            else
                return GetBarCode(code, pdfWriter, 1.70f); 
        }

        public static Image GetBarCode(string code, PdfWriter pdfWriter, float x)
        {

            var barcode = new Barcode128();

            barcode.BarHeight = Utilities.MillimetersToPoints(17f);

            barcode.ChecksumText = true;
            barcode.GenerateChecksum = true;
            barcode.StartStopText = true;

            barcode.CodeSet = Barcode128.Barcode128CodeSet.AUTO;
            barcode.CodeType = Barcode.CODE128_RAW;
            //barcode.Code = Barcode128.GetRawText(code, true, Barcode128.Barcode128CodeSet.AUTO);
            barcode.Code = Barcode128.GetRawText(code, false, Barcode128.Barcode128CodeSet.AUTO);

            barcode.X = x;
            barcode.Font = null;

            var bitmap2 = barcode.CreateImageWithBarcode(pdfWriter.DirectContent, BaseColor.BLACK, BaseColor.BLACK);
            return bitmap2;
        }
    }
}
