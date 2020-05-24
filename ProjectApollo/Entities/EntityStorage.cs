//   Copyright 2020 Vircadia
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace Project_Apollo.Entities
{
    /// <summary>
    /// Base class for all entities stored in memory.
    /// This provides a common wrapper for storing and fetching the entities.
    /// </summary>
    public abstract class EntityMem
    {
        [JsonIgnore]    // field used for management and not serialized out
        public DateTime LastAccessed;
        [JsonIgnore]
        private EntityStorage _storageSystem;

        public EntityMem(EntityStorage pStorageSystem)
        {
            _storageSystem = pStorageSystem;
        }
        // Touch the Entity so it is recent
        public void Updated()
        {
            LastAccessed = DateTime.UtcNow;
            Save();
        }
        public void Save()
        {
            _storageSystem.StoreInStorage(this);
        }
        // The name of this type of entity (used for storage directory)
        public abstract string EntityType();
        // The name to use to index and store this type of entity
        public abstract string StorageName();
    }

    /// <summary>
    /// Store entities in a directory.
    /// Entities are stored as JSON files in the directory "entityType/entityStorageName".
    /// Entities are kept in memory for quick reference and read or
    ///     written out as needed.
    /// Entities are flushed from in memory after "Storage.IdleMinutes" minutes.
    /// </summary>
    public abstract class EntityStorage
    {
        private static readonly string _logHeader = "[EntityStorage]";

        // Used to lock across storage checking and updates.
        protected static object _storageLock = new object();

        protected string _storageEntityTypeName;

        protected string _entityStorageDir;

        public EntityStorage(string pEntityTypeName)
        {
            _storageEntityTypeName = pEntityTypeName;


            // The GetFullPath() will correct the directory separators in the config path
            string fullDirPath = Path.GetFullPath(Context.Params.P<string>("Storage.Dir"));
            _entityStorageDir = Path.Combine( fullDirPath, _storageEntityTypeName);

            Context.Log.Debug("{0} Storing {1} entities into {2}", _logHeader, _storageEntityTypeName, _entityStorageDir);
            lock (_storageLock)
            {
                if (!Directory.Exists(_entityStorageDir))
                {
                    try
                    {
                        Directory.CreateDirectory(_entityStorageDir);
                    }
                    catch (Exception e)
                    {
                        Context.Log.Error("{0} Failure creating storage directory {1}: {2}",
                                        _logHeader, _entityStorageDir, e);
                    }
                }
            }
        }

        // Return true if the named entity exists in the storage system
        public bool ExistsInStorage(string pStorageName)
        {
            return File.Exists(EntityFilename(pStorageName));
        }

        public T FetchFromStorage<T>(string pStorageName)
        {
            T entity =  JsonConvert.DeserializeObject<T>(File.ReadAllText(EntityFilename(pStorageName)));
            return entity;
        }

        public void StoreInStorage(EntityMem pEntity)
        {
            File.WriteAllText(EntityFilename(pEntity), JsonConvert.SerializeObject(pEntity, Formatting.Indented));
        }

        protected string EntityFilename(string pStorageName)
        {
            return _entityStorageDir
                        + Path.DirectorySeparatorChar
                        + pStorageName
                        + ".json";
        }
        protected string EntityFilename(EntityMem pEntity)
        {
            return _entityStorageDir
                        + Path.DirectorySeparatorChar
                        + pEntity.StorageName()
                        + ".json";
        }

        /// <summary>
        /// Read in all entities of a particular type.
        /// This is used when starting up to file the list of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected IEnumerable<T> AllEntities<T>()
        {
            foreach (string dirFile in Directory.EnumerateFiles(_entityStorageDir))
            {
                string storageID = Path.GetFileNameWithoutExtension(dirFile);
                T anEntity = default;
                bool fetchSuccess = true;
                try
                {
                    anEntity = FetchFromStorage<T>(storageID);
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} Exception reading entity {1}: {2}",
                                _logHeader, dirFile, e.ToString());
                    fetchSuccess = false;
                }
                if (fetchSuccess)
                {
                    yield return anEntity;
                }
            }
        }

    }
}
