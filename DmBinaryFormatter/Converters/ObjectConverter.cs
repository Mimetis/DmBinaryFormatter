using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DmBinaryFormatter.Converters
{
    public abstract class ObjectConverter
    {
        public static ObjectConverter GetConverter(Type objType)
        {
            if (objType == typeof(Type))
                return new ObjectTypeConverter();

            if (objType == (typeof(Type).GetType()))
                return new ObjectTypeConverter();

            if (objType == typeof(CultureInfo))
                return new CultureInfoConverter();

            if (objType == typeof(Version))
                return new VersionConverter();

            return null ;
        }
        public abstract object ConvertFromString(String obj);
        public abstract string ConvertToString(Object s);

    }
}
