using DirectMapper.Test.Models;
using Mighty;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DirectMapper.Test
{
    internal static class Helper
    {
        public static List<Customer> LoadCustomers()
        {
            var db = new MightyOrm<Customer>(
                "ProviderName=System.Data.SqlClient; Data Source=.;Initial Catalog=AdventureWorksDW2019; Integrated Security=sspi",
                "DimCustomer",
                "CustomerKey"
            );

            return db.All().ToList();
        }

        public static bool ArePropertiesEqual<T>(T first, T second)
        {
            if (first == null && second == null) return true;
            if ((first == null && second != null) || first != null && second == null) return false;

            foreach (var property in typeof(T).GetProperties())
            {
                if (property?.GetValue(first)?.Equals(property?.GetValue(second)) == false) return false;
            }
            return true;
        }

        public static bool ArePropertiesMatched<TFirst, TSecond>(TFirst first, TSecond second)
        {
            if (first == null && second == null) return true;
            if ((first == null && second != null) || first != null && second == null) return false;

            var firstProperties = typeof(TFirst).GetProperties();
            var secondProperties = typeof(TSecond).GetProperties();

            foreach (var property in firstProperties)
            {
                if (secondProperties.FirstOrDefault(p => p.Name == property.Name && p.PropertyType == property.PropertyType) is PropertyInfo secondProperty)
                {
                    if (property.GetValue(first)?.Equals(secondProperty.GetValue(second)) == false) return false;
                }
            }
            return true;
        }

        public static bool AreItemsEqual<TItem>(IEnumerable<TItem> first, IEnumerable<TItem> second)
        {
            if (first == null && second == null) return true;
            if ((first == null && second != null) || first != null && second == null) return false;

            List<TItem> firstList = first.ToList();
            List<TItem> secondList = second.ToList();
            if (firstList.Count != secondList.Count) return false;

            for (int i = 0; i < firstList.Count; i++)
            {
                if (!firstList[i].Equals(secondList[i])) return false;
            }

            return true;
        }

        public static bool AreItemsMatched<TFirstItem, TSecondItem>(IEnumerable<TFirstItem> first, IEnumerable<TSecondItem> second)
        {
            //if (first == null && second == null) return true;
            //if ((first == null && second != null) || first != null && second == null) return false;

            //List<TItem> firstList = first.ToList();
            //List<TItem> secondList = second.ToList();
            //if (firstList.Count != secondList.Count) return false;

            //for (int i = 0; i < firstList.Count; i++)
            //{
            //    if (!firstList[i].Equals(secondList[i])) return false;
            //}

            return true;
        }
    }
}