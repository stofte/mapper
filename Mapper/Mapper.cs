using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.AccessControl;

namespace Mapper
{
    /// <summary>
    /// Allows mapping between two classes and also determine if any changes where made to the target class.
    /// </summary>
    /// <typeparam name="TSource">Type to map from</typeparam>
    /// <typeparam name="TTarget">Type to map to</typeparam>
    public class Mapper<TSource, TTarget>
    {
        List<MemberMappingConfiguration> mapList = new List<MemberMappingConfiguration>();
        object[] comparerList = Array.Empty<object>();

        Delegate? mapAction = null;

        /// <summary>
        /// Define a mapping from the source type to the target type. Certain limitations apply due to the usage of Expression Trees.
        /// </summary>
        public Mapper<TSource, TTarget> ForMember<TResult>(Expression<Func<TTarget, TResult>> target, Expression<Func<TSource, TResult>> source)
        {
            mapList.Add(new MemberMappingConfiguration
            {
                Target = target,
                Source = source
            });
            return this;
        }

        public Mapper<TSource, TTarget> ForMember<TResult>(Expression<Func<TTarget, TResult>> target, Expression<Func<TSource, TResult>> source, IEqualityComparer<TResult> comparer)
        {
            mapList.Add(new MemberMappingConfiguration
            {
                Target = target,
                Source = source,
                Comparer = comparer
            });
            return this;
        }

        /// <summary>
        /// Maps the two passed instances as per defined in the declared rules.
        /// </summary>
        public bool Map(TSource source, TTarget target)
        {
            if (mapAction == null)
            {
                throw new InvalidOperationException("Mapper not built");
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var mapParams = new object[] { source, target }.Concat(comparerList).ToArray();
            var changedRes = mapAction.DynamicInvoke(mapParams);

            if (changedRes == null)
            {
                throw new InvalidOperationException("Internal error");
            }

            return (bool)changedRes;
        }

        /// <summary>
        /// Builds the mapper. Must be called before usage.
        /// </summary>
        public Mapper<TSource, TTarget> Build()
        {
            if (mapAction != null)
            {
                throw new InvalidOperationException("Mapper already built");
            }

            var exprList = new List<Expression>();
            var targetParameter = Expression.Parameter(typeof(TTarget), "target");
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");
            var comparerParameters = new Dictionary<object, Tuple<ParameterExpression, MethodInfo>>();
            var changedVar = Expression.Variable(typeof(bool), "changed");
            exprList.Add(Expression.Assign(changedVar, Expression.Constant(false)));

            foreach (var map in mapList)
            {
                var targetBody = GetExpressionBodyFromDelegateExpression(map.Target, isTarget: true);
                var target = targetBody as MemberExpression;
                if (target == null)
                {
                    throw new InvalidOperationException("Can not map to target property");
                }
                var targetModifier = new ParameterModifier<TTarget>(targetParameter);
                var newTarget = targetModifier.Modify(target);
                var sourceBody = GetExpressionBodyFromDelegateExpression(map.Source, isTarget: false);
                var sourceModifier = new ParameterModifier<TSource>(sourceParameter);
                var newSource = sourceModifier.Modify(sourceBody);

                // If we assign from foo to nullable<foo>, we do a convert as this should be always safe (?)
                if (newTarget.Type.IsConstructedGenericType)
                {
                    if (newTarget.Type.GetGenericTypeDefinition() == typeof(Nullable<>) && newTarget.Type.GenericTypeArguments.Single() == newSource.Type)
                    {
                        newSource = Expression.Convert(newSource, typeof(Nullable<>).MakeGenericType(newSource.Type));
                    }
                }

                Expression? condition = null;
                if (map.Comparer == null)
                {
                    // This is when we just do straight "x != y" for comparison
                    condition = Expression.NotEqual(newTarget, newSource);
                }
                else
                {
                    // See if we already have a comparer instance passed in here:
                    if (!comparerParameters.ContainsKey(map.Comparer))
                    {
                        var compType = map.Comparer.GetType();
                        var equalityCompInterface = compType.GetInterface("IEqualityComparer`1");
                        if (equalityCompInterface == null)
                        {
                            throw new InvalidOperationException("Could not find IEqualityComparer");
                        }
                        var foo = compType.GetMethods();
                        var flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
                        var equalsMethod = compType.GetMethod("Equals", flags);
                        if (equalsMethod == null)
                        {
                            throw new InvalidOperationException("Invalid IEqualityComparer");
                        }
                        comparerParameters.Add(map.Comparer, Tuple.Create(Expression.Parameter(compType), equalsMethod));
                    }

                    var comparerData = comparerParameters[map.Comparer];

                    condition = Expression.Not(Expression.Call(comparerData.Item1, comparerData.Item2, newTarget, newSource));
                }

                AddAssignment(exprList, condition, changedVar, newTarget, newSource);
            }

            exprList.Add(changedVar);
            comparerList = comparerParameters.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            var compParams = comparerParameters.OrderBy(x => x.Key).Select(x => x.Value.Item1);
            var mapParams = new ParameterExpression[] { sourceParameter, targetParameter }.Concat(compParams).ToArray();
            var mapLambda = Expression.Lambda(Expression.Block(new ParameterExpression[] { changedVar }, exprList), mapParams);
            mapAction = mapLambda.Compile();

            return this;
        }

        private void AddAssignment(List<Expression> list, Expression condition, ParameterExpression changedVar, Expression target, Expression source)
        {
            try
            {
                var assignChanged = Expression.Assign(changedVar, Expression.Constant(true));
                var assignTarget = Expression.Assign(target, source);
                var block = Expression.Block(assignChanged, assignTarget);
                var ifThen = Expression.IfThen(condition, block);
                list.Add(ifThen);
            }
            catch (ArgumentException exn) when (exn?.TargetSite?.Name == "RequiresCanWrite")
            {
                throw new InvalidOperationException($"Target member must be assignable");
            }
            catch (ArgumentException exn) when (exn?.TargetSite?.Name == "Assign" && exn.Message.Contains("cannot be used for assignment to type", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"Cannot map from type '{source.Type}' to type '{target.Type}'");
            }
        }

        private Expression GetExpressionBodyFromDelegateExpression(Expression expr, bool isTarget)
        {
            var bodyProp = expr.GetType().GetProperty("Body");
            var body = bodyProp?.GetValue(expr);
            var member = body as Expression;
            if (member == null)
            {
                throw new InvalidOperationException("Could not parse expression");
            }

            // When the to/from functions use implicitly convertible types (eg float to doubles),
            // the use of generic constraints (that both functions must return TResult) will cause
            // the generated expression to contain a Convert node. This will always appear on the
            // smaller type (eg the "float" property from above), and if this is the target property,
            // we could simply move the convert to the source property, but this could lead to
            // data loss, and so we instead throw an exception, and let the user cast manually
            if (isTarget && member is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
            {
                throw new InvalidOperationException("Can not map between members without explicit casting");
            }

            return member;
        }

        private class ParameterModifier<T> : ExpressionVisitor
        {
            private readonly ParameterExpression parameter;

            public ParameterModifier(ParameterExpression parameter)
            {
                this.parameter = parameter;
            }

            public Expression Modify(Expression expression)
            {
                return Visit(expression);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.NodeType == ExpressionType.MemberAccess && node.Member.DeclaringType == typeof(T))
                {
                    return Expression.MakeMemberAccess(parameter, node.Member);
                }
                return base.VisitMember(node);
            }
        }

        private class MemberMappingConfiguration
        {
            public Expression Target { get; set; }
            public Expression Source { get; set; }
            public object Comparer { get; set; } 
        }
    }
}