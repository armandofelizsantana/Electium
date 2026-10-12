using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.Interfaces;

public interface IIdentityDocumentOcrService
{
    string ReadText(byte[] imageBytes);
}