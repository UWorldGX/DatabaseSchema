using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirewallDemo.Utility;

public static class Utilities
{
    public static bool IsEnglishStr(this string str)
    {
        return Regex.IsMatch(str, "^[A-Za-z ]+$");
    }
    /// <summary>
    /// 辅助方法，该方法可以将原对象中与目标对象类型、名称一致的字段拷贝过去
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    public static void Copy(object source, object destination)
    {
        PropertyInfo[] sourceProperties = source.GetType().GetProperties();
        PropertyInfo[] destinationProperties = destination.GetType().GetProperties();

        foreach (var sourceProp in sourceProperties)
        {
            foreach (var destinationProp in destinationProperties)
            {
                if (sourceProp.Name == destinationProp.Name &&
                    sourceProp.PropertyType == destinationProp.PropertyType)
                {
                    destinationProp.SetValue(destination, sourceProp.GetValue(source));
                    break;
                }
            }
        }
    }
}
