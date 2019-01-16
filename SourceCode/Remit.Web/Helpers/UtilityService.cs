using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Remit.Web.Helpers
{
    public static class UtilityService
    {
        //Numeric int to string
        public static string GetNumericStringFromIntegerValue(int intValue, int length)
        {
            try
            {
                return new string((intValue).ToString().ToArray()).PadLeft(length, '0');
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //Numeric string to int
        public static int GetIntegerValueFromNumericString(string numeric)
        {
            try
            {
                return int.Parse(numeric, System.Globalization.NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}