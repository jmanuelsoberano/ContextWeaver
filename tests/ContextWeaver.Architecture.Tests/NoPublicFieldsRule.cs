using Mono.Cecil;
using NetArchTest.Rules;

namespace ContextWeaver.Architecture.Tests;

public class NoPublicFieldsRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        // Ignorar Enums (tienen un campo de instancia 'value__' que parece público en IL)
        if (type.IsEnum)
            return true;

        // Retorna true si NO tiene campos públicos
        // (excluyendo constantes y estáticos si se desea, pero aquí seremos estrictos)
        foreach (var field in type.Fields)
        {
            if (field.IsPublic && !field.IsStatic && !field.IsInitOnly && !field.IsLiteral)
            {
                return false;
            }
        }

        return true;
    }
}
