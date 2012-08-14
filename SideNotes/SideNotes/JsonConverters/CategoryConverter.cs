using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using SideNotes.Models;

namespace SideNotes.JsonConverters
{
    public class CategoryConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(Category) })); }
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var category = obj as Category;
            if (category == null)
                throw new InvalidOperationException("object must be of the Category type");
            
            var children = category.Children.Select(c => Serialize(c, serializer)).ToList();
            IDictionary<string, object> jsonResult = new Dictionary<string, object>();
            jsonResult.Add("Id", category.Id);
            jsonResult.Add("Name", category.Name);
            jsonResult.Add("Children", children);
            return jsonResult;
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}