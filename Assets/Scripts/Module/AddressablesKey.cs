using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesKey
{
    enum eLoadStatus
    {
        None = 0,
        Load,
        Ready,
    }

    class AssetEntity
    {
        /// <summary>アドレス、ラベル</summary>
        public string               Key;
        /// <summary>参照カウンタ</summary>
        public int                  RefCount;
        /// <summary>ロード状況</summary>
        public eLoadStatus          LoadStatus = eLoadStatus.None;
        /// <summary>ロード中にキャンセル指示が来たら true</summary>
        public bool                 LoadCancel;
        /// <summary>Addressables のロードハンドラ</summary>
        public AsyncOperationHandle Handle;
    }

    static Dictionary<string, AssetEntity> entities = new Dictionary<string, AssetEntity>();
    
    /// <summary>
    /// Addressables.LoadAssetAsync() の代わり
    /// </summary>
    /// <typeparam name="T">読み込むデータのデータ型</typeparam>
    /// <param name="key">アドレス、ラベル</param>
    /// <param name="action">引数は ASyncOperationHandle ではなく T型 のインスタンス</param>
    public static void LoadAssetAsync<T>(string key, System.Action<T> action = null)
    {
        loadAssetAsync(key, action, null);
    }

    /// <summary>
    /// Addressables.LoadAssetsAsync() の代わり
    /// </summary>
    /// <typeparam name="T">読み込むデータのデータ型</typeparam>
    /// <param name="key">アドレス、ラベル</param>
    /// <param name="actions">引数は ASyncOperationHandle ではなく T型 のインスタンス配列</param>
    public static void LoadAssetsAsync<T>(string key, System.Action<IList<T>> actions = null)
    {
        loadAssetAsync(key, null, actions);
    }

    /// <summary>
    /// 現在のロードファイル数を取得します
    /// </summary>
    public static int GetLoadCount()
    {
        int loadCount = 0;
        
        foreach (var pair in entities)
        {
            if (pair.Value.LoadStatus == eLoadStatus.Load)
            {
                loadCount++;
            }
        }

        return loadCount;
    }
    
    /// <summary>
    /// 指定したアドレス、ラベルの読み込み完了を確認します
    /// </summary>
    public static bool CheckLoadDone<T>(string key)
    {
        string typekey = $"{key} {typeof(T)}";

        if (entities.ContainsKey(typekey) == false)
        {
            return false;
        }
        if (entities[typekey].LoadStatus != eLoadStatus.Ready)
        {
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// アセットアンロード
    /// </summary>
    /// <typeparam name="T">読み込んだデータのデータ型</typeparam>
    /// <param name="key">アドレス、ラベル</param>
    public static void Release<T>(string key)
    {
        string typekey = $"{key} {typeof(T)}";

        if (entities.ContainsKey(typekey) == false)
        {
            return;
        }

        AssetEntity entity = entities[typekey];
        if (--entity.RefCount > 0)
        {
            return;
        }

        if (entity.LoadStatus == eLoadStatus.Ready)
        {
            unload(typekey);
        }
        else
        if (entity.LoadStatus == eLoadStatus.Load)
        {
            // ロード中は、ロード完了を待ってからアンロードする
            entity.LoadCancel = true;
            CoroutineAccessor.Start(loadCancel(typekey));
        }
    }
    
    /// <summary>
    /// ロード本体
    /// </summary>
    static void loadAssetAsync<T>(string key, System.Action<T> action = null, System.Action<IList<T>> actions = null)
    {
        string typekey = $"{key} {typeof(T)}";

        if (entities.ContainsKey(typekey) == true)
        {
            AssetEntity entity = entities[typekey];
            entity.RefCount++;

            if (entity.LoadStatus == eLoadStatus.Ready)
            {
                // 既に読み込まれているならキャッシュで complete
                loadCompleted(entity, action, actions);
            }
            else
            if (entity.LoadStatus == eLoadStatus.Load)
            {
                // 既に読み込み中なら読み込み完了イベントで complete
                entity.Handle.Completed += 
                    (op) =>
                    {
                        loadCompleted(entity, action, actions);
                    };
            }
        }
        else
        {
            AssetEntity entity = new AssetEntity();
            entity.RefCount++;

            entity.LoadStatus = eLoadStatus.Load;
            if (action != null)
            {
                entity.Handle = Addressables.LoadAssetAsync<T>(key);
            }
            else
            {
                entity.Handle = Addressables.LoadAssetsAsync<T>(key, null);
            }
            
            entity.Key = typekey;
            entity.Handle.Completed +=
                (op) =>
                {
                    loadCompleted(entity, action, actions);
                    entity.LoadStatus = eLoadStatus.Ready;
                };
            entities[typekey] = entity;
        }
    }

    /// <summary>
    /// ロード完了
    /// </summary>
    static void loadCompleted<T>(AssetEntity entity, System.Action<T> action = null, System.Action<IList<T>> actions = null)
    {
        if (entity.LoadCancel == true)
        {
            // キャンセル指示が入ってるので、complete を成立させない
            return;
        }

        if (action != null)
        {
            action?.Invoke((T)entity.Handle.Result);
        }
        else
        {
            actions?.Invoke((IList<T>)entity.Handle.Result);
        }
    }

    /// <summary>
    /// ロード中にキャンセル入った場合のアンロード処理
    /// ロード完了を待ってからアンロードする
    /// </summary>
    static IEnumerator loadCancel(string typekey)
    {
        AssetEntity entity = entities[typekey];

        while (entity.LoadStatus == eLoadStatus.Load)
        {
            yield return null;
        }
        
        unload(typekey);
    }
    
    /// <summary>
    /// アンロード
    /// </summary>
    static void unload(string typekey)
    {
        AssetEntity entity = entities[typekey];

        Addressables.Release(entity.Handle);
        entities.Remove(typekey);
    }
}