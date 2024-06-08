using System;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Helpers;

public class ColladaParseUtils
{
    public static int[] String_To_Int(string int_array)
    {
        string[] str = int_array.Split(' ');
        int[] array = new int[str.LongLength];
        try
        {
            for (long i = 0; i < str.LongLength; i++)
                array[i] = Convert.ToInt32(str[i]);
        }
        catch (Exception e)
        {
            Utilities.Log(LogLevelEnum.Error, e.ToString());
            Utilities.Log(LogLevelEnum.Error);
            Utilities.Log(LogLevelEnum.Error, int_array);
        }
        return array;
    }

    public static float[] String_To_Float(string float_array)
    {
        string[] str = float_array.Split(' ');
        float[] array = new float[str.LongLength];
        try
        {
            for (long i = 0; i < str.LongLength; i++)
                array[i] = Convert.ToSingle(str[i]);
        }
        catch (Exception e)
        {
            Utilities.Log(LogLevelEnum.Error, e.ToString());
            Utilities.Log(LogLevelEnum.Error);
            Utilities.Log(LogLevelEnum.Error, float_array);
        }
        return array;
    }

    public static bool[] String_To_Bool(string bool_array)
    {
        string[] str = bool_array.Split(' ');
        bool[] array = new bool[str.LongLength];
        try
        {
            for (long i = 0; i < str.LongLength; i++)
                array[i] = Convert.ToBoolean(str[i]);
        }
        catch (Exception e)
        {
            Utilities.Log(LogLevelEnum.Error, e.ToString());
            Utilities.Log(LogLevelEnum.Error);
            Utilities.Log(LogLevelEnum.Error, bool_array);
        }
        return array;
    }



}
