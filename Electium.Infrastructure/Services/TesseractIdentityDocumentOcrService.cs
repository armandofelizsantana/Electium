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
        // TesseractOCR busca el archivo de idioma directamente dentro
        // del directorio que se le pasa al motor.
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

        var tessdataPath = Path.Combine(
            applicationBasePath,
            "tessdata");

        using var engine = new Engine(
            tessdataPath,
            Language.SpanishCastilian,
            EngineMode.LstmOnly);

        using var image = Image.LoadFromMemory(imageBytes);
        using var page = engine.Process(image);

        return page.Text ?? string.Empty;
    }
}