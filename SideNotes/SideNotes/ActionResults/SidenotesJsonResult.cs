using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using SideNotes.JsonConverters;

namespace SideNotes.ActionResults
{
    public class SidenotesJsonResult : JsonResult
    {
        const string JsonRequest_GetNotAllowed = "This request has been blocked because sensitive information could be disclosed to third party web sites when this is used in a GET request. To allow GET requests, set JsonRequestBehavior to AllowGet.";
        public SidenotesJsonResult()
        {
            MaxJsonLength = 1024000;
            RecursionLimit = 100;
        }
 
        public int MaxJsonLength { get; set; }
        public int RecursionLimit { get; set; }
 
        public override void ExecuteResult( ControllerContext context )
        {
            if( context == null )
            {
                throw new ArgumentNullException( "context" );
            }
            if( JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                String.Equals( context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase ) )
            {
                throw new InvalidOperationException( JsonRequest_GetNotAllowed );
            }
 
            HttpResponseBase response = context.HttpContext.Response;
 
            if( !String.IsNullOrEmpty( ContentType ) )
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }
            if( ContentEncoding != null )
            {
                response.ContentEncoding = ContentEncoding;
            }
            if( Data != null )
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = MaxJsonLength, RecursionLimit = RecursionLimit };
                serializer.RegisterConverters(new []{new CategoryConverter()});
                response.Write( serializer.Serialize( Data ) );
            }
        }

    }
}