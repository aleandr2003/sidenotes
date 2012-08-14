using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Reflection;

namespace SideNotes.Application.Validation
{
    public class DataTypeAttributeAdapter : DataAnnotationsModelValidator<DataTypeAttribute>
    {
        public DataTypeAttributeAdapter(ModelMetadata metadata, ControllerContext context, DataTypeAttribute attribute)
            : base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            if (Attribute.DataType == DataType.EmailAddress)
            {
                var rule = new ModelClientValidationEmailRule(Attribute.FormatErrorMessage(Metadata.GetDisplayName()));
                rule.ValidationParameters.Add("email", "true");
                return new[] { rule };
            }

            return base.GetClientValidationRules();
        }
        public override IEnumerable<ModelValidationResult> Validate(object container)
        {

            List<ModelValidationResult> result = new List<ModelValidationResult>();
            result.AddRange(base.Validate(container));
            if (Attribute.DataType == DataType.EmailAddress)
            {
                Type t = container.GetType();
                PropertyInfo p = t.GetProperty(Metadata.PropertyName);
                string email = (string)p.GetValue(container, null);

                if (!IsValidEmail(email))
                {
                    var emailValidationResult = new ModelValidationResult()
                    {
                        MemberName = Metadata.PropertyName,
                        Message = Attribute.FormatErrorMessage(Metadata.GetDisplayName())
                    };
                    result.Add(emailValidationResult);
                }
            }

            return result;
        }

        public bool IsValidEmail(string email)
        {
            //regular expression pattern for valid email
            //addresses, allows for the following domains:
            //com,edu,info,gov,int,mil,net,org,biz,name,museum,coop,aero,pro,tv
            string pattern = @"^[-a-zA-Z0-9_][-.a-zA-Z0-9_]*@[-.a-zA-Z0-9_]+(\.[-.a-zA-Z0-9_]+)*\.
    (com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$";
            //Regular expression object
            Regex check = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            //boolean variable to return to calling method
            bool valid = false;

            //make sure an email address was provided
            if (string.IsNullOrEmpty(email))
            {
                valid = false;
            }
            else
            {
                //use IsMatch to validate the address
                valid = check.IsMatch(email);
            }
            //return the value to the calling method
            return valid;
        }
    }
}