using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DevSharp.Annotations;

namespace DevSharp.TestWeb.Models
{
    public class AllTestsModel
    {
        public TestModel[] Tests { get; set; }
    }

    public class TestModel
    {
        public TestModel(Type type)
        {
            Type = type;
            Name = Type.Name;
            Id = Type.FullName;
            DisplayName = Type.GetDisplayName();
        }

        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public Type Type { get; set; }
    }

    public static class Extensions
    {
        public static string GetDisplayName(this Type type)
        {
            // First look into attributes on a type and it's parents
            DisplayNameAttribute attr;
            attr = (DisplayNameAttribute) type.GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();

            // Look for [MetadataType] attribute in type hierarchy
            // http://stackoverflow.com/questions/1910532/attribute-isdefined-doesnt-see-attributes-applied-with-metadatatype-class
            //if (attr == null)
            //{
            //    MetadataTypeAttribute metadataType = (MetadataTypeAttribute) type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
            //    if (metadataType != null)
            //    {
            //        attr = (DisplayAttribute) metadataType.MetadataClassType.GetCustomAttributes(typeof (DisplayNameAttribute), true).SingleOrDefault();
            //    }
            //}
            return (attr != null) ? attr.Name : type.Name;


        }
        public static string GetDisplayName<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> expression)
        {

            Type type = typeof(TModel);

            MemberExpression memberExpression = (MemberExpression) expression.Body;
            string propertyName = ((memberExpression.Member is PropertyInfo) ? memberExpression.Member.Name : null);

            // First look into attributes on a type and it's parents
            DisplayNameAttribute attr;
            attr = (DisplayNameAttribute) type.GetProperty(propertyName).GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();

            //// Look for [MetadataType] attribute in type hierarchy
            //// http://stackoverflow.com/questions/1910532/attribute-isdefined-doesnt-see-attributes-applied-with-metadatatype-class
            //if (attr == null)
            //{
            //    MetadataTypeAttribute metadataType = (MetadataTypeAttribute) type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
            //    if (metadataType != null)
            //    {
            //        var property = metadataType.MetadataClassType.GetProperty(propertyName);
            //        if (property != null)
            //        {
            //            attr = (DisplayAttribute) property.GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();
            //        }
            //    }
            //}
            return (attr != null) ? attr.Name : propertyName;


        }
    }
}