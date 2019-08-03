using System;

namespace StateBliss
{
    public static class EnumExtensions
    {
        public static int ToInt<TEnum>(this TEnum target) where TEnum : Enum
        {
            return (int) Enum.ToObject(target.GetType(), target);
        }
        
        public static TEnum ToEnum<TEnum>(this int target) where TEnum : Enum
        {
            return  (TEnum)(object) target;
        }
    }
}