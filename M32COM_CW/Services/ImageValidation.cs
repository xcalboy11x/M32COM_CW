using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace M32COM_CW.Services
{
    public class ImageValidation : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null) {
                var file = value as IFormFile;
                if (file.Length > 1 * 1024 * 1024)
                {
                    return false;
                }
                if (file.ContentType.ToLower() != "image/jpg" &&
                        file.ContentType.ToLower() != "image/jpeg")
                {
                    return false;
                }
            }
            return true;
        }
    }
}
