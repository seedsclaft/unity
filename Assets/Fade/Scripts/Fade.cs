/*
 The MIT License (MIT)

Copyright (c) 2013 yamamura tatsuhiko

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using UnityEngine;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

public class Fade : MonoBehaviour
{
	IFade fade;

	float cutoutRange;

	private CancellationTokenSource _cancellationTokenSource;

	public void Init ()
	{
		fade = GetComponent<IFade> ();
		fade.Range = cutoutRange;
	}

	IEnumerator FadeoutCoroutine (float time, System.Action action)
	{
		float endTime = Time.timeSinceLevelLoad + time * cutoutRange;

		var endFrame = new WaitForEndOfFrame ();

		while (Time.timeSinceLevelLoad <= endTime) 
		{
			cutoutRange = (endTime - Time.timeSinceLevelLoad) / time;
			fade.Range = cutoutRange;
			yield return endFrame;
		}
		cutoutRange = 0;
		fade.Range = cutoutRange;

		action?.Invoke();
	}

/*
	IEnumerator FadeinCoroutine (float time, System.Action action)
	{
		float endTime = Time.timeSinceLevelLoad + time * (1 - cutoutRange);
		var endFrame = new WaitForEndOfFrame ();

		while (Time.timeSinceLevelLoad <= endTime) {
			cutoutRange = 1 - ((endTime - Time.timeSinceLevelLoad) / time);
			fade.Range = cutoutRange;
			yield return endFrame;
		}
		cutoutRange = 1;
		fade.Range = cutoutRange;

		if (action != null) {
			action ();
		}
	}
*/

	public Coroutine FadeOut (float time, System.Action action)
	{
		StopAllCoroutines ();
		return StartCoroutine (FadeoutCoroutine (time, action));
	}

	public Coroutine FadeOut (float time)
	{
		return FadeOut (time, null);
	}

/*
	public Coroutine FadeIn (float time, System.Action action)
	{
		StopAllCoroutines ();
		return StartCoroutine (FadeinCoroutine (time, action));
	}

	public Coroutine FadeIn (float time)
	{
		return FadeIn (time, null);
	}
*/
	
	public void FadeIn (float time, System.Action action)
	{
		_cancellationTokenSource = new CancellationTokenSource();  
        UpdateLoop(time,action).Forget();
	}

	private async UniTaskVoid UpdateLoop(float time,System.Action action)
    {
        while (true)
        {
            await FadeIn(time);
			if (action != null) 
			{
				action ();
				_cancellationTokenSource.Cancel();
				break;
			}
        }
    }

    private async UniTask FadeIn(float fadeTime)
    {
        for (var time = 0.0f; time < fadeTime; time += Time.deltaTime)
        {
			cutoutRange = 1 - (fadeTime - time);
			fade.Range = cutoutRange;
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
        }
    }
}