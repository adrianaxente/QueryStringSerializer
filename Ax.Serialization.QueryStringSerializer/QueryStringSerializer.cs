using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Ax.Serialization.QueryStringSerializer
{
    public class QueryStringSerializer: IQueryStringSerializer
    {
        private IMetadataProvider _metadataProvider;

        private static readonly Lazy<ConcurrentDictionary<Type, SerializeDelegate>> _serializationDelegateDictionary =
            new Lazy<ConcurrentDictionary<Type, SerializeDelegate>>(
                () => new ConcurrentDictionary<Type, SerializeDelegate>(),
                true);

        public QueryStringSerializer(IMetadataProvider metadataProvider)
        {
            if (metadataProvider == null)
            {
                throw new ArgumentNullException(nameof(metadataProvider));
            }

            _metadataProvider = metadataProvider;
        }

        public string Serialize(object @object)
        {
            if (@object == null)
            {
                return null;
            }

            return GetSerializationDelegate(@object.GetType())(@object);
        }

        private SerializeDelegate GetSerializationDelegate(Type type)
        {
            return _serializationDelegateDictionary.Value.GetOrAdd(type, t => BuildSerializationDelegate(t));
        }

        //TODO: Implement Converter mechanism
        //TODO: Optimize the folowing method:
        //          - Don't use casts
        //          - In case of value types except nullable the null condition is useless
        //          - Find other optimization points
        private SerializeDelegate BuildSerializationDelegate(Type type)
        {
            var typeMetadata = _metadataProvider.GetMetadata(type);

            var stringBuilderConstructorInfo =
                typeof(StringBuilder)
                    .GetTypeInfo()
                    .DeclaredConstructors
                    .FirstOrDefault(c => c.GetParameters()?.Length == 0);

            var stringBuilderAppendWithStringMethodInfo =
                typeof(StringBuilder)
                    .GetRuntimeMethod(
                        nameof(StringBuilder.Append),
                        new Type[] { typeof(string) });
            
            var stringBuilderAppendWithObjectMethodInfo =
                typeof(StringBuilder)
                    .GetRuntimeMethod(
                        nameof(StringBuilder.Append),
                        new Type[] { typeof(object) });

            var stringBuilderToStringMethodInfo =
                typeof(StringBuilder)
                    .GetRuntimeMethod(
                        nameof(StringBuilder.ToString),
                        new Type[0]);
            
            var inputObject = Expression.Parameter(typeof(object));
            var inputObjectCastedVariable = Expression.Variable(typeMetadata.Type);
            var inputObjectCast = Expression.Convert(inputObject, typeMetadata.Type);
            var inputObjectCastedVariableAssigment = Expression.Assign(inputObjectCastedVariable, inputObjectCast);
            var stringBuilderVariable = Expression.Variable(typeof(StringBuilder));
            var stringBuilderCreation = Expression.New(stringBuilderConstructorInfo);
            var stringBuilderVariableAssigment = Expression.Assign(stringBuilderVariable, stringBuilderCreation);

            var variables = new List<ParameterExpression>()
            {
                inputObjectCastedVariable,
                stringBuilderVariable
            };


            var bodyList = new List<Expression>
            {
                inputObjectCastedVariableAssigment,
                stringBuilderVariableAssigment
            };

            foreach(var memberMetadata in typeMetadata.MembersMetadata)
            {

                var memberValueVariable = 
                    Expression.Variable(typeof(object));

                var memberValue =
                    Expression.PropertyOrField(
                            inputObjectCastedVariable,
                            memberMetadata.MemberInfo.Name);

                var memberValueCast =
                    Expression.Convert(
                        memberValue,
                        typeof(object));

                var memberValueAssigment =
                    Expression
                        .Assign(
                            memberValueVariable,
                            memberValueCast);

                variables.Add(memberValueVariable);
                bodyList.Add(memberValueAssigment);

                var ifValueNotNull =
                    Expression.IfThen(
                        Expression.NotEqual(
                            memberValueVariable,
                            Expression.Constant(null)),
                        Expression.Block(
                            new Expression[]
                            {
                                Expression.Call(
                                    stringBuilderVariable,
                                    stringBuilderAppendWithStringMethodInfo,
                                    new Expression[]
                                    {
                                        Expression.Constant(memberMetadata.ValueName)
                                    }),
                                Expression.Call(
                                    stringBuilderVariable,
                                    stringBuilderAppendWithStringMethodInfo,
                                    new Expression[]
                                    {
                                        Expression.Constant("=")
                                    }),
                                Expression.Call(
                                    stringBuilderVariable,
                                    stringBuilderAppendWithObjectMethodInfo,
                                    new Expression[]
                                    {
                                        memberValueVariable
                                    })

                            }));

                var ifStringBuilderLengthGreaterThanZero =
                    Expression.IfThen(
                        Expression.GreaterThan(
                            Expression.PropertyOrField(
                                stringBuilderVariable,
                                nameof(StringBuilder.Length)),
                            Expression.Constant(0)),
                        Expression.Call(
                            stringBuilderVariable,
                            stringBuilderAppendWithStringMethodInfo,
                            new Expression[]
                            {
                                Expression.Constant("&")
                            }));

                bodyList.Add(ifStringBuilderLengthGreaterThanZero);
                bodyList.Add(ifValueNotNull);
            }

            var result =
                Expression
                    .Call(
                        stringBuilderVariable,
                        stringBuilderToStringMethodInfo);
                
            bodyList.Add(result);

            var lambda =
                Expression.Lambda<SerializeDelegate>(
                    Expression.Block(
                        variables,
                        bodyList),
                    inputObject);

            return lambda.Compile();
        }

        private SerializeDelegate BuildSerializationDelegate_old(Type type)
        {
            var typeMetadata = _metadataProvider.GetMetadata(type);

            var result = new SerializeDelegate(o =>
            {
                var stringBuilder = new StringBuilder();

                foreach (var memberMetadata in typeMetadata.MembersMetadata)
                {
                    var memberValue =
                        memberMetadata.MemberInfo is FieldInfo
                                      ? (memberMetadata.MemberInfo as FieldInfo).GetValue(o) 
                                      : (memberMetadata.MemberInfo as PropertyInfo).GetValue(o);

                    if (memberValue == null)
                    {
                        continue;
                    }

                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("&");
                    }

                    stringBuilder
                        .Append(memberMetadata.ValueName)
                        .Append("=")
                        .Append(memberValue);
                }

                return stringBuilder.ToString();
            });

            return result;
        }
    }
}
