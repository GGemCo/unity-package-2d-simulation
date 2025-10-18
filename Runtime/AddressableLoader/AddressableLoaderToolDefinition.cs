using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GGemCo2DSimulation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GGemCo2DCore
{
    /// <summary>
    /// Addressables에서 ToolDefinition을 로드 후
    /// - 주소(PrimaryKey) 기준 캐싱
    /// - 등록된 '라벨 키' 기준 캐싱 (런타임 역인덱스)
    /// </summary>
    public class AddressableLoaderToolDefinition : MonoBehaviour
    {
        public static AddressableLoaderToolDefinition Instance { get; private set; }

        private readonly Dictionary<string, ToolDefinition> _dictionary = new();
        private readonly Dictionary<string, ToolDefinition> _dictionaryLabels = new(); // 라벨 -> 단일 ToolDefinition
        // 필요 시 Dictionary<string, List<ToolDefinition>> 로 확장 가능

        private readonly HashSet<AsyncOperationHandle> _activeHandles = new();
        private float _prefabLoadProgress;
        
        // 라벨 필터 정규식: GGemCo_SimulationToolDefinition_숫자
        // ConfigAddressableLabel.SimulationToolDefinition 값 사용
        private static Regex CreateLabelRegex()
        {
            // 예: ConfigAddressableLabel.SimulationToolDefinition = "GGemCo_SimulationToolDefinition"
            string prefix = ConfigAddressableLabel.SimulationToolDefinition;
            string pattern = $"^{Regex.Escape(prefix)}_\\d+$";
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        private Regex _labelRegex;
        
        private void Awake()
        {
            _prefabLoadProgress = 0f;
            _labelRegex = CreateLabelRegex();
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

        private void OnDestroy() => ReleaseAll();

        private void ReleaseAll()
        {
            AddressableLoaderController.ReleaseByHandles(_activeHandles);
        }

        public async Task LoadPrefabsAsync()
        {
            try
            {
                _dictionary.Clear();
                _dictionaryLabels.Clear();

                // 1) ToolDefinition 후보 로케이션 조회
                var locationHandle = Addressables.LoadResourceLocationsAsync(
                    ConfigAddressableLabel.SimulationToolDefinition, typeof(ToolDefinition));
                await locationHandle.Task;

                if (!locationHandle.IsValid() || locationHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    GcLogger.LogError($"{ConfigAddressableLabel.SimulationToolDefinition} 레이블을 가진 리소스를 찾을 수 없습니다.");
                    return;
                }

                // 주소 매칭을 위한 빠른 조회 구조
                var addressToLocation = new Dictionary<string, IResourceLocation>();
                foreach (var loc in locationHandle.Result)
                {
                    // PrimaryKey는 통상 Address입니다.
                    addressToLocation[loc.PrimaryKey] = loc;
                }

                // 2) 실제 에셋 로드 + 주소 기반 캐싱
                int totalCount = locationHandle.Result.Count;
                int loadedCount = 0;

                foreach (IResourceLocation location in locationHandle.Result)
                {
                    string address = location.PrimaryKey;
                    var loadHandle = Addressables.LoadAssetAsync<ToolDefinition>(address);

                    while (!loadHandle.IsDone)
                    {
                        _prefabLoadProgress = (loadedCount + loadHandle.PercentComplete) / totalCount;
                        await Task.Yield();
                    }

                    _activeHandles.Add(loadHandle);
                    ToolDefinition asset = await loadHandle.Task;
                    if (!asset) continue;

                    _dictionary[address] = asset;
                    loadedCount++;
                }

                _activeHandles.Add(locationHandle);
                _prefabLoadProgress = Mathf.Approximately(totalCount, 0) ? 1f : (float)loadedCount / totalCount;

                // 3) 모든 리소스 로케이터의 '키(=주소/라벨)'를 순회하여 라벨 역인덱스 구성
                //    - 라벨 키로 Locate 시, ToolDefinition 로케이션 목록을 얻을 수 있음
                //    - 그 목록의 PrimaryKey가 우리가 로드한 address와 일치하면, 해당 라벨을 그 ToolDefinition에 매핑
                var labelCandidates = new HashSet<string>();
                foreach (var locator in Addressables.ResourceLocators)
                {
                    if (locator == null) continue;
                    foreach (var keyObj in locator.Keys)
                    {
                        // 키는 object이지만, 보통 string
                        string keyStr = keyObj?.ToString();
                        if (string.IsNullOrEmpty(keyStr)) continue;
                        
                        // 정규식 필터: GGemCo_SimulationToolDefinition_숫자
                        if (!_labelRegex.IsMatch(keyStr)) continue;

                        // 중복 키 방지
                        if (!labelCandidates.Add(keyStr)) continue;

                        // 이 키가 ToolDefinition들에 매칭되는 '라벨 키'인지 확인
                        if (locator.Locate(keyObj, typeof(ToolDefinition), out IList<IResourceLocation> locs) && locs != null)
                        {
                            foreach (var loc in locs)
                            {
                                // 우리가 방금 로드한 대상(addressToLocation) 안에 있는지 확인
                                if (!string.IsNullOrEmpty(loc.PrimaryKey) && _dictionary.TryGetValue(loc.PrimaryKey, out var toolDef))
                                {
                                    // 단일 매핑 사양: 중복 라벨이면 경고만
                                    if (!_dictionaryLabels.ContainsKey(keyStr))
                                    {
                                        _dictionaryLabels.Add(keyStr, toolDef);
                                    }
                                    else if (!ReferenceEquals(_dictionaryLabels[keyStr], toolDef))
                                    {
                                        GcLogger.LogWarning($"라벨 '{keyStr}'이(가) 여러 ToolDefinition에 매칭됩니다. 기존: {_dictionaryLabels[keyStr].name}, 새로 발견: {toolDef.name}");
                                    }
                                }
                            }
                        }
                    }
                }

                _prefabLoadProgress = 1f;
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"프리팹 로딩 중 오류 발생: {ex.Message}");
            }
        }

        public ToolDefinition GetToolDefinitionByName(string key)
        {
            if (_dictionary.TryGetValue(key, out var toolDefinition))
                return toolDefinition;

            GcLogger.LogError($"Addressables에서 {key} 스크립터블 오브젝트를 찾을 수 없습니다.");
            return null;
        }

        public ToolDefinition GetToolDefinitionByLabel(string label)
        {
            if (_dictionaryLabels.TryGetValue(label, out var toolDefinition))
                return toolDefinition;

            GcLogger.LogError($"Addressables에서 라벨 '{label}' 을 가진 ToolDefinition을 찾을 수 없습니다.");
            return null;
        }

        public float GetPrefabLoadProgress() => _prefabLoadProgress;
    }
}
