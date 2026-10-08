using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Exceptions;

namespace Electium.Application.Exceptions;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}