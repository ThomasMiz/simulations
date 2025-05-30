// Copyright (c) 2017 Kastellanos Nikolaos

namespace System.Text
{
    static class StringBuilderExtensions
    {
        public static StringBuilder AppendNumber(this StringBuilder sb, int value)
        {
            if (value < 0)
            {
                sb.Append('-');
                value = -value;
            }

            AppendInt(sb, value);
            return sb;
        }

        private static void AppendInt(StringBuilder sb, int value)
        {
            char ch = (char)('0' + value % 10);

            value /= 10;
            if (value > 0)
                AppendInt(sb, value);

            sb.Append(ch);
        }

        public static StringBuilder AppendNumber(this StringBuilder sb, float value, int precision = 7)
        {
            AppendNumber(sb, (int)value);
            
            if (value % 1 > 0)
                sb.Append('.');

            int mag = 1;
            float valMag = value;
            while (valMag % 1 > 0 && precision-- > 0)
            {
                mag *= 10;
                valMag = value * mag;
                var i = (int)((valMag) % 10);
                char ch = (char)('0' + i);
                sb.Append(ch);
            }

            return sb;
        }        
    }   
}