//
// This file contains the DirectMapper 1.0.0-rc1 source code and is published under BSD 3-Clause License.
// Visit https://github.com/FarzanHajian/DirectMapper/blob/main/LICENSE for details.
//

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FarzanHajian.DirectMapper
{
    public static class DirectMapper
    {
        private static readonly Dictionary<string, object> funcCache = new Dictionary<string, object>();
        private static GlobalRuleBuilder globalRuleBuilder = null;

        #region Main Functionalities

        public static TDestination DirectMap<TSource, TDestination>(this TSource source)
        {
            if (source == null) return default;

            Func<TSource, TDestination> funcCopy = GetCopyFunction<TSource, TDestination>();
            funcCopy ??= PrepareMapperFunction<TSource, TDestination>(null);    // No specific rule is defined, force copy function creation.
            return funcCopy(source);
        }

        public static IEnumerable<TDestination> DirectMapRange<TSource, TDestination>(this IEnumerable<TSource> source)
        {
            if (source == null) return null;

            Func<TSource, TDestination> funcCopy = GetCopyFunction<TSource, TDestination>();
            funcCopy ??= PrepareMapperFunction<TSource, TDestination>(null);    // No specific rule is defined, force copy function creation.
            return source.Select(s => funcCopy(s));
        }

        public static Func<TSource, TDestination> GetMapper<TSource, TDestination>()
        {
            return GetCopyFunction<TSource, TDestination>();
        }

        private static Func<TSource, TDestination> GetCopyFunction<TSource, TDestination>()
        {
            var hash = GetHash<TSource, TDestination>();
            return funcCache.ContainsKey(hash) ? (Func<TSource, TDestination>)funcCache[hash] : null;
        }

        private static Func<TSource, TDestination> PrepareMapperFunction<TSource, TDestination>(Dictionary<string, Delegate> specificRules)
        {
            Func<TSource, TDestination> result = CreateCopyFunction<TSource, TDestination>(specificRules);
            var hash = GetHash<TSource, TDestination>();
            funcCache.Add(hash, result);
            return result;
        }

        private static Func<TSource, TDestination> CreateCopyFunction<TSource, TDestination>(Dictionary<string, Delegate> specificRules)
        {
            /* Copy function would be similar to the following template:
             *
             * TDestionation [CopyFunc](TSource source)
             * {
             *      return new TDestination
             *      {
             *          [Prop1] = source.[Prop1],   *
             *          [Prop2] = source.[Prop2],   *
             *          ...
             *          [Prop3] = ((Func<object, string>)((srcProp) => srcProp.ToString()))(source.[Prop3]),  **
             *          [Prop4] = ((Func<TSrcProp, TDestProp>)((srcProp) => {...}))(source.[Prop4]),  ***
             *          ...
             *          [Propn] = source.[Propn],   *
             *      };
             * }
             *
             *  NOTES: *   : Normal copy process from source to destination
             *         **  : When Global ToString rule is active
             *         *** : When either there is a global rule on two types or a specific rule on a property is defined
             *
             */

            bool hasSpecificRules = specificRules != null && specificRules.Keys.Count > 0;
            bool hasGlobalRules = globalRuleBuilder != null;
            bool hasRules = hasSpecificRules || hasGlobalRules;
            Type sourceType = typeof(TSource);
            Type destType = typeof(TDestination);

            var functionParamEx = Expression.Parameter(sourceType, "source");

            List<PropertyInfo> properties = GetCommonProperties(sourceType, destType, !hasRules);
            var assignmentExs = new List<MemberAssignment>(properties.Count);

            foreach (PropertyInfo property in properties)
            {
                var srcPropAccessEx = Expression.Property(functionParamEx, sourceType, property.Name);

                if (hasSpecificRules && specificRules.ContainsKey(property.Name))
                {
                    try
                    {
                        var ruleCallEx = Expression.Invoke(Expression.Constant(specificRules[property.Name]), srcPropAccessEx);
                        var bindingEx = Expression.Bind(property, ruleCallEx);
                        assignmentExs.Add(bindingEx);
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        throw new InvalidOperationException(
                            $"The specified rule for property '{property.Name}' does not produce correct data. " +
                            $"The rule must return a value of type '{property.PropertyType.FullName}'."
                        );
                    }
                }

                if (hasGlobalRules)
                {
                    Type srcPropType = sourceType.GetProperty(property.Name).PropertyType;
                    Type destPropType = property.PropertyType;
                    if (srcPropType != destPropType)
                    {
                        var hash = GetHash(srcPropType, destPropType);
                        if (globalRuleBuilder.GlobalRules.ContainsKey(hash))
                        {
                            var ruleCallEx = Expression.Invoke(Expression.Constant(globalRuleBuilder.GlobalRules[hash]), srcPropAccessEx);
                            var bindingEx = Expression.Bind(property, ruleCallEx);
                            assignmentExs.Add(bindingEx);
                            continue;
                        }
                        else if (globalRuleBuilder.IsGlobalToStringActive && destPropType == typeof(string))
                        {
                            var toStringDelegate = new Func<object, string>(src => src.ToString());
                            var ruleCallEx = Expression.Invoke(Expression.Constant(toStringDelegate), Expression.Convert(srcPropAccessEx, typeof(object)));
                            var bindingEx = Expression.Bind(property, ruleCallEx);
                            assignmentExs.Add(bindingEx);
                            continue;
                        }
                    }
                }

                try
                {
                    var bindingEx = Expression.Bind(property, srcPropAccessEx);
                    assignmentExs.Add(bindingEx);
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException($"Property '{property.Name}' has different data type in source and destination. A rule is needed for mapping this property.");
                }
            }

            var newEx = Expression.New(destType);
            var initExp = Expression.MemberInit(newEx, assignmentExs);

            return (Expression.Lambda<Func<TSource, TDestination>>(initExp, new[] { functionParamEx })).Compile();
        }

        private static List<PropertyInfo> GetCommonProperties(Type sourceType, Type destType, bool checkPropertyTypes)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            Func<PropertyInfo, bool> propertySetterCheck = (PropertyInfo p) => p.CanWrite;

            if (sourceType == destType) return destType.GetProperties(bindingFlags).Where(propertySetterCheck).ToList();

            var sourceProps = sourceType.GetProperties(bindingFlags);
            var destProps = destType.GetProperties(bindingFlags).Where(propertySetterCheck).ToArray();

            var result = new List<PropertyInfo>(Math.Max(sourceProps.Length, destProps.Length));

            for (int i = sourceProps.Length - 1; i >= 0; i--)
            {
                var srcProp = sourceProps[i];
                for (int j = destProps.Length - 1; j >= 0; j--)
                {
                    var destProp = destProps[j];
                    if (srcProp.Name == destProp.Name)
                    {
                        if (!checkPropertyTypes || (checkPropertyTypes && srcProp.PropertyType == destProp.PropertyType))
                        {
                            result.Add(destProp);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetHash(Type sourceType, Type destinationType)
        {
            return $"{sourceType.AssemblyQualifiedName}:{destinationType.AssemblyQualifiedName}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetHash<TSource, TDestination>()
        {
            return GetHash(typeof(TSource), typeof(TDestination));
        }

        internal static void ResetForNextTest()
        {
            funcCache.Clear();
            globalRuleBuilder = null;
        }

        #endregion Main Functionalities

        #region Fluent API

        public class TypeSpecificRuleBuider<TSource, TDestination>
        {
            // Key => Property name, Value => Mapping function
            private readonly Dictionary<string, Delegate> mappingRules = new Dictionary<string, Delegate>();

            public TypeSpecificRuleBuider<TSource, TDestination> WithRule<TSourceProperty, TDestinationProperty>(string propertyname, Func<TSourceProperty, TDestinationProperty> rule)
            {
                if (mappingRules.ContainsKey(propertyname))
                    throw new InvalidOperationException($"There is already a rule for property '{propertyname}' defined.");

                mappingRules.Add(propertyname, rule);
                return this;
            }

            public void Build()
            {
                if (GetCopyFunction<TSource, TDestination>() is not null)
                    throw new InvalidOperationException($"Mapping rules are already defined from {typeof(TSource).FullName} to {typeof(TDestination).FullName}.");
                PrepareMapperFunction<TSource, TDestination>(mappingRules);
            }
        }

        public class GlobalRuleBuilder
        {
            // Key => Hash code, Value => Mapping function
            internal Dictionary<string, Delegate> GlobalRules { get; } = new Dictionary<string, Delegate>();

            internal bool IsGlobalToStringActive { get; private set; } = false;

            public GlobalRuleBuilder WithRule<TSource, TDestination>(Func<TSource, TDestination> rule)
            {
                var hash = GetHash<TSource, TDestination>();
                if (GlobalRules.ContainsKey(hash))
                    throw new InvalidOperationException($"A global rule is already defined from '{typeof(TSource).FullName}' to '{typeof(TDestination).FullName}'.");

                GlobalRules.Add(hash, rule);
                return this;
            }

            public GlobalRuleBuilder WithGlobalToString()
            {
                if (IsGlobalToStringActive)
                    throw new InvalidOperationException("The global ToString rule is already defined.");
                IsGlobalToStringActive = true;
                return this;
            }

            public void Build()
            {
                if (globalRuleBuilder != null) throw new InvalidOperationException("Global rules are already defined.");
                if (funcCache.Count > 0) throw new InvalidOperationException("Global rules must be defined before type-specific rules are defined.");
                globalRuleBuilder = this;
            }
        }

        public static TypeSpecificRuleBuider<TSource, TDestination> BuildMapper<TSource, TDestination>()
        {
            return new TypeSpecificRuleBuider<TSource, TDestination>();
        }

        public static GlobalRuleBuilder BuildGlobalRules()
        {
            return new GlobalRuleBuilder();
        }

        #endregion Fluent API
    }
}