using org.mariuszgromada.math.mxparser;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Expression = System.Linq.Expressions.Expression;
using MathExpression = org.mariuszgromada.math.mxparser.Expression;

namespace Ethereality.Converters
{
    public interface IAnyValueConverter : IValueConverter, IMultiValueConverter
    { }

    public class InternalMathConverter : IAnyValueConverter
    {
        private static readonly ConstructorInfo _argumentCtor;

        private readonly Func<double, Argument>? _arg_generator;

        private readonly Func<object[], Argument[]>? _args_generator;

        private readonly string _expr;

        static InternalMathConverter()
        {
            _argumentCtor = typeof(Argument).GetConstructor(
                BindingFlags.Public
                | BindingFlags.Instance,
                new Type[] { typeof(string), typeof(double) }
            )!;
        }

        public InternalMathConverter(string expr)
        {
            _expr = expr;

            var matches = Regex.Matches(_expr, @"\b[a-z]\b");

            var names = matches
                .Select(match => match)
                .Distinct()
                .ToArray();

            if (names.Length == 1)
            {
                var parameter = Expression.Parameter(typeof(double), "value");
                _arg_generator = Expression.Lambda<Func<double, Argument>>(
                    Expression.New(
                        _argumentCtor,
                        Expression.Constant(names.Single().Value),
                        parameter
                    ), parameter
                ).Compile();
            }
            else if (names.Length > 1)
            {
                var parameter = Expression.Parameter(typeof(object[]), "values");
                _args_generator = Expression.Lambda<Func<object[], Argument[]>>(
                    Expression.NewArrayInit(
                        typeof(Argument),
                        matches
                            .Select(match => match.Value)
                            .Distinct()
                            .Select((p, i) => Expression.New(
                                _argumentCtor,
                                Expression.Constant(p),
                                Expression.Convert(
                                    Expression.ArrayIndex(parameter, Expression.Constant(i)),
                                    typeof(double)
                                )
                            )
                        )
                    ), parameter
                ).Compile();
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double x) return DependencyProperty.UnsetValue;

            var arg = _arg_generator!(x);
            var e = new MathExpression(_expr, arg);
            var result = e.calculate();

            if (result.GetType() == targetType)
                return result;
            else
                return TypeDescriptor.GetConverter(targetType).ConvertFrom(result)!;
        }

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(v => v is not double)) return DependencyProperty.UnsetValue;

            var args = _args_generator!(values);
            var e = new MathExpression(_expr, args);
            var result = e.calculate();

            if (result.GetType() == targetType)
                return result;
            else
                return TypeDescriptor.GetConverter(targetType).ConvertFrom(result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [MarkupExtensionReturnType(typeof(IAnyValueConverter))]
    public class MathConverter : MarkupExtension
    {
        public MathConverter()
        { }

        public MathConverter(string expr)
        {
            Expr = expr;
        }

        [ConstructorArgument("expr")]
        public string Expr { get; set; } = null!;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new InternalMathConverter(Expr);
        }
    }
}