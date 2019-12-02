using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// ModelをGameObjectとして配置する時に使用してください
    /// </summary>
    public abstract class IModelHome : MonoBehaviour
    {
        public Model RootModel
        {
            get => BinderInstanceMap?.RootModel ?? null;
            set 
            {
                Assert.IsNotNull(BinderInstanceMap);
                BinderInstanceMap.RootModel = value;
            }
        }
        public abstract ModelViewBinderMap BinderMap { get; }
        public abstract ModelViewBinderInstanceMap BinderInstanceMap { get; }

        public bool DoJoin(Model target)
        {
            return BinderInstanceMap.BindInstances.ContainsKey(target);
        }

        public static IModelHome[] GetAllHomes()
        {
            return FindObjectsOfType<IModelHome>();
        }

        /// <summary>
        /// 指定したtargetが所属しているIModelHomeを検索する
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IEnumerable<IModelHome> GetJoinHomes(Model target)
        {
            return FindObjectsOfType<IModelHome>().Where(_m => _m.DoJoin(target));
        }

        /// <summary>
        /// 指定したmodelに最も近い親をRootModelに持つIModelHomeを検索する
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IModelHome GetNearestHome(Model target)
        {
            var joinHomes = GetJoinHomes(target);
            return target.GetTraversedRootEnumerable()
                .Select(_m => joinHomes.FirstOrDefault(_b => _b.RootModel == _m))
                .FirstOrDefault(_h => _h != null);
        }
    }
}
