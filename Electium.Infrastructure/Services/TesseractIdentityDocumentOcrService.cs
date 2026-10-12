using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;
using TesseractOCR;
using TesseractOCR.Enums;
using TesseractOCR.Pix;

namespace Electium.Infrastructure.Services;

public class TesseractIdentityDocumentOcrService
    : IIdentityDocumentOcrService
{
    public string ReadText(byte[] imageBytes)
    {
        if (imageBytes is null || imageBytes.Length == 0)
        {
            throw new ArgumentException(
                "La imagen recibida está vacía.",
                nameof(imageBytes));
        }

        // El directorio base es la carpeta de salida de Electium.Web.
        // Allí debe existir la subcarpeta tessdata.
        var applicationBasePath = AppContext.BaseDirectory;
        var languageDataPath = Path.Combine(
            applicationBasePath,
            "tessdata",
            "spa.traineddata");

        if (!File.Exists(languageDataPath))
        {
            throw new InvalidOperationException(
                $"No se encontró el archivo de idioma OCR: {languageDataPath}");
        }

        // Tesseract recibe el directorio padre que contiene "tessdata".
        using var engine = new Engine(
            applicationBasePath,
            Language.SpanishCastilian,
            EngineMode.LstmOnly);

        using var image = Image.LoadFromMemory(imageBytes);
        using var page = engine.Process(image);

        return page.Text ?? string.Empty;
    }
}