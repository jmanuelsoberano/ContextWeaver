using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Infrastructure;

/// <summary>
///     Registrador de tipos para Spectre.Console.Cli que utiliza Microsoft.Extensions.DependencyInjection.
/// </summary>
public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TypeRegistrar"/> class.
    /// </summary>
    /// <param name="builder">Service collection builder.</param>
    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = builder;
    }

    /// <inheritdoc />
    public ITypeResolver Build()
    {
        return new TypeResolver(_builder.BuildServiceProvider());
    }

    /// <inheritdoc />
    public void Register(Type service, Type implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    /// <inheritdoc />
    public void RegisterInstance(Type service, object implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    /// <inheritdoc />
    public void RegisterLazy(Type service, Func<object> factory)
    {
        _builder.AddSingleton(service, _ => factory());
    }
}

/// <summary>
///     Resolvedor de tipos para Spectre.Console.Cli que utiliza IServiceProvider.
/// </summary>
public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TypeResolver"/> class.
    /// </summary>
    /// <param name="provider">Service provider.</param>
    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return _provider.GetService(type);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
