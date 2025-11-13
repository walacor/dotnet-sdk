using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Walacor_SDK;
using Xunit;

public partial class WalacorServiceTests
{
    [Fact]
    public void SchemaService_is_created_on_first_property_access_only()
    {
        var svc = new WalacorService("http://localhost", "user", "pass");

        var factoryField = typeof(WalacorService).GetField("_factory",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(factoryField);

        var factory = factoryField!.GetValue(svc);
        Assert.NotNull(factory);

        var dictField = factory.GetType().GetField("_singletons",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(dictField);

        var dict = dictField!.GetValue(factory) as ConcurrentDictionary<Type, Lazy<object>>;

        if (dict is null)
        {
            Assert.Null(dict);
        }
        else
        {
            Assert.Empty(dict);
        }

        var _ = svc.SchemaService;

        dict = dictField.GetValue(factory) as ConcurrentDictionary<Type, Lazy<object>>;
        Assert.NotNull(dict);
        Assert.Single(dict!);

        var lazy = dict!.Values.Single();
        Assert.True(lazy.IsValueCreated);
    }

    [Fact]
    public void Service_is_created_on_first_property_access_and_cached()
    {
        var svc = new WalacorService("http://localhost", "user", "pass");

        var s1 = svc.SchemaService;

        var s2 = svc.SchemaService;

        Assert.NotNull(s1);
        Assert.Same(s1, s2);
    }
}
