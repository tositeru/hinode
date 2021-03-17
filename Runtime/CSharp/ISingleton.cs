// Copyright 2019~ tositeru
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ISingleton<T>
        where T : ISingleton<T>, new()
    {
        static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                ResetInstance();
                return _instance;
            }
        }

        protected ISingleton() {}

        protected static void ResetInstance()
        {
            _instance = new T();
            _instance.OnCreated();
        }

        virtual protected void OnCreated() { }
    }
}
