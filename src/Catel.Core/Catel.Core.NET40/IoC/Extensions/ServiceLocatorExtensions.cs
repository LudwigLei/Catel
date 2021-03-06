﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceLocatorExtensions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;

    /// <summary>
    /// Extension methods for the <see cref="IServiceLocator"/> interface.
    /// </summary>
    public static class ServiceLocatorExtensions
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Resolves the type using parameters. This method combines the <see cref="IServiceLocator.GetRegistrationInfo" /> and
        /// the <see cref="ITypeFactory.CreateInstanceWithParameters" /> to provide the functionality.
        /// </summary>
        /// <typeparam name="T">The type of the interface to resolve.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The instantiated type constructed with the specified parameters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="parameters" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The type is not registered in the container as transient type.</exception>
        public static T ResolveTypeUsingParameters<T>(this IServiceLocator serviceLocator, object[] parameters, object tag = null)
        {
            return (T)ResolveTypeUsingParameter(serviceLocator, typeof(T), parameters, tag);
        }

        /// <summary>
        /// Resolves the type using parameters. This method combines the <see cref="IServiceLocator.GetRegistrationInfo" /> and
        /// the <see cref="ITypeFactory.CreateInstanceWithParameters" /> to provide the functionality.
        /// </summary>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The instantiated type constructed with the specified parameters.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceType" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="parameters" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The type is not registered in the container as transient type.</exception>
        public static object ResolveTypeUsingParameter(this IServiceLocator serviceLocator, Type serviceType, object[] parameters, object tag = null)
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);
            Argument.IsNotNull("serviceType", serviceType);
            Argument.IsNotNull("parameters", parameters);

            var registrationInfo = serviceLocator.GetRegistrationInfo(serviceType, tag);
            if (registrationInfo == null)
            {
                string error = string.Format("The service locator could not return the registration info for type '{0}' with tag '{1}', cannot resolve type",
                    serviceType.FullName, ObjectToStringHelper.ToString(tag));
                Log.Error(error);
                throw new InvalidOperationException(error);
            }

            var typeFactory = serviceLocator.ResolveType<ITypeFactory>();

            if (registrationInfo.RegistrationType == RegistrationType.Singleton)
            {
                if (registrationInfo.IsTypeInstantiatedForSingleton)
                {
                    return serviceLocator.ResolveType(serviceType);
                }

                Log.Debug("Type '{0}' is registered as singleton but has not yet been instantiated. Instantiated it with the specified parameters now and registering it in the ServiceLocator", serviceType.FullName);

                var instance = typeFactory.CreateInstanceWithParameters(registrationInfo.ImplementingType, parameters);
                serviceLocator.RegisterInstance(serviceType, instance);
                return instance;
            }

            return typeFactory.CreateInstanceWithParameters(registrationInfo.ImplementingType, parameters);
        }

        /// <summary>
        /// Determines whether the specified service type is registered.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <returns><c>true</c> if the specified service type is registered; otherwise, <c>false</c>.</returns>
        /// <remarks>Note that the actual implementation lays in the hands of the IoC technique being used.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        public static bool IsTypeRegistered<TService>(this IServiceLocator serviceLocator)
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);

            return serviceLocator.IsTypeRegistered(typeof(TService));
        }

        /// <summary>
        /// Determines whether the specified service type is registered as singleton.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <returns><c>true</c> if the <typeparamref name="TService" /> type is registered as singleton, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        public static bool IsTypeRegisteredAsSingleton<TService>(this IServiceLocator serviceLocator)
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);

            return serviceLocator.IsTypeRegisteredAsSingleton(typeof(TService));
        }

        /// <summary>
        /// Registers a specific instance of a service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="instance">The specific instance to register.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="instance" /> is <c>null</c>.</exception>
        public static void RegisterInstance<TService>(this IServiceLocator serviceLocator, TService instance)
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);
            Argument.IsNotNull("instance", instance);

            serviceLocator.RegisterInstance(typeof(TService), instance);
        }

        /// <summary>
        /// Registers an implementation of an service, but only if the type is not yet registered.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TServiceImplementation">The type of the implementation.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="registrationType">The registration type. The default value is <see cref="RegistrationType.Singleton" />.</param>
        /// <remarks>Note that the actual implementation lays in the hands of the IoC technique being used.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        public static void RegisterTypeIfNotYetRegistered<TService, TServiceImplementation>(this IServiceLocator serviceLocator, RegistrationType registrationType = RegistrationType.Singleton)
            where TServiceImplementation : TService
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);

            serviceLocator.RegisterType<TService, TServiceImplementation>(registrationType, false);
        }

        /// <summary>
        /// Registers an implementation of an service, but only if the type is not yet registered.
        /// </summary>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="serviceImplementationType">The type of the implementation.</param>
        /// <param name="tag">The tag to register the service with. The default value is <c>null</c>.</param>
        /// <param name="registrationType">The registration type. The default value is <see cref="RegistrationType.Singleton"/>.</param>
        /// <remarks>
        /// Note that the actual implementation lays in the hands of the IoC technique being used.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceImplementationType"/> is <c>null</c>.</exception>
        public static void RegisterTypeIfNotYetRegistered(this IServiceLocator serviceLocator, Type serviceType, Type serviceImplementationType, object tag = null, RegistrationType registrationType = RegistrationType.Singleton)
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);

            serviceLocator.RegisterType(serviceType, serviceImplementationType, tag, registrationType, false);
        }

        /// <summary>
        /// Registers a service where the implementation type is the same as the registered type.
        /// </summary>
        /// <typeparam name="TServiceImplementation">The type of the service definition and implementation.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="tag">The tag to register the service with. The default value is <c>null</c>.</param>
        /// <param name="registrationType">The registration type. The default value is <see cref="RegistrationType.Singleton" />.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        /// <remarks>Note that the actual implementation lays in the hands of the IoC technique being used.</remarks>
        public static void RegisterType<TServiceImplementation>(this IServiceLocator serviceLocator, object tag = null, RegistrationType registrationType = RegistrationType.Singleton)
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);

            serviceLocator.RegisterType(typeof(TServiceImplementation), typeof(TServiceImplementation), tag, registrationType);
        }

        /// <summary>
        /// Registers an implementation of an service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TServiceImplementation">The type of the implementation.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="registrationType">The registration type. The default value is <see cref="RegistrationType.Singleton"/>.</param>
        /// <param name="registerIfAlreadyRegistered">If set to <c>true</c>, an older type registration is overwritten by this new one.</param>
        /// <remarks>
        /// Note that the actual implementation lays in the hands of the IoC technique being used.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        public static void RegisterType<TService, TServiceImplementation>(this IServiceLocator serviceLocator, RegistrationType registrationType = RegistrationType.Singleton, bool registerIfAlreadyRegistered = true)
            where TServiceImplementation : TService
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);

            serviceLocator.RegisterType(typeof(TService), typeof(TServiceImplementation), null, registrationType, registerIfAlreadyRegistered);
        }

        /// <summary>
        /// Resolves an instance of the type registered on the service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <returns>An instance of the type registered on the service.</returns>
        /// <remarks>
        /// Note that the actual implementation lays in the hands of the IoC technique being used.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        public static TService ResolveType<TService>(this IServiceLocator serviceLocator)
        {
            Argument.IsNotNull("serviceLocator", serviceLocator);

            return (TService)serviceLocator.ResolveType(typeof(TService));
        }

        /// <summary>
        /// Resolves all instances of the type registered on the service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="serviceLocator">The service locator.</param>
        /// <returns>All instance of the type registered on the service.</returns>
        /// <remarks>Note that the actual implementation lays in the hands of the IoC technique being used.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceLocator" /> is <c>null</c>.</exception>
        public static IEnumerable<TService> ResolveTypes<TService>(this IServiceLocator serviceLocator)
        {
            return serviceLocator.ResolveTypes(typeof(TService)).Cast<TService>();
        }
    }
}