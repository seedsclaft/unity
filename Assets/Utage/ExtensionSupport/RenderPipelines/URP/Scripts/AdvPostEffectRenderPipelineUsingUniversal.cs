#if UTAGE_URP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UtageExtensions;

namespace Utage.RenderPipeline.Urp
{
	//URPを使ったポストエフェクト処理
	public class AdvPostEffectRenderPipelineUsingUniversal : MonoBehaviour,
		IAdvPostEffectRenderPipelineBridge, IAdvSaveData
	{
		AdvPostEffectManager AdvPostEffectManager => this.GetComponentCache(ref postEffectManager);
		AdvPostEffectManager postEffectManager;
		AdvEngine Engine => AdvPostEffectManager.Engine;

		public virtual (IPostEffectStrength effect, float start, float end) DoCommandColorFade(Camera targetCamera, IAdvCommandFade command)
		{
			var volumes = targetCamera.GetComponentInChildren<AdvCameraPostEffectManager>(true);
			var fadeVolume = volumes.FadeVolume;
			float start, end;
			if (command.Inverse)
			{
				//画面全体のフェードイン（つまりカメラのカラーフェードアウト）
				start = fadeVolume.Strength;
				end = 0;
			}
			else
			{
				//画面全体のフェードアウト（つまりカメラのカラーフェードイン）
				start = fadeVolume.Strength;
				end = command.Color.a;
			}

			if (fadeVolume.TryGetVolumeController(
				    out ColorFadeVolumeController colorFadeVolumeController))
			{
				colorFadeVolumeController.SetColor(command.Color);
				colorFadeVolumeController.SetActive(true);
			}
			else
			{
				Debug.LogError($"{nameof(ColorFadeVolumeController)} is not found", this);
			}
			return (fadeVolume, start, end);
		}

		public virtual (IPostEffectStrength effect, float start, float end) DoCommandRuleFade(Camera targetCamera, IAdvCommandFade command)
		{
			var volumes = targetCamera.GetComponentInChildren<AdvCameraPostEffectManager>(true);
			var fadeVolume = volumes.FadeVolume;
			float start, end;
			if (command.Inverse)
			{
				//画面全体のフェードイン（つまりカメラのカラーフェードアウト）
				start = fadeVolume.Strength;
				end = 0;
			}
			else
			{
				//画面全体のフェードアウト（つまりカメラのカラーフェードイン）
				start = fadeVolume.Strength;
				end = 1;
			}

			if (fadeVolume.TryGetVolumeController(
				    out RuleFadeVolumeController ruleFade))
			{
				ruleFade.SetRuleTexture(Engine.EffectManager.FindRuleTexture(command.RuleImage));
				ruleFade.SetVague(command.Vague);
				ruleFade.SetColor(command.Color);
				ruleFade.SetActive(true);
			}
			else
			{
				Debug.LogError($"{nameof(ColorFadeVolumeController)} is not found", this);
			}
			return (fadeVolume, start, end);
		}

		public (IPostEffect effect, Action onComplete) DoCommandImageEffect(Camera targetCamera, IAdvCommandImageEffect command,
			Action onComplete)
		{
			var manager = targetCamera.GetComponentInChildren<AdvCameraPostEffectManager>(true);
			var imageEffectVolume = manager.ImageEffectVolume;
			var volumeComponent = imageEffectVolume.SetActiveVolume( $"{command.ImageEffectType}Volume");
			return (imageEffectVolume, onComplete);
		}

		public void DoCommandImageEffectAllOff(Camera targetCamera, IAdvCommandImageEffect command, Action onComplete)
		{
			var manager = targetCamera.GetComponentInChildren<AdvCameraPostEffectManager>(true);
			var imageEffectVolume = manager.ImageEffectVolume;
			foreach (var component in imageEffectVolume.Volume.profile.components)
			{
				component.active = false;
			}
			onComplete();
		}

		AdvCameraPostEffectManager[] PostEffectManagers => Engine.CameraManager.GetComponentsInChildren<AdvCameraPostEffectManager>(true);

		public string SaveKey
		{
			get { return "PostEffectRenderPipelineUsingUniversal"; }
		}
		const int Version = 0;
		//セーブデータ用のバイナリ書き込み
		public void OnWrite(BinaryWriter writer)
		{
			var managers = PostEffectManagers;
			writer.Write(Version);
			writer.Write(managers.Length);
			foreach (var manager in managers)
			{
				writer.Write(manager.TargetCamera.name);
				writer.WriteBuffer(manager.Write);
			}
		}

		//セーブデータ用のバイナリ読み込み
		public void OnRead(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version < 0 || version > Version)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
				return;
			}

			var managers = PostEffectManagers;

			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string cameraName = reader.ReadString();
				AdvCameraPostEffectManager manager = managers.FirstOrDefault(x=>x.TargetCamera.name== cameraName);
				if (manager != null)
				{
					reader.ReadBuffer(manager.Read);
				}
				else
				{
					Debug.LogError($"Not found Camera {cameraName}");
					//セーブされていたが、消えているので読み込まない
					reader.SkipBuffer();
				}
			}
		}

		public void OnClear()
		{
			foreach (var manager in PostEffectManagers)
			{
				manager.Clear();
			}
		}
	}
}
#endif
