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
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private static readonly Dictionary<string, object> funcCache = new Dictionary<string, object>();
        private static GlobalRuleBuilder globalRuleBuilder = null;

        #region Main Functionalities

        public static TDestination DirectMap<TSource, TDestination>(this TSource source)
        {
            Type sourceType = typeof(TSource);
            Type destType = typeof(TDestination);
            return GetCopyFunction<TSource, TDestination>(sourceType, destType, null, true)(source);
        }

        public static IEnumerable<TDestination> DirectMapRange<TSource, TDestination>(this IEnumerable<TSource> source)
        {
            if (source == null) return null;

            Type sourceType = typeof(TSource);
            Type destType = typeof(TDestination);

            Func<TSource, TDestination> funcCopy = GetCopyFunction<TSource, TDestination>(sourceType, destType, null, true);
            return source.Select(s => funcCopy(s));
        }

        public static Func<TSource, TDestination> GetMapper<TSource, TDestination>()
        {
            Type sourceType = typeof(TSource);
            Type destType = typeof(TDestination);
            return GetCopyFunction<TSource, TDestination>(sourceType, destType, null, false);
        }

        private static Func<TSource, TDestination> GetCopyFunction<TSource, TDestination>(Type sourceType, Type destType, Dictionary<string, Delegate> rules, bool createIfNotExists)
        {
            var hash = GetHash(sourceType, destType);
            if (funcCache.ContainsKey(hash)) return (Func<TSource, TDestination>)funcCache[hash];

            Func<TSource, TDestination> result = null;

            semaphore.Wait();
            if (funcCache.ContainsKey(hash))
            {
                result = (Func<TSource, TDestination>)funcCache[hash];
            }
            else if (createIfNotExists)
            {
                result = CreateCopyFunction<TSource, TDestination>(sourceType, destType, rules);
                funcCache.Add(hash, result);
            }
            semaphore.Release();

            return result;
        }

        private static Func<TSource, TDestination> CreateCopyFunction<TSource, TDestination>(Type sourceType, Type destType, Dictionary<string, Delegate> rules)
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

            bool hasSpecificRules = rules != null && rules.Keys.Count > 0;
            bool hasGlobalRules = globalRuleBuilder != null;
            bool hasRules = hasSpecificRules || hasGlobalRules;

            var functionParamEx = Expression.Parameter(sourceType, "source");

            List<PropertyInfo> properties = GetCommonProperties(sourceType, destType, !hasRules);
            var assignmentExs = new List<MemberAssignment>(properties.Count);

            foreach (PropertyInfo property in properties)
            {
                var srcPropAccessEx = Expression.Property(functionParamEx, sourceType, property.Name);

                if (hasSpecificRules && rules.ContainsKey(property.Name))
                {
                    try
                    {
                        var ruleCallEx = Expression.Invoke(Expression.Constant(rules[property.Name]), srcPropAccessEx);
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
                    throw new InvalidOperationException($"Property '{property.Name}' has different data types in source and destination. A rule is needed.");
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
                if (funcCache.Count > 0) throw new InvalidOperationException("Defining type-specific rules must precede any mapping operation.");
                Type sourceType = typeof(TSource);
                Type destType = typeof(TDestination);
                GetCopyFunction<TSource, TDestination>(sourceType, destType, mappingRules, true);
            }
        }

        public class GlobalRuleBuilder
        {
            // Key => Hash code, Value => Mapping function
            internal Dictionary<string, Delegate> GlobalRules { get; private set; } = new Dictionary<string, Delegate>();

            internal bool IsGlobalToStringActive { get; private set; } = false;

            public GlobalRuleBuilder WithRule<TSourceType, TDestinationType>(Func<TSourceType, TDestinationType> rule)
            {
                Type sourceType = typeof(TSourceType);
                Type destType = typeof(TDestinationType);
                var hash = GetHash(sourceType, destType);

                if (GlobalRules.ContainsKey(hash))
                    throw new InvalidOperationException($"There is already a rule to convert '{sourceType.Name}' to '{destType.Name}' defined.");

                GlobalRules.Add(hash, rule);
                return this;
            }

            public GlobalRuleBuilder WithGlobalToString()
            {
                IsGlobalToStringActive = true;
                return this;
            }

            public void Build()
            {
                globalRuleBuilder = this;
            }
        }

        public static TypeSpecificRuleBuider<TSource, TDestination> BuildMapper<TSource, TDestination>()
        {
            return new TypeSpecificRuleBuider<TSource, TDestination>();
        }

        public static GlobalRuleBuilder BuildGlobalRules()
        {
            if (globalRuleBuilder != null) throw new InvalidOperationException("Global rules are already defined.");
            if (funcCache.Count > 0) throw new InvalidOperationException("Defining global rules must precede any mapping operation.");
            return new GlobalRuleBuilder();
        }

        #endregion Fluent API
    }
}
