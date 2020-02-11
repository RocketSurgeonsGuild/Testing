using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;

namespace Rocket.Surgery.Extensions.Testing
{
    class RemoveProxyFromEnumerableRegistrationSource : IRegistrationSource
    {
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor
        )
        {
            // This is only designed to work with IEnumerable, not array or list.
            if (service is TypedService typedService && typedService.ServiceType.IsGenericType &&
                typedService.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var registrations = registrationAccessor(service);
                foreach (var registration in registrations.TakeLast(1))
                {
                    void onRegistrationOnActivating(object sender, ActivatingEventArgs<object> args)
                    {
                        var method = typeof(RemoveProxyFromEnumerableRegistrationSource).GetMethod(
                                nameof(ReplaceInstance),
                                BindingFlags.Static | BindingFlags.NonPublic
                            )
                           .MakeGenericMethod(typedService.ServiceType.GenericTypeArguments[0]);

                        if (args.Instance is IEnumerable<object> enumerable && enumerable.Any(FakeItEasy.Fake.IsFake))
                        {
                            method.Invoke(null, new object[] { args, enumerable });
                        }

                        registration.Activating -= onRegistrationOnActivating;
                    }

                    registration.Activating += onRegistrationOnActivating;
                }
            }

            yield break;
        }

        public bool IsAdapterForIndividualComponents { get; } = true;

        static void ReplaceInstance<T>(ActivatingEventArgs<object> args, IEnumerable<T> items)
        {
            // autofac does arrays, we're using a list.
            if (items.GetType() == typeof(List<T>))
                return;

            var newItems = new List<T>();
            newItems.AddRange(items);
            if (newItems.Count == 1)
            {
                // The fake can only be the one item here
                newItems.Clear();
            }
            else if (newItems.Count >= 2)
            {
                // The Fake seems to always be index 1
                newItems.RemoveAt(1);
            }

            args.ReplaceInstance(newItems.AsEnumerable());
        }
    }
}