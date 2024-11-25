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
        IDictionary<Expression, Expression> mapsExpressions = new Dictionary<Expression, Expression>();

        Action<TSource, TTarget>? mapAction = null;
        Func<TSource, TTarget, bool>? diffAction = null;

        /// <summary>
        /// Define a mapping from the source type to the target type. Certain limitations apply due to the usage of Expression Trees.
        /// </summary>
        public Mapper<TSource, TTarget> ForMember<TTargetResult, TSourceResult>(Expression<Func<TTarget, TTargetResult>> target, Expression<Func<TSource, TSourceResult>> source)
        {
            mapsExpressions.Add(target, source);
            return this;
        }

        /// <summary>
        /// Maps the two passed instances as per defined in the declared rules.
        /// </summary>
        public bool Map(TSource source, TTarget target)
        {
            if (mapAction == null || diffAction == null)
            {
                throw new InvalidOperationException("Mapper not built");
            }

            if (diffAction(source, target))
            {
                mapAction(source, target);
                return true;
            }

            return false;
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
            Expression equalsEs = Expression.Constant(true);
            var targetParameter = Expression.Parameter(typeof(TTarget), "target");
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");

            foreach (var map in mapsExpressions)
            {
                var targetBody = GetExpressionBodyFromDelegateExpression(map.Key);
                var target = targetBody as MemberExpression;
                if (target == null)
                {
                    throw new InvalidOperationException("Can not map to target property");
                }
                var targetModifier = new ParameterModifier<TTarget>(targetParameter);
                var newTarget = targetModifier.Modify(target);
                var sourceBody = GetExpressionBodyFromDelegateExpression(map.Value);
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

                AddAssignment(exprList, newTarget, newSource);
                equalsEs = Expression.AndAlso(equalsEs, Expression.NotEqual(newTarget, newSource));
            }

            var mapLambda = Expression.Lambda<Action<TSource, TTarget>>(Expression.Block(exprList), new ParameterExpression[] { sourceParameter, targetParameter });
            mapAction = mapLambda.Compile();

            var diffLambda = Expression.Lambda<Func<TSource, TTarget, bool>>(equalsEs, new ParameterExpression[] { sourceParameter, targetParameter });
            diffAction = diffLambda.Compile();

            return this;
        }

        private void AddAssignment(List<Expression> list, Expression target, Expression source)
        {
            try
            {
                list.Add(Expression.Assign(target, source));
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

        private Expression GetExpressionBodyFromDelegateExpression(Expression target)
        {
            var bodyProp = target.GetType().GetProperty("Body");
            var body = bodyProp?.GetValue(target);
            var member = body as Expression;
            if (member == null)
            {
                throw new InvalidOperationException("Could not parse expression");
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
    }
}