﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavableModelBaseBase.cs" company="Catel development team">
//   Copyright (c) 2008 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Data
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    using Logging;

#if NET
    using Catel.Runtime.Serialization;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
#elif NETFX_CORE
    using Windows.Storage.Streams;
    using Runtime.Serialization;
#else
    using System.IO.IsolatedStorage;
    using Runtime.Serialization;
#endif

    /// <summary>
    /// Abstract class that makes the <see cref="ModelBase{TModelBase}"/> serializable.
    /// </summary>
    /// <typeparam name="T">Type that the class should hold (same as the defined type).</typeparam>
#if NET
    [Serializable]
#endif
    public abstract class SavableModelBase<T> : ModelBase, ISavableModel
        where T : class
    {
        #region Fields
        /// <summary>
        /// The log.
        /// </summary>
#if NET
        [field: NonSerialized]
#endif
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SavableModelBase{T}"/> class.
        /// </summary>
        protected SavableModelBase()
        {
            if (ContainsNonSerializableMembers)
            {
                throw new NotSupportedException("Properties that are not serializable are not supported");
            }
        }

#if NET
        /// <summary>
        /// Initializes a new instance of the <see cref="SavableModelBase{T}"/> class.
        /// </summary>
        /// <param name="info">SerializationInfo object, null if this is the first time construction.</param>
        /// <param name="context">StreamingContext object, simple pass a default new StreamingContext() if this is the first time construction.</param>
        /// <remarks>
        /// Call this method, even when constructing the object for the first time (thus not deserializing).
        /// </remarks>
        protected SavableModelBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        #endregion

        #region Properties
        /// <summary>
        /// Gets the bytes of the current binary serialized data object.
        /// </summary>
        /// <value>The bytes.</value>
#if !WINDOWS_PHONE && !NETFX_CORE
        [Browsable(false)]
#endif
        [XmlIgnore]
        public byte[] Bytes { get { return ToByteArray(); } }
        #endregion

        #region Methods
        /// <summary>
        /// Serializes the object to and xml object.
        /// </summary>
        /// <returns><see cref="XDocument"/> containing the serialized data.</returns>
        public XDocument ToXml()
        {
            using (var memoryStream = new MemoryStream())
            {
                Save(memoryStream, SerializationMode.Xml);

                memoryStream.Position = 0L;

                using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                {
                    return XDocument.Load(xmlReader);
                }
            }
        }

        /// <summary>
        /// Serializes the object to a byte array.
        /// </summary>
        /// <returns>Byte array containing the serialized data.</returns>
        public byte[] ToByteArray()
        {
            using (var memoryStream = new MemoryStream())
            {
#if NET
                Save(memoryStream, SerializationMode.Binary);
#else
                Save(memoryStream, SerializationMode.Xml);
#endif

                return memoryStream.ToArray();
            }
        }

        #region Saving
#if NET
        /// <summary>
        /// Saves the object to a file using the default formatting.
        /// </summary>
        /// <param name="fileName">Filename of the file that will contain the serialized data of this object.</param>
        public void Save(string fileName)
        {
            Save(fileName, Mode);
        }

        /// <summary>
        /// Saves the object to a file using a specific formatting.
        /// </summary>
        /// <param name="fileName">Filename of the file that will contain the serialized data of this object.</param>
        /// <param name="mode"><see cref="SerializationMode"/> to use.</param>
        public void Save(string fileName, SerializationMode mode)
        {
            var fileInfo = new FileInfo(fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                Save(stream, mode);
            }
        }
#elif NETFX_CORE
        /// <summary>
        /// Saves the object to an isolated storage file stream using the default formatting.
        /// </summary>
        /// <param name="fileStream">Stream that will contain the serialized data of this object.</param>
        public void Save(IRandomAccessStream fileStream)
        {
            Save((Stream)fileStream);
        }
#else
        /// <summary>
        /// Saves the object to an isolated storage file stream using the default formatting.
        /// </summary>
        /// <param name="fileStream">Stream that will contain the serialized data of this object.</param>
        public void Save(IsolatedStorageFileStream fileStream)
        {
            Save((Stream)fileStream);
        }
#endif

        /// <summary>
        /// Saves the object to a stream using the default formatting.
        /// </summary>
        /// <param name="stream">Stream that will contain the serialized data of this object.</param>
        public void Save(Stream stream)
        {
            Save(stream, Mode);
        }

        /// <summary>
        /// Saves the object to a stream using a specific formatting.
        /// </summary>
        /// <param name="stream">Stream that will contain the serialized data of this object.</param>
        /// <param name="mode"><see cref="SerializationMode"/> to use.</param>
        public void Save(Stream stream, SerializationMode mode)
        {
            Log.Debug("Saving object '{0}' as '{1}'", GetType().Name, mode);

            switch (mode)
            {
#if NET
                case SerializationMode.Binary:
                    BinaryFormatter binaryFormatter = SerializationHelper.GetBinarySerializer(false);
                    binaryFormatter.Serialize(stream, this);
                    break;
#endif

                case SerializationMode.Xml:
                    var settings = new XmlWriterSettings();
                    settings.OmitXmlDeclaration = false;
                    settings.Indent = true;

                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
                    {
                        xmlWriter.WriteStartElement(GetType().Name);
                        ((IXmlSerializable)this).WriteXml(xmlWriter);
                        xmlWriter.WriteEndElement();
                    }

                    break;
            }

            Log.Debug("Saved object");

            ClearIsDirtyOnAllChilds();
        }
        #endregion

        #region Loading
#if NET
        /// <summary>
        /// Loads the object from a file using binary formatting.
        /// </summary>
        /// <param name="fileName">Filename of the file that contains the serialized data of this object.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(string fileName, bool enableRedirects = false)
        {
            return Load<T>(fileName, enableRedirects);
        }
#elif NETFX_CORE
        /// <summary>
        /// Loads the object from a file using binary formatting.
        /// </summary>
        /// <param name="fileStream">File stream of the file that contains the serialized data of this object.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(IRandomAccessStream fileStream, bool enableRedirects = false)
        {
            return Load<T>(fileStream, enableRedirects);
        }
#else
        /// <summary>
        /// Loads the object from a file using binary formatting.
        /// </summary>
        /// <param name="fileStream">File stream of the file that contains the serialized data of this object.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(IsolatedStorageFileStream fileStream, bool enableRedirects = false)
        {
            return Load<T>(fileStream, enableRedirects);
        }
#endif

#if NET
        /// <summary>
        /// Loads the object from a file using a specific formatting.
        /// </summary>
        /// <param name="fileName">Filename of the file that contains the serialized data of this object.</param>
        /// <param name="mode"><see cref="SerializationMode"/> to use.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(string fileName, SerializationMode mode, bool enableRedirects = false)
        {
            return Load<T>(fileName, mode, enableRedirects);
        }
#elif NETFX_CORE
        /// <summary>
        /// Loads the object from a file using a specific formatting.
        /// </summary>
        /// <param name="fileStream">File stream of the file that contains the serialized data of this object.</param>
        /// <param name="mode"><see cref="SerializationMode"/> to use.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(IRandomAccessStream fileStream, SerializationMode mode, bool enableRedirects = false)
        {
            return Load<T>(fileStream, mode, enableRedirects);
        }
#else
        /// <summary>
        /// Loads the object from a file using a specific formatting.
        /// </summary>
        /// <param name="fileStream">File stream of the file that contains the serialized data of this object.</param>
        /// <param name="mode"><see cref="SerializationMode"/> to use.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(IsolatedStorageFileStream fileStream, SerializationMode mode, bool enableRedirects = false)
        {
            return Load<T>(fileStream, mode, enableRedirects);
        }
#endif

        /// <summary>
        /// Loads the object from an XmlDocument object.
        /// </summary>
        /// <param name="xmlDocument">The XML document.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        public static T Load(XDocument xmlDocument)
        {
            return Load<T>(xmlDocument);
        }

        /// <summary>
        /// Loads the object from a stream.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>
        /// Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.
        /// </returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(byte[] bytes, bool enableRedirects = false)
        {
            return Load<T>(bytes, enableRedirects);
        }

        /// <summary>
        /// Loads the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(Stream stream, bool enableRedirects = false)
        {
            return Load<T>(stream, enableRedirects);
        }

        /// <summary>
        /// Loads the object from a stream using a specific formatting.
        /// </summary>
        /// <param name="stream">Stream that contains the serialized data of this object.</param>
        /// <param name="mode"><see cref="SerializationMode"/> to use.</param> 
        /// <param name="enableRedirects">if set to <c>true</c>, redirects will be enabled.</param>
        /// <returns>Deserialized instance of the object. If the deserialization fails, <c>null</c> is returned.</returns>
        /// <remarks>
        /// When enableRedirects is enabled, loading will take more time. Only set
        /// the parameter to <c>true</c> when the deserialization without redirects fails.
        /// </remarks>
        public static T Load(Stream stream, SerializationMode mode, bool enableRedirects = false)
        {
            return Load<T>(stream, mode, enableRedirects);
        }
        #endregion
        #endregion
    }
}