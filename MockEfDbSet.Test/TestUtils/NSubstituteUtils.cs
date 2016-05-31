using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using NSubstitute;
using System;

namespace MockEfDbSet.Test.TestUtils
{
    public static class NSubstituteUtils
    {
        public static DbSet<T> CreateMockDbSet<T>(IEnumerable<T> data = null)
            where T: class
        {
            var mockSet = Substitute.For<MockableDbSetWithExtensions<T>, IQueryable<T>, IDbAsyncEnumerable<T>>();
            mockSet.AsNoTracking().Returns(mockSet);

            if (data != null)
            {
                var queryable = data.AsQueryable();

                // setup all IQueryable and IDbAsyncEnumerable methods using what you have from "data"
                // the setup below is a bit different from the test above
                ((IDbAsyncEnumerable<T>) mockSet).GetAsyncEnumerator()
                    .Returns(new TestDbAsyncEnumerator<T>(queryable.GetEnumerator()));
                ((IQueryable<T>) mockSet).Provider.Returns(new TestDbAsyncQueryProvider<T>(queryable.Provider));
                ((IQueryable<T>) mockSet).Expression.Returns(queryable.Expression);
                ((IQueryable<T>) mockSet).ElementType.Returns(queryable.ElementType);
                ((IQueryable<T>) mockSet).GetEnumerator().Returns(new TestDbEnumerator<T>(queryable.GetEnumerator()));
            }

            return mockSet;
        }

        public static T CreateMockEntity<T>(T entity)
        {
            var props = typeof(T).GetProperties().Where(p => !p.GetMethod.IsVirtual);
            var values = new List<object>();
            foreach (var prop in props)
            {
                values.Add(prop.GetValue(entity));
            }
            var mEntity = Substitute.For(new Type[] { typeof(T)}, values.ToArray());
            return (T)mEntity;
        }

        public static void SetMockVirtualProp<T, TResult>(T entity, string propName, Func<T, TResult> expr)
        {
            var prop = typeof(T).GetProperty(propName);
            if (!prop.GetMethod.IsVirtual)
            {
                throw new ArgumentException($"{propName} is not a virtual property.");
            }
            prop.GetValue(entity).Returns(expr(entity));
        }
    }
}
