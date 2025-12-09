namespace LightHouse
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using System.Reflection;

    public static class TypeConstraint
    {
        // Base class for all constraints.
        public interface IConstraint { }
        // The first constraint in parameter constraint list. Doesn't act as an actual constraint, but more like a summary of all following constraints.
        public class ConstraintInfo : IConstraint
        {
            public bool NotClass = false;
            public bool NotInterface = false;
            public bool NotValueType = false;
            public bool NotArray = false;
            public bool NeedsDefault = false;
            // Pointer types are not allowed in constraints too.
            // public bool NotPointer = false;
            public Type FixedType = null;
            public ConstraintInfo() { }
        }
        // The constrained type must be equal to `Type`.
        public class Equivalent : IConstraint
        {
            // `Type` must not contain unbound generic parameters.
            public Type Type;
            public Equivalent(Type type) { Type = type; }
        }
        // The constrained type must be assignable to `Parent`. (`Parent` is an upper bound of the type.)
        public class UpperBound : IConstraint
        {
            // `Parent` must not contain unbound generic parameters.
            public Type Parent;
            public UpperBound(Type parent) { Parent = parent; }
        }
        // `Child` must be assignable to the constrained type. (`Child` is an upper bound of the type.)
        public class LowerBound : IConstraint
        {
            // `Child` must not contain unbound generic parameters.
            public Type Child;
            public LowerBound(Type child) { Child = child; }
        }
        // The constrained type must be a reference type.
        public class ReferenceType : IConstraint { }
        // The constrained type must be a non nullable value type.
        public class NotNullableValueType : IConstraint { }
        // The constrained type must have a parameterless constructor.
        public class DefaultConstructor : IConstraint { }

        // Fetch candidate types that can potentially satisfy all constraints if their type parameters are set.
        // If the returned type has no unbound type parameters, it is guaranteed to meet all constraints.
        // If the returned type has unbound type parameters, we will need to find appropriate type parameters.
        public static List<Type> GetCandidates(IConstraint[] constraints, IEnumerable<Type> types)
        {
            // `types` can be any type, but we only these types are considered valid candidates:
            // - Abstract and nonabstract classes.
            // - Interfaces.
            // - Value types (structs, enums, primitive types).
            // - Array types.
            // - And nested variants of the above.
            // - No pointer types.
            // - No type parameters.
            // - No constructed (or partially constructed) generic types (`Bar<int, float>`, `List<int>`, `Bar<T, float>`...).
            // We consider `T[]` to be conceptually equal to `SomeInternalArrayType<T>`.

            // All constraints should only contain types without unbound generic parameters.
            foreach (IConstraint constraint in constraints)
            {
                if (constraint is ConstraintInfo)
                {
                    // Ignore constraint info.
                    continue;
                }
                else if (constraint is Equivalent { Type: var constraintType })
                {
                    if (constraintType.ContainsGenericParameters)
                        throw new Exception();
                }
                else if (constraint is UpperBound { Parent: var parentType })
                {
                    if (parentType.ContainsGenericParameters)
                        throw new Exception();
                }
                else if (constraint is LowerBound { Child: var childType })
                {
                    if (childType.ContainsGenericParameters)
                        throw new Exception();
                }
                else if (constraint is ReferenceType)
                {
                    continue;
                }
                else if (constraint is NotNullableValueType)
                {
                    continue;
                }
                else if (constraint is DefaultConstructor)
                {
                    continue;
                }
                else
                    throw new Exception();
            }
            List<Type> candidates = new();
            foreach (Type type in types)
            {
                if (type.IsPointer)
                {
                    // We don't consider pointers as they are unsafe, and I don't really understand them at all. Sorry for my incompetence.
                    continue;
                }
                else if (!type.ContainsGenericParameters)
                {
                    // This type doesn't have any unbound type parameters.
                    // We don't want constructed generic types to appear in candidates, so filter them.
                    if (type.IsGenericType)
                        continue;
                    // Check if it satisfies all constraints by using `Type.IsAssignableFrom()`.
                    bool allConstraintsSatisfied = true;
                    foreach (IConstraint constraint in constraints)
                    {
                        if (constraint is ConstraintInfo)
                        {
                            // Ignore constraint info.
                            continue;
                        }
                        else if (constraint is Equivalent { Type: var constraintType })
                        {
                            if (type == constraintType)
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is UpperBound { Parent: var parentType })
                        {
                            if (parentType.IsAssignableFrom(type))
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is LowerBound { Child: var childType })
                        {
                            if (type.IsAssignableFrom(childType))
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is ReferenceType)
                        {
                            if (!type.IsValueType)
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is NotNullableValueType)
                        {
                            if (type.IsValueType)
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is DefaultConstructor)
                        {
                            if (type.GetConstructor(Type.EmptyTypes) != null)
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else
                            throw new Exception();
                    }
                    if (allConstraintsSatisfied)
                        candidates.Add(type);
                    continue;
                }
                else if (type.IsGenericParameter)
                {
                    // We don't consider type parameters, as they will always be considered valid candidates.
                    continue;
                }
                else if (type.IsGenericTypeDefinition)
                {
                    // This type is a generic type definition or an array or a pointer.
                    // We can't just use `Type.IsAssignableFrom()`, because we will be missing generic interface implementations that might possibly resolve to the constraint.
                    // So manually go over all implemented interfaces to see if there's a chance `type` implements all constraints.
                    bool allConstraintsSatisfied = true;
                    foreach (IConstraint constraint in constraints)
                    {
                        if (constraint is ConstraintInfo)
                        {
                            // Ignore constraint info.
                            continue;
                        }
                        else if (constraint is Equivalent { Type: var constraintType })
                        {
                            if (constraintType.IsGenericType && type == constraintType.GetGenericTypeDefinition())
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is UpperBound { Parent: var parentType })
                        {
                            if (parentType.IsArray)
                            {
                                // We are not an array type.
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsPointer)
                            {
                                // We are not a pointer type.
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsInterface)
                            {
                                // If it is an interface, check if `type` implements this interface.
                                bool constraintSatisfied = false;
                                // `type.GetInterfaces()` doesn't return `type` when `type.IsInterface`. So we explicitly add it here.
                                // It doesn't matter if `!type.IsInterface`, because `parentType` is an interface for sure, and we check if `interfaceType` is equal to `parentType`.
                                foreach (Type interfaceType in type.GetInterfaces().Append(type))
                                {
                                    if (interfaceType.IsGenericType && parentType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == parentType.GetGenericTypeDefinition()))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                    else if (!interfaceType.IsGenericType && !parentType.IsGenericType && (interfaceType == parentType))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                }
                                if (constraintSatisfied)
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsClass)
                            {
                                // If it is a class, follow up `type`'s parent classes to see if we have `parentType`.
                                bool constraintSatisfied = false;
                                for (Type currentType = type; currentType != null; currentType = currentType.BaseType)
                                {
                                    if (currentType.IsGenericType && parentType.IsGenericType && (currentType.GetGenericTypeDefinition() == parentType.GetGenericTypeDefinition()))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                    else if (!currentType.IsGenericType && !parentType.IsGenericType && (currentType == parentType))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                }
                                if (constraintSatisfied)
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsValueType)
                            {
                                if (parentType.IsGenericType && (type == parentType.GetGenericTypeDefinition()))
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            // I am unsure what kind of `parentType` will ever reach here.
                            throw new Exception();
                        }
                        else if (constraint is LowerBound { Child: var childType })
                        {
                            if (type.IsArray)
                            {
                                // `type` is a generic type definition, so it can't be an array.
                                throw new Exception();
                            }
                            else if (type.IsPointer)
                            {
                                // We alreay ruled out pointers.
                                throw new Exception();
                            }
                            else if (type.IsInterface)
                            {
                                // If it is an interface, check if `childType` implements this interface.
                                bool constraintSatisfied = false;
                                // `childType.GetInterfaces()` doesn't return `childType` when `childType.IsInterface`. So we explicitly add it here.
                                // It doesn't matter if `!childType.IsInterface`, because `type` is an interface for sure, and we check if `interfaceType` is equal to `type`.
                                foreach (Type interfaceType in childType.GetInterfaces().Append(childType))
                                {
                                    if (interfaceType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == type))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                }
                                if (constraintSatisfied)
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (type.IsClass)
                            {
                                // If it is a class, follow up `childType`'s parent classes to see if we have `type`.
                                bool constraintSatisfied = false;
                                for (Type currentType = childType; currentType != null; currentType = currentType.BaseType)
                                {
                                    if (currentType.IsGenericType && (currentType.GetGenericTypeDefinition() == type))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                }
                                if (constraintSatisfied)
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (type.IsValueType)
                            {
                                if (childType.IsGenericType && (type == childType.GetGenericTypeDefinition()))
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            // I am unsure what kind of `type` will ever reach here.
                            throw new Exception();
                        }
                        else if (constraint is ReferenceType)
                        {
                            if (!type.IsValueType)
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is NotNullableValueType)
                        {
                            if (type.IsValueType)
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is DefaultConstructor)
                        {
                            if (type.GetConstructor(Type.EmptyTypes) != null)
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else
                            throw new Exception();
                    }
                    if (allConstraintsSatisfied)
                        candidates.Add(type);
                    continue;
                }
                else if (type.IsArray)
                {
                    if (!type.GetElementType().IsGenericParameter)
                    {
                        // We don't consider arrays other then `T[]`, because they are conceptually (partially) constructed generic types.
                        continue;
                    }
                    bool allConstraintsSatisfied = true;
                    foreach (IConstraint constraint in constraints)
                    {
                        if (constraint is ConstraintInfo)
                        {
                            // Ignore constraint info.
                            continue;
                        }
                        else if (constraint is Equivalent { Type: var constraintType })
                        {
                            if (constraintType.IsArray && constraintType.GetArrayRank() == type.GetArrayRank())
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is UpperBound { Parent: var parentType })
                        {
                            if (parentType.IsArray)
                            {
                                if (parentType.GetArrayRank() == type.GetArrayRank())
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsPointer)
                            {
                                // We are not a pointer type.
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsInterface)
                            {
                                // If it is an interface, check if `type` implements this interface.
                                bool constraintSatisfied = false;
                                // `type.GetInterfaces()` doesn't return `type` when `type.IsInterface`. So we explicitly add it here.
                                // It doesn't matter if `!type.IsInterface`, because `parentType` is an interface for sure, and we check if `interfaceType` is equal to `parentType`.
                                foreach (Type interfaceType in type.GetInterfaces().Append(type))
                                {
                                    if (interfaceType.IsGenericType && parentType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == parentType.GetGenericTypeDefinition()))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                    else if (!interfaceType.IsGenericType && !parentType.IsGenericType && (interfaceType == parentType))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                }
                                if (constraintSatisfied)
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsClass)
                            {
                                // Arrays don't inherit from classes... except for `System.Array`. I'll just leave this part.
                                // If it is a class, follow up `type`'s parent classes to see if we have `parentType`.
                                bool constraintSatisfied = false;
                                for (Type currentType = type; currentType != null; currentType = currentType.BaseType)
                                {
                                    if (currentType.IsGenericType && parentType.IsGenericType && (currentType.GetGenericTypeDefinition() == parentType.GetGenericTypeDefinition()))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                    else if (!currentType.IsGenericType && !parentType.IsGenericType && (currentType == parentType))
                                    {
                                        constraintSatisfied = true;
                                        break;
                                    }
                                }
                                if (constraintSatisfied)
                                    continue;
                                allConstraintsSatisfied = false;
                                break;
                            }
                            else if (parentType.IsValueType)
                            {
                                // We are not a value type, and value types are never parents.
                                allConstraintsSatisfied = false;
                                break;
                            }
                            // I am unsure what kind of `parentType` will ever reach here.
                            throw new Exception();
                        }
                        else if (constraint is LowerBound { Child: var childType })
                        {
                            // Arrays are not inheritable, so we should be equiavalent to `childType`.
                            if (childType.IsArray && childType.GetArrayRank() == type.GetArrayRank())
                                continue;
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is ReferenceType)
                        {
                            // Arrays are actually reference types (I found this shocking).
                            if (!type.IsValueType)
                                continue;
                            // This will always pass, but just to be safe :)
                            throw new Exception();
                        }
                        else if (constraint is NotNullableValueType)
                        {
                            if (type.IsValueType)
                                // This will never pass, but just to be safe :)
                                throw new Exception();
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else if (constraint is DefaultConstructor)
                        {
                            // Arrays don't have parameterless default constructors.
                            if (type.GetConstructor(Type.EmptyTypes) != null)
                                // This will never pass, but just to be safe :)
                                throw new Exception();
                            allConstraintsSatisfied = false;
                            break;
                        }
                        else
                            throw new Exception();
                    }
                    if (allConstraintsSatisfied)
                        candidates.Add(type);
                    continue;
                }
                // `type` is not closed, but it isn't a type parameter, not a definition, and not an array.
                // Thus `type` is a partially constructed generic type, and we don't want them.
            }
            return candidates;
        }

        // Applies `constraints` to a generic type `type`, and returns constraints for type parameters that satisfies all.
        public static Dictionary<Type, List<IConstraint>> PropagateConstraints(Type type, IConstraint[] constraints)
        {
            // `type` should be one of the following:
            // - Abstract or nonabstract class.
            // - Interface.
            // - Value type (structs, enums, primitive type).
            // - Array type.
            // - And nested variant of the above.
            // - No pointer types.
            // - No type parameters.
            // We consider `T[]` to be conceptually equal to `SomeInternalArrayType<T>`.
            // And under this, `type` can be nongeneric, or a generic type definition.
            // We don't allow constructed or partially constructed generic types, since that will impose dependencies between type arguments.
            // That means the only generic array type allowed is `T[]`. No `T[][]` or `Foo<T>[]`, as they are "partially constructed generic types".

            // All constraints should only contain types without unbound generic parameters.
            foreach (IConstraint constraint in constraints)
            {
                if (constraint is ConstraintInfo)
                {
                    // Ignore constraint info.
                    continue;
                }
                else if (constraint is Equivalent { Type: var constraintType })
                {
                    if (constraintType.ContainsGenericParameters)
                        throw new Exception();
                }
                else if (constraint is UpperBound { Parent: var parentType })
                {
                    if (parentType.ContainsGenericParameters)
                        throw new Exception();
                }
                else if (constraint is LowerBound { Child: var childType })
                {
                    if (childType.ContainsGenericParameters)
                        throw new Exception();
                }
                else if (constraint is ReferenceType)
                {
                    continue;
                }
                else if (constraint is NotNullableValueType)
                {
                    continue;
                }
                else if (constraint is DefaultConstructor)
                {
                    continue;
                }
                else
                    throw new Exception();
            }
            // Prepare the parameter constraint dictionary.
            Dictionary<Type, List<IConstraint>> parameterConstraints = new();
            Type[] typeParameters;
            if (type.IsGenericTypeDefinition)
                typeParameters = type.GetGenericArguments();
            else if (type.IsArray)
            {
                if (!type.GetElementType().IsGenericParameter)
                    throw new Exception();
                typeParameters = new Type[] { type.GetElementType() };
            }
            else
            {
                // `type` should either be a generic type definition or a `T[]`.
                throw new Exception();
            }
            foreach (Type typeParameter in typeParameters)
            {
                // All type arguments must already be unbound parameters, but just to be sure.
                if (!typeParameter.IsGenericParameter)
                    throw new Exception();
                if (parameterConstraints.ContainsKey(typeParameter))
                    throw new Exception();
                parameterConstraints[typeParameter] = new List<IConstraint> { new ConstraintInfo() };
                // If any `AddContraint()` call fails, this means the type parameter has an insatisfiable constraint, which should be prohibited by the compiler.
                // That means I missed something in my code; throw an exception.
                foreach (Type parentType in typeParameter.GetGenericParameterConstraints())
                {
                    if (!TryAddConstraint(typeParameter, new UpperBound(parentType), parameterConstraints))
                        throw new Exception();
                }
                var specialConstraint = typeParameter.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
                if ((specialConstraint & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                    if (!TryAddConstraint(typeParameter, new ReferenceType(), parameterConstraints))
                        throw new Exception();
                if ((specialConstraint & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                    if (!TryAddConstraint(typeParameter, new NotNullableValueType(), parameterConstraints))
                        throw new Exception();
                if ((specialConstraint & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                    if (!TryAddConstraint(typeParameter, new DefaultConstructor(), parameterConstraints))
                        throw new Exception();
            }

            foreach (IConstraint constraint in constraints)
            {
                if (constraint is ConstraintInfo)
                {
                    // Ignore constraint info.
                    continue;
                }
                else if (constraint is Equivalent { Type: var constraintType })
                {
                    if (!TryApplyEquivalent(type, constraintType, ref parameterConstraints))
                        return null;
                }
                else if (constraint is UpperBound { Parent: var parentType })
                {
                    if (!TryApplyUpperBound(type, parentType, ref parameterConstraints))
                        return null;
                }
                else if (constraint is LowerBound { Child: var childType })
                {
                    if (!TryApplyLowerBound(type, childType, ref parameterConstraints))
                        return null;
                }
                else if (constraint is ReferenceType)
                {
                    if (type.IsValueType)
                        return null;
                }
                else if (constraint is NotNullableValueType)
                {
                    if (!type.IsValueType)
                        return null;
                }
                else if (constraint is DefaultConstructor)
                {
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        return null;
                }
                else
                    throw new Exception();
            }
            return parameterConstraints;
        }

        // Find constraints for type parameters to make `type` and `target` equivalent.
        // Generated constraints will always be `Equivalent`.
        static bool TryApplyEquivalent(Type type, Type target, ref Dictionary<Type, List<IConstraint>> parameterConstraints)
        {
            // We assume `target` to not contain unbound type parameters.
            // `target` can be anything except a pointer type:
            // - Abstract or nonabstract class.
            // - Interface.
            // - Value type (structs, enums, primitive type).
            // - Array type.
            // - And nested variant of the above.
            // - No pointer types.
            // So we are allowing constructed generic types too.
            // `type` can be everything that 'target' can be, and additionaly it can contain unbound type parameters.
            // We will constrain all of them.

            // Nongeneric, type parameter, array, class, interface, value type.

            // `target` should not contain unbound type parameters.
            if (target.ContainsGenericParameters)
                throw new Exception();

            if (type.IsPointer || target.IsPointer)
            {
                throw new Exception();
            }
            if (!type.ContainsGenericParameters)
            {
                // `type` doesn't have any unbound type parameters too.
                return type == target;
            }
            else if (type.IsGenericParameter)
            {
                // `type` itself is a generic parameter.
                return TryAddConstraint(type, new Equivalent(target), parameterConstraints);
            }
            else if (type.IsArray)
            {
                if (!target.IsArray)
                    return false;
                if (type.GetArrayRank() != target.GetArrayRank())
                    return false;
                return TryApplyEquivalent(type.GetElementType(), target.GetElementType(), ref parameterConstraints);
            }
            else if (type.IsPointer)
            {
                // Not reachable, but I like this branch of if-elseif statements, so... :P
                throw new Exception();
            }
            // Since `type` contains generic paramters, is not a generic parameter itself, and is not an array nor a pointer, `type` is generic.
            else if (type.IsValueType || type.IsClass || type.IsInterface)
            {
                if (!target.IsGenericType || (type.GetGenericTypeDefinition() != target.GetGenericTypeDefinition()))
                    return false;
                var typeArguments = type.GetGenericArguments();
                var targetArguments = target.GetGenericArguments();
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    if (!TryApplyEquivalent(typeArguments[i], targetArguments[i], ref parameterConstraints))
                        return false;
                }
                return true;
            }
            // I can't think of a `type` that can ever reach here.
            throw new Exception();
        }

        // Find constraints for type parameters to make `parent` an upper bound of `type`.
        // Generated constraints will always be `Equivalent`, `UpperBound`, or `LowerBound`.
        static bool TryApplyUpperBound(Type type, Type parent, ref Dictionary<Type, List<IConstraint>> parameterConstraints)
        {
            // We assume `parent` to not contain unbound type parameters.
            // `parent` can be anything except a pointer type:
            // - Abstract or nonabstract class.
            // - Interface.
            // - Value type (structs, enums, primitive type).
            // - Array type.
            // - And nested variant of the above.
            // - No pointer types.
            // So we are allowing constructed generic types too.
            // `type` can be everything that 'parent' can be, and additionaly it can contain unbound type parameters.
            // We will constrain all of them.

            // Nongeneric, type parameter, array, class, interface, value type.

            // `parent` should not contain unbound type parameters.
            if (parent.ContainsGenericParameters)
                throw new Exception();

            if (type.IsPointer || parent.IsPointer)
            {
                throw new Exception();
            }
            if (!type.ContainsGenericParameters)
            {
                // `type` doesn't have any unbound type parameters too.
                return parent.IsAssignableFrom(type);
            }
            else if (type.IsGenericParameter)
            {
                // `type` itself is a generic parameter.
                return TryAddConstraint(type, new UpperBound(parent), parameterConstraints);
            }
            // From here, `type` is one of array, interface, class, or value type.
            if (parent.IsArray)
            {
                if (!type.IsArray)
                    return false;
                if (type.GetArrayRank() != parent.GetArrayRank())
                    return false;
                return TryApplyEquivalent(type.GetElementType(), parent.GetElementType(), ref parameterConstraints);
            }
            else if (parent.IsPointer)
            {
                // Not reachable.
                throw new Exception();
            }
            else if (parent.IsInterface)
            {
                // A type may implement two different variants of a single interface.
                // So even when one seemingly promising match was rejected, we still need to try all other interfaces.
                // Rarely a type may have two ways to satisfy a single constraint... but we don't consider that case.

                foreach (Type interfaceType in type.GetInterfaces().Append(type))
                {
                    if (interfaceType.IsGenericType && parent.IsGenericType && (interfaceType.GetGenericTypeDefinition() == parent.GetGenericTypeDefinition()))
                    {
                        var clonedParameterConstraints = parameterConstraints.ToDictionary(entry => entry.Key, entry => entry.Value.ToList());
                        var typeArguments = interfaceType.GetGenericArguments();
                        var parentArguments = parent.GetGenericArguments();
                        var variances = parent.GetGenericTypeDefinition().GetGenericArguments().Select((type) => (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)).ToList();
                        bool success = true;
                        for (int i = 0; i < typeArguments.Length; i++)
                        {
                            var typeArgument = typeArguments[i];
                            var parentArgument = parentArguments[i];
                            var variance = variances[i];
                            // C# doesn't want generic types to silently box (and allocate) value types in references.
                            // This can happen when we attempt to assign value-type to a covariant reference-type type parameter.
                            // Or when assigning a reference-type to a contravariant value-type type parameter.
                            // Both are prohibited by type system, so we don't want to consider them as valid options.
                            if (variance == GenericParameterAttributes.None)
                            {
                                if (!TryApplyEquivalent(typeArgument, parentArgument, ref clonedParameterConstraints))
                                {
                                    success = false;
                                    break;
                                }
                            }
                            else if (variance == GenericParameterAttributes.Covariant)
                            {
                                // If any of `parentArgument` or `typeArgument` is a value type, this should be considered a non-variant case.
                                if (parentArgument.IsValueType)
                                {
                                    // `parentArgument` is a value type. Let the recursive `TryApplyEquivalent()` call do the job.
                                    if (!TryApplyEquivalent(typeArgument, parentArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    // `parentArgument` is not a value type. Since reference types can't be equivalent to value types, `typeArgument` must not be a value type.
                                    if (typeArgument.IsGenericParameter)
                                    {
                                        // If `typeArgument` is a generic parameter, this is as easy as adding the `ReferenceType` constraint.
                                        if (!TryAddConstraint(typeArgument, new ReferenceType(), clonedParameterConstraints))
                                        {
                                            success = false;
                                            break;
                                        }
                                    }
                                    else if (typeArgument.IsValueType)
                                    {
                                        // If `typeArgument` is a value type, but `parentArgument` isn't, we are doomed.
                                        success = false;
                                        break;
                                    }
                                    // If we made this far, `typeArgument` is indeed a reference type.
                                    if (!TryApplyUpperBound(typeArgument, parentArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                            }
                            else if (variance == GenericParameterAttributes.Contravariant)
                            {
                                // If any of `parentArgument` or `typeArgument` is a value type, this should be considered a non-variant case.
                                if (parentArgument.IsValueType)
                                {
                                    // `parentArgument` is a value type. Let the recursive `TryApplyEquivalent()` call do the job.
                                    if (!TryApplyEquivalent(typeArgument, parentArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    // `parentArgument` is not a value type. Since reference types can't be equivalent to value types, `typeArgument` must not be a value type.
                                    if (typeArgument.IsGenericParameter)
                                    {
                                        // If `typeArgument` is a generic parameter, this is as easy as adding the `ReferenceType` constraint.
                                        if (!TryAddConstraint(typeArgument, new ReferenceType(), clonedParameterConstraints))
                                        {
                                            success = false;
                                            break;
                                        }
                                    }
                                    else if (typeArgument.IsValueType)
                                    {
                                        // If `typeArgument` is a value type, but `parentArgument` isn't, we are doomed.
                                        success = false;
                                        break;
                                    }
                                    // If we made this far, `typeArgument` is indeed a reference type.
                                    if (!TryApplyLowerBound(typeArgument, parentArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                            }
                            else
                                throw new Exception();
                        }
                        if (success)
                        {
                            parameterConstraints = clonedParameterConstraints;
                            return true;
                        }
                    }
                    else if (!interfaceType.IsGenericType && !parent.IsGenericType && (interfaceType == parent))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (parent.IsClass)
            {
                for (Type currentType = type; currentType != null; currentType = currentType.BaseType)
                {
                    if (currentType.IsGenericType && parent.IsGenericType && (currentType.GetGenericTypeDefinition() == parent.GetGenericTypeDefinition()))
                    {
                        var typeArguments = currentType.GetGenericArguments();
                        var parentArguments = parent.GetGenericArguments();
                        var variances = parent.GetGenericTypeDefinition().GetGenericArguments().Select((type) => (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)).ToList();
                        for (int i = 0; i < typeArguments.Length; i++)
                        {
                            var typeArgument = typeArguments[i];
                            var parentArgument = parentArguments[i];
                            var variance = variances[i];
                            if (variance == GenericParameterAttributes.None)
                            {
                                if (!TryApplyEquivalent(typeArgument, parentArgument, ref parameterConstraints))
                                    return false;
                            }
                            // Only generic interfaces allow variance.
                            else
                                throw new Exception();
                        }
                        return true;
                    }
                    if (!currentType.IsGenericType && !parent.IsGenericType && (currentType == parent))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (parent.IsValueType)
            {
                if (!type.IsValueType)
                    return false;
                // Since `type` contains generic parmeters and is a value type (thus isn't an array nor a pointer type), it should be generic.
                if (!parent.IsGenericType || type.GetGenericTypeDefinition() != parent.GetGenericTypeDefinition())
                    return false;
                var typeArguments = type.GetGenericArguments();
                var parentArguments = parent.GetGenericArguments();
                var variances = parent.GetGenericTypeDefinition().GetGenericArguments().Select((type) => (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)).ToList();
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    var typeArgument = typeArguments[i];
                    var parentArgument = parentArguments[i];
                    var variance = variances[i];
                    if (variance == GenericParameterAttributes.None)
                    {
                        if (!TryApplyEquivalent(typeArgument, parentArgument, ref parameterConstraints))
                            return false;
                    }
                    // Only generic interfaces allow variance.
                    else
                        throw new Exception();
                }
                return true;
            }
            // I am unsure what kind of `type` will ever reach here.
            throw new Exception();
        }

        // Find constraints for type parameters to make `child` a lower bound of `type`.
        // Generated constraints will always be `Equivalent`, `UpperBound`, or `LowerBound`.
        static bool TryApplyLowerBound(Type type, Type child, ref Dictionary<Type, List<IConstraint>> parameterConstraints)
        {
            /// We assume `child` to not contain unbound type parameters.
            // `child` can be anything except a pointer type:
            // - Abstract or nonabstract class.
            // - Interface.
            // - Value type (structs, enums, primitive type).
            // - Array type.
            // - And nested variant of the above.
            // - No pointer types.
            // So we are allowing constructed generic types too.
            // `type` can be everything that 'child' can be, and additionaly it can contain unbound type parameters.
            // We will constrain all of them.

            // Nongeneric, type parameter, array, class, interface, value type.

            // `child` should not contain unbound type parameters.
            if (child.ContainsGenericParameters)
                throw new Exception();

            if (type.IsPointer || child.IsPointer)
            {
                throw new Exception();
            }
            if (!type.ContainsGenericParameters)
            {
                // `type` doesn't have any unbound type parameters too.
                return type.IsAssignableFrom(child);
            }
            else if (type.IsGenericParameter)
            {
                // `type` itself is a generic parameter.
                return TryAddConstraint(type, new LowerBound(child), parameterConstraints);
            }
            if (type.IsArray)
            {
                if (!child.IsArray)
                    return false;
                if (type.GetArrayRank() != child.GetArrayRank())
                    return false;
                return TryApplyEquivalent(type.GetElementType(), child.GetElementType(), ref parameterConstraints);
            }
            else if (type.IsPointer)
            {
                // Not reachable.
                throw new Exception();
            }
            else if (type.IsInterface)
            {
                // A type may implement two different variants of a single interface.
                // So even when one seemingly promising match was rejected, we still need to try all other interfaces.
                // Rarely a type may have two ways to satisfy a single constraint... but we don't consider that case.

                foreach (Type interfaceType in child.GetInterfaces().Append(child))
                {
                    if (interfaceType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()))
                    {
                        var clonedParameterConstraints = parameterConstraints.ToDictionary(entry => entry.Key, entry => entry.Value.ToList());
                        var typeArguments = type.GetGenericArguments();
                        var childArguments = child.GetGenericArguments();
                        var variances = child.GetGenericTypeDefinition().GetGenericArguments().Select((type) => (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)).ToList();
                        bool success = true;
                        for (int i = 0; i < typeArguments.Length; i++)
                        {
                            var typeArgument = typeArguments[i];
                            var childArgument = childArguments[i];
                            var variance = variances[i];
                            if (variance == GenericParameterAttributes.None)
                            {
                                if (!TryApplyEquivalent(typeArgument, childArgument, ref clonedParameterConstraints))
                                {
                                    success = false;
                                    break;
                                }
                            }
                            else if (variance == GenericParameterAttributes.Covariant)
                            {
                                // If any of `childArgument` or `typeArgument` is a value type, this should be considered a non-variant case.
                                if (childArgument.IsValueType)
                                {
                                    // `childArgument` is a value type. Let the recursive `TryApplyEquivalent()` call do the job.
                                    if (!TryApplyEquivalent(typeArgument, childArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    // `childArgument` is not a value type. Since reference types can't be equivalent to value types, `typeArgument` must not be a value type.
                                    if (typeArgument.IsGenericParameter)
                                    {
                                        // If `typeArgument` is a generic parameter, this is as easy as adding the `ReferenceType` constraint.
                                        if (!TryAddConstraint(typeArgument, new ReferenceType(), clonedParameterConstraints))
                                        {
                                            success = false;
                                            break;
                                        }
                                    }
                                    else if (typeArgument.IsValueType)
                                    {
                                        // If `typeArgument` is a value type, but `childArgument` isn't, we are doomed.
                                        success = false;
                                        break;
                                    }
                                    // If we made this far, `typeArgument` is indeed a reference type.
                                    if (!TryApplyLowerBound(typeArgument, childArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                            }
                            else if (variance == GenericParameterAttributes.Contravariant)
                            {
                                // If any of `childArgument` or `typeArgument` is a value type, this should be considered a non-variant case.
                                if (childArgument.IsValueType)
                                {
                                    // `childArgument` is a value type. Let the recursive `TryApplyEquivalent()` call do the job.
                                    if (!TryApplyEquivalent(typeArgument, childArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    // `childArgument` is not a value type. Since reference types can't be equivalent to value types, `typeArgument` must not be a value type.
                                    if (typeArgument.IsGenericParameter)
                                    {
                                        // If `typeArgument` is a generic parameter, this is as easy as adding the `ReferenceType` constraint.
                                        if (!TryAddConstraint(typeArgument, new ReferenceType(), clonedParameterConstraints))
                                        {
                                            success = false;
                                            break;
                                        }
                                    }
                                    else if (typeArgument.IsValueType)
                                    {
                                        // If `typeArgument` is a value type, but `childArgument` isn't, we are doomed.
                                        success = false;
                                        break;
                                    }
                                    // If we made this far, `typeArgument` is indeed a reference type.
                                    if (!TryApplyUpperBound(typeArgument, childArgument, ref clonedParameterConstraints))
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                            }
                            else
                                throw new Exception();
                        }
                        if (success)
                        {
                            parameterConstraints = clonedParameterConstraints;
                            return true;
                        }
                    }
                }
                return false;
            }
            if (type.IsClass)
            {
                for (Type currentType = child; currentType != null; currentType = currentType.BaseType)
                {
                    if (currentType.IsGenericType && (currentType.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()))
                    {
                        var typeArguments = type.GetGenericArguments();
                        var childArguments = child.GetGenericArguments();
                        var variances = child.GetGenericTypeDefinition().GetGenericArguments().Select((type) => (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)).ToList();
                        for (int i = 0; i < typeArguments.Length; i++)
                        {
                            var typeArgument = typeArguments[i];
                            var childArgument = childArguments[i];
                            var variance = variances[i];
                            if (variance == GenericParameterAttributes.None)
                            {
                                if (!TryApplyEquivalent(typeArgument, childArgument, ref parameterConstraints))
                                    return false;
                            }
                            // Only generic interfaces allow variance.
                            else
                                throw new Exception();
                        }
                        return true;
                    }
                }
                return false;
            }
            if (type.IsValueType)
            {
                if (!child.IsGenericType || type.GetGenericTypeDefinition() != child.GetGenericTypeDefinition())
                    return false;
                var typeArguments = type.GetGenericArguments();
                var childArguments = child.GetGenericArguments();
                var variances = child.GetGenericTypeDefinition().GetGenericArguments().Select((type) => (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)).ToList();
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    var typeArgument = typeArguments[i];
                    var childArgument = childArguments[i];
                    var variance = variances[i];
                    if (variance == GenericParameterAttributes.None)
                    {
                        if (!TryApplyEquivalent(typeArgument, childArgument, ref parameterConstraints))
                            return false;
                    }
                    // Only generic interfaces allow variance.
                    else
                        throw new Exception();
                }
                return true;
            }
            // I am unsure what kind of `type` will ever reach here.
            throw new Exception();
        }

        // Add a constraint to `typeParameter`, checking for any colliding constraints.
        // Ex. two `Equivalent`, an `Equivalent` with a `Type` that doesn't respect a `UpperBound`.
        static bool TryAddConstraint(Type typeParameter, IConstraint constraint, Dictionary<Type, List<IConstraint>> parameterConstraints)
        {
            // `constraint` must not contain unbound type parameters.
            // And `constraint` should not contain a pointer type.
            if (constraint is ConstraintInfo)
            {
                // Ignore constraint info.
            }
            else if (constraint is Equivalent { Type: var constraintType })
            {
                if (constraintType.ContainsGenericParameters || constraintType.IsPointer)
                    throw new Exception();
            }
            else if (constraint is UpperBound { Parent: var parentType })
            {
                if (parentType.ContainsGenericParameters || parentType.IsPointer)
                    throw new Exception();
            }
            else if (constraint is LowerBound { Child: var childType })
            {
                if (childType.ContainsGenericParameters || childType.IsPointer)
                    throw new Exception();
            }
            else if (constraint is ReferenceType)
            {
            }
            else if (constraint is NotNullableValueType)
            {
            }
            else if (constraint is DefaultConstructor)
            {
            }
            else
                throw new Exception();
            // Trust me bro. The first constraint is always the info.
            ConstraintInfo info = (ConstraintInfo)parameterConstraints[typeParameter][0];
            if (constraint is ConstraintInfo)
            {
                // We can't add constraint infos with this function.
                throw new Exception();
            }
            else if (constraint is Equivalent { Type: var constraintType })
            {
                if (info.FixedType != null)
                {
                    if (info.FixedType == constraintType)
                        return true;
                    // Two equivalent constraints, but they mismatch.
                    return false;
                }
                // An equivalent constraint has just arrived!
                info.FixedType = constraintType;
                // Check if the fixed type satisfies all previous constraints.

                // We use `info` to rule out invalid constraints early on, before we iterate over all constraints.
                if (info.FixedType.IsClass)
                {
                    if (info.NotClass)
                        return false;
                    info.NotInterface = true;
                    info.NotValueType = true;
                    info.NotArray = true;
                }
                else if (info.FixedType.IsInterface)
                {
                    if (info.NotInterface)
                        return false;
                    info.NotClass = true;
                    info.NotValueType = true;
                    info.NotArray = true;
                }
                else if (info.FixedType.IsValueType)
                {
                    if (info.NotValueType)
                        return false;
                    info.NotClass = true;
                    info.NotInterface = true;
                    info.NotArray = true;
                }
                else if (info.FixedType.IsArray)
                {
                    if (info.NotArray)
                        return false;
                    info.NotClass = true;
                    info.NotInterface = true;
                    info.NotValueType = true;
                }
                else if (info.FixedType.IsPointer)
                {
                    // Constraint types can't be pointers.
                    return false;
                }
                else
                    throw new Exception();

                if (info.NeedsDefault && info.FixedType.GetConstructor(Type.EmptyTypes) == null)
                    return false;

                // The quick check passed; time for iteration.
                foreach (IConstraint otherConstraint in parameterConstraints[typeParameter])
                {
                    if (otherConstraint is ConstraintInfo)
                    {
                        continue;
                    }
                    else if (otherConstraint is Equivalent { Type: var otherConstraintType })
                    {
                        // We just checked that we are the first equivalent constraint. There's no way we have a previous one.
                        throw new Exception();
                    }
                    else if (otherConstraint is UpperBound { Parent: var otherParentType })
                    {
                        if (!otherParentType.IsAssignableFrom(info.FixedType))
                            return false;
                    }
                    else if (otherConstraint is LowerBound { Child: var otherChildType })
                    {
                        if (!info.FixedType.IsAssignableFrom(otherChildType))
                            return false;
                    }
                    else if (otherConstraint is ReferenceType)
                    {
                        // We already checked these constraints with the flags.
                        continue;
                    }
                    else if (otherConstraint is NotNullableValueType)
                    {
                        // We already checked these constraints with the flags.
                        continue;
                    }
                    else if (otherConstraint is DefaultConstructor)
                    {
                        // We already checked these constraints with the flags.
                        continue;
                    }
                    else
                        throw new Exception();
                }
                // All constraints passed. Now flush all constraints.
                parameterConstraints[typeParameter] = new List<IConstraint> { info, constraint };
                return true;
            }
            else if (constraint is UpperBound { Parent: var parentType })
            {
                if (info.FixedType != null)
                {
                    if (!parentType.IsAssignableFrom(info.FixedType))
                        return false;
                    return true;
                }
                // We don't have a fixed type (no equivalent constraint).

                // We use `info` to rule out invalid constraints early on, before we iterate over all constraints.
                if (parentType.IsClass)
                {
                    // Classes can live under classes, only if the class is its ancestor.
                    // Arrays can live under classes, only if the class is an `Array` or an `object`.
                    // Value types can live under classes, only if the class is a `ValueType` or an `object`.
                    // because that's going to be beneficial only if the type is already constrained to be an array, which should be impossible without an equivalent constraint.
                    if (info.NotClass && info.NotArray && info.NotValueType)
                        return false;
                    info.NotInterface = true;
                    if (!(parentType == typeof(Array) || parentType == typeof(object)))
                        info.NotArray = true;
                    if (!(parentType == typeof(ValueType) || parentType == typeof(object)))
                        info.NotValueType = true;
                }
                else if (parentType.IsInterface)
                {
                    // Everything can live under interfaces.
                }
                else if (parentType.IsValueType)
                {
                    // This should be considered an equivalent constraint.
                    if (info.NotValueType)
                        return false;
                    // This recursive call will be cheap (no recursive-recursive calls) because equivalent contraints are easy to solve.
                    return TryAddConstraint(typeParameter, new Equivalent(parentType), parameterConstraints);
                }
                else if (parentType.IsArray)
                {
                    // This should be considered an equivalent constraint.
                    if (info.NotArray)
                        return false;
                    // This recursive call will be cheap (no recursive-recursive calls) because equivalent contraints are easy to solve.
                    return TryAddConstraint(typeParameter, new Equivalent(parentType), parameterConstraints);
                }
                else if (parentType.IsPointer)
                {
                    // Not reachable, as we checked for pointer type constraints above.
                    throw new Exception();
                }
                else
                    throw new Exception();

                // We now know this constraint isn't trivially impossible; time for iteration.
                // Check for colliding (or unifiable) constraints.
                List<IConstraint> newConstraints = new();

                foreach (IConstraint existingConstraint in parameterConstraints[typeParameter])
                {
                    if (existingConstraint is ConstraintInfo)
                    {
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else if (existingConstraint is Equivalent { Type: var otherConstraintType })
                    {
                        // We just checked that we don't have an equivalent constraint. There's no way we have one.
                        throw new Exception();
                    }
                    else if (existingConstraint is UpperBound { Parent: var otherParentType })
                    {
                        var result = ValidateConstraint(constraint, existingConstraint);
                        if (result == ValidateConstraintResult.Invalid)
                            // Colliding constraints.
                            return false;
                        else if (result == ValidateConstraintResult.Overwrite)
                            // The new constraint is stonger than the existing one.
                            // No need to keep the existing one.
                            continue;
                        else if (result == ValidateConstraintResult.Ignore)
                            // The new constraint is weaker than the existing one.
                            return true;
                        else if (result == ValidateConstraintResult.Add)
                        {
                            // We need to keep both constraints.
                            newConstraints.Add(existingConstraint);
                            continue;
                        }
                        else
                            throw new Exception();
                    }
                    else if (existingConstraint is LowerBound { Child: var otherChildType })
                    {
                        var result = ValidateConstraint(constraint, existingConstraint);
                        if (result == ValidateConstraintResult.Invalid)
                            // Colliding constraints.
                            return false;
                        else if (result == ValidateConstraintResult.Overwrite)
                            // The new constraint is stonger than the existing one.
                            // No need to keep the existing one.
                            continue;
                        else if (result == ValidateConstraintResult.Ignore)
                            // The new constraint is weaker than the existing one.
                            return true;
                        else if (result == ValidateConstraintResult.Add)
                        {
                            // We need to keep both constraints.
                            newConstraints.Add(existingConstraint);
                            continue;
                        }
                        else
                            throw new Exception();
                    }
                    else if (existingConstraint is ReferenceType)
                    {
                        // We already checked these constraints with the flags.
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else if (existingConstraint is NotNullableValueType)
                    {
                        // We already checked these constraints with the flags.
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else if (existingConstraint is DefaultConstructor)
                    {
                        // We can check this constraint once we fix the type.
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else
                        throw new Exception();
                }
                newConstraints.Add(constraint);
                parameterConstraints[typeParameter] = newConstraints;
                return true;
            }
            else if (constraint is LowerBound { Child: var childType })
            {
                if (info.FixedType != null)
                {
                    if (!info.FixedType.IsAssignableFrom(childType))
                        return false;
                    return true;
                }
                // We don't have a fixed type (no equivalent constraint).

                // We use `info` to rule out invalid constraints early on, before we iterate over all constraints.
                if (childType.IsClass)
                {
                    // Classes can only inherit classes and other interfaces.
                    if (info.NotClass && info.NotInterface)
                        return false;
                    info.NotArray = true;
                    info.NotValueType = true;
                }
                else if (childType.IsInterface)
                {
                    // Interfaces can only inherit interfaces.
                    if (info.NotInterface)
                        return false;
                    info.NotClass = true;
                    info.NotValueType = true;
                    info.NotArray = true;
                }
                else if (childType.IsValueType)
                {
                    // Value types can inherit interfaces and classes `object` and `ValueType`, and can be equivalent to structs.
                    if (info.NotInterface && info.NotValueType && info.NotClass)
                        return false;
                    info.NotArray = true;
                    // So if the type is not an interface nor a class, we can take this as an equivalent constraint.
                    if (info.NotClass && info.NotInterface)
                        return TryAddConstraint(typeParameter, new Equivalent(childType), parameterConstraints);
                }
                else if (childType.IsArray)
                {
                    // Array types inherit classes and interfaces, and can be equivalent to arrays.
                    if (info.NotClass && info.NotInterface && info.NotArray)
                        return false;
                    info.NotValueType = true;
                    // So if the type is not an interface nor a class, we can take this as an equivalent constraint.
                    if (info.NotClass && info.NotInterface)
                        return TryAddConstraint(typeParameter, new Equivalent(childType), parameterConstraints);
                }
                else if (childType.IsPointer)
                {
                    // Not reachable, as we checked for pointer type constraints above.
                    throw new Exception();
                }
                else
                    throw new Exception();

                // We now know this constraint isn't trivially impossible; time for iteration.
                // Check for colliding (or unifiable) constraints.
                List<IConstraint> newConstraints = new();

                foreach (IConstraint existingConstraint in parameterConstraints[typeParameter])
                {
                    if (existingConstraint is ConstraintInfo)
                    {
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else if (existingConstraint is Equivalent { Type: var otherConstraintType })
                    {
                        // We just checked that we don't have an equivalent constraint. There's no way we have one.
                        throw new Exception();
                    }
                    else if (existingConstraint is UpperBound || existingConstraint is LowerBound)
                    {
                        var result = ValidateConstraint(constraint, existingConstraint);
                        if (result == ValidateConstraintResult.Invalid)
                            // Colliding constraints.
                            return false;
                        else if (result == ValidateConstraintResult.Overwrite)
                            // The new constraint is stonger than the existing one.
                            // No need to keep the existing one.
                            continue;
                        else if (result == ValidateConstraintResult.Ignore)
                            // The new constraint is weaker than the existing one.
                            return true;
                        else if (result == ValidateConstraintResult.Add)
                        {
                            // We need to keep both constraints.
                            newConstraints.Add(existingConstraint);
                            continue;
                        }
                        else
                            throw new Exception();
                    }
                    else if (existingConstraint is ReferenceType)
                    {
                        // We already checked these constraints with the flags.
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else if (existingConstraint is NotNullableValueType)
                    {
                        // We already checked these constraints with the flags.
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else if (existingConstraint is DefaultConstructor)
                    {
                        // We can check this constraint once we fix the type.
                        newConstraints.Add(existingConstraint);
                        continue;
                    }
                    else
                        throw new Exception();
                }
                newConstraints.Add(constraint);
                parameterConstraints[typeParameter] = newConstraints;
                return true;
            }
            else if (constraint is ReferenceType)
            {
                if (info.NotValueType)
                    return true;
                if (info.NotClass && info.NotInterface && info.NotArray)
                    return false;
                info.NotValueType = true;
                parameterConstraints[typeParameter].Add(constraint);
                return true;
            }
            else if (constraint is NotNullableValueType)
            {
                if (info.NotClass && info.NotInterface && info.NotArray)
                    return true;
                if (info.NotValueType)
                    return false;
                info.NotClass = true;
                info.NotInterface = true;
                info.NotArray = true;
                parameterConstraints[typeParameter].Add(constraint);
                return true;
            }
            else if (constraint is DefaultConstructor)
            {
                // Can't check this until we have an equivalent constraint.
                if (info.NeedsDefault)
                    return true;
                info.NeedsDefault = true;
                parameterConstraints[typeParameter].Add(constraint);
                return true;
            }
            else
                throw new Exception();
        }

        enum ValidateConstraintResult
        {
            Invalid, // Two constraints can't coexist.
            Overwrite, // The new constraint is stonger. Overwrite the existing one with new.
            Ignore, // The existing contraint is stronger. Ignore the new constraint.
            Add // We need both contraints.
        }

        static ValidateConstraintResult ValidateConstraint(IConstraint newConstraint, IConstraint existingConstraint)
        {
            // We only care about upperbound and lowerbound constraints.
            if (!(newConstraint is UpperBound || newConstraint is LowerBound) || !(existingConstraint is UpperBound || existingConstraint is LowerBound))
                throw new Exception();
            // TODO: Validate constraints.
            return ValidateConstraintResult.Add;
        }

        public static bool IsValidRootType(Type type)
        {
            return
                (type.IsPublic || type.IsNestedPublic || type.IsNestedPrivate) &&
                !type.IsAbstract &&
                !type.IsValueType &&
                !typeof(UnityEngine.Object).IsAssignableFrom(type) &&
                Attribute.IsDefined(type, typeof(SerializableAttribute)) &&
                !Attribute.IsDefined(type, typeof(HideInTypeMenuAttribute));
        }
    }
}
