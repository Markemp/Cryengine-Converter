namespace CgfConverter.Utililities;

public static class CryHalf
{
    public static float ConvertToFloat(ushort value)
    {
        uint mantissa;
        uint exponent;
        uint result;

        mantissa = (uint)(value & 0x03FF);
        
        if ((value & 0x7C00) != 0)
            exponent = (uint)((value >> 10) & 0x1F);
        else if (mantissa != 0)
        {
            exponent = 1;
            do
            {
                exponent--;
                mantissa <<= 1;
            } while ((mantissa & 0x0400) == 0);
            mantissa &= 0x03FF;
        }
        else
            exponent = unchecked((uint)-112);
        
        result = ((value & (uint)0x8000) << 16) | ((exponent + 112) << 23) | (mantissa << 13);
        
        return (float)result;
    }
}
