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
    /// </summary>
    public abstract class EntityMem
    {
        [JsonIgnore]    // field used for management and not serialized out
        public DateTime lastAccessed;

        public EntityMem()
        {
        }
        // Touch the Entity so it is recent
        public void Touch()
        {
            lastAccessed = DateTime.UtcNow;
        }
        // The name of this type of entity (used for storage directory)
        public abstract string EntityType();
        // The name to use to index and store this type of entity
        public abstract string StorageName();
    }

    public class EntityStorage
    {
        private static readonly string _logHeader = "[EntityStorage]";

        // Used to lock across storage checking and updates.
        protected object storageLock = new object();

        protected string entityStorageDir;
        protected int minutesToKeepIdleEntities;

        public EntityStorage()
        {
            entityStorageDir = Context.Params.P<string>("Storage.Dir");
            minutesToKeepIdleEntities = Context.Params.P<int>("Storage.IdleMinutes");
        }

        // Return true if the named entity exists in the storage system
        public bool ExistsInStorage(string pEntityType, string pStorageName)
        {
            return false;
        }

        public T FetchFromStorage<T>(string pEntityType, string pStorageName)
        {
            T entity = JsonConvert.DeserializeObject<T>(File.ReadAllText(EntityFilename(pEntityType, pStorageName)));
            return entity;
        }

        public void StoreInStorage(EntityMem pEntity)
        {
            lock (storageLock)
            {
                if (!Directory.Exists(pEntity.EntityType()))
                {
                    Directory.CreateDirectory(pEntity.EntityType());
                }
                File.WriteAllText(EntityFilename(pEntity), JsonConvert.SerializeObject(pEntity, Formatting.Indented));
            }
        }

        protected string EntityFilename(string pEntityType, string pStorageName)
        {
            return entityStorageDir
                        + Path.DirectorySeparatorChar
                        + pEntityType
                        + Path.DirectorySeparatorChar
                        + pStorageName
                        + ".json";
        }
        protected string EntityFilename(EntityMem pEntity)
        {
            return entityStorageDir
                        + Path.DirectorySeparatorChar
                        + pEntity.EntityType()
                        + Path.DirectorySeparatorChar
                        + pEntity.StorageName()
                        + ".json";
        }

    }
}
