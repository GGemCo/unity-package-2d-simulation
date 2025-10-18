using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo2DSimulation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGemCo2DCore
{
    /// <summary>
    /// 아이템 이미지 로드
    /// </summary>
    public class AddressableLoaderGrowth : MonoBehaviour
    {
        public static AddressableLoaderGrowth Instance { get; private set; }
        private readonly Dictionary<string, GrowthBase> _dictionary = new Dictionary<string, GrowthBase>();
        private readonly HashSet<AsyncOperationHandle> _activeHandles = new HashSet<AsyncOperationHandle>();
        private float _prefabLoadProgress;

        private void Awake()
        {
            _prefabLoadProgress = 0f;
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            ReleaseAll();
        }

        /// <summary>
        /// 모든 로드된 리소스를 해제합니다.
        /// </summary>
        private void ReleaseAll()
        {
            AddressableLoaderController.ReleaseByHandles(_activeHandles);
        }
        public async Task LoadPrefabsAsync()
        {
            try
            {
                // 아이콘 이미지
                _dictionary.Clear();
                var locationHandle = Addressables.LoadResourceLocationsAsync(ConfigAddressableLabel.SimulationGrowth);
                await locationHandle.Task;

                if (!locationHandle.IsValid() || locationHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    GcLogger.LogError($"{ConfigAddressableLabel.SimulationGrowth} 레이블을 가진 리소스를 찾을 수 없습니다.");
                    return;
                }

                int totalCount = locationHandle.Result.Count;
                int loadedCount = 0;

                foreach (var location in locationHandle.Result)
                {
                    string address = location.PrimaryKey;
                    var loadHandle = Addressables.LoadAssetAsync<GrowthBase>(address);

                    while (!loadHandle.IsDone)
                    {
                        _prefabLoadProgress = (loadedCount + loadHandle.PercentComplete) / totalCount;
                        await Task.Yield();
                    }
                    _activeHandles.Add(loadHandle);

                    GrowthBase prefab = await loadHandle.Task;
                    if (!prefab) continue;
                    _dictionary[address] = prefab;
                    loadedCount++;
                }
                _activeHandles.Add(locationHandle);

                _prefabLoadProgress = 1f; // 100%
                // GcLogger.Log($"총 {loadedCount}/{totalCount}개의 프리팹을 성공적으로 로드했습니다.");
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"프리팹 로딩 중 오류 발생: {ex.Message}");
            }
        }

        public GrowthBase GetGrowthBaseByName(string key)
        {
            if (_dictionary.TryGetValue(key, out var growthBase))
            {
                return growthBase;
            }

            GcLogger.LogError($"Addressables에서 {key} 스크립터블 오브젝트을 찾을 수 없습니다.");
            return null;
        }
        public float GetPrefabLoadProgress() => _prefabLoadProgress;

    }
}
