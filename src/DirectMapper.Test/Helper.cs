using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DirectMapper.Test
{
    internal static class Helper
    {
        public static bool ArePropertiesEqual<T>(T? first, T? second)
        {
            bool isFirstNull = first is null;
            bool isSecondNull = second is null;
            if (isFirstNull && isSecondNull) return true;   // Both are null
            if (isFirstNull ^ isSecondNull) return false;   // One is null and the other is not null

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (property.GetValue(first)?.Equals(property.GetValue(second)) == false) return false;
            }
            return true;
        }

        public static bool AreCommonPropertiesEqual<TFirst, TSecond>(TFirst? first, TSecond? second, params string[] ignoredProperties)
        {
            bool isFirstNull = first is null;
            bool isSecondNull = second is null;
            if (isFirstNull && isSecondNull) return true;   // Both are null
            if (isFirstNull ^ isSecondNull) return false;   // One is null and the other is not null

            var firstProperties = typeof(TFirst).GetProperties();
            var secondProperties = typeof(TSecond).GetProperties();

            foreach (PropertyInfo firstProp in firstProperties.Where(p => !ignoredProperties.Contains(p.Name)))
            {
                if (secondProperties.FirstOrDefault(p => p.Name == firstProp.Name && p.PropertyType == firstProp.PropertyType) is PropertyInfo secondProperty)
                {
                    if (firstProp.GetValue(first)?.Equals(secondProperty.GetValue(second)) == false) return false;
                }
            }
            return true;
        }

        public static bool AreItemsEqual<TItem>(IEnumerable<TItem> first, IEnumerable<TItem> second)
        {
            bool isFirstNull = first is null;
            bool isSecondNull = second is null;
            if (isFirstNull && isSecondNull) return true;   // Both are null
            if (isFirstNull ^ isSecondNull) return false;   // One is null and the other is not null

            var firstList = first!.ToList();
            var secondList = second!.ToList();
            if (firstList.Count != secondList.Count) return false;

            for (int i = 0; i < firstList.Count; i++)
            {
                if (ArePropertiesEqual(firstList[i], secondList[i]) == false) return false;
            }

            return true;
        }

        public static bool AreItemsMatched<TFirstItem, TSecondItem>(IEnumerable<TFirstItem> first, IEnumerable<TSecondItem> second)
        {
            bool isFirstNull = first is null;
            bool isSecondNull = second is null;
            if (isFirstNull && isSecondNull) return true;   // Both are null
            if (isFirstNull ^ isSecondNull) return false;   // One is null and the other is not null

            var firstList = first!.ToList();
            var secondList = second!.ToList();
            if (firstList.Count != secondList.Count) return false;

            for (int i = 0; i < firstList.Count; i++)
            {
                if (AreCommonPropertiesEqual(firstList[i], secondList[i]) == false) return false;
            }

            return true;
        }
    }
}