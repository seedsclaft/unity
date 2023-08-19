using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StatusActorList : ListWindow , IInputHandlerEvent
{
    private bool _animationBusy = false;
    public bool AnimationBusy => _animationBusy;

    private int baseX = 480;
    public void Initialize()
    {
        InitializeListView(1);
        SetInputFrame(24);
        SetDragHandler(ObjectList[0],() => CallListInputHandler(InputKeyType.SideLeft1),() => CallListInputHandler(InputKeyType.SideRight1));
    }

    public void Refresh(ActorInfo actorInfo,List<ActorInfo> actorInfos)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var StatusActor = ObjectList[i].GetComponent<ActorInfoComponent>();
            StatusActor.Clear();
            StatusActor.UpdateInfo(actorInfo,actorInfos);
        }
    }

    public void SetDragHandler(GameObject gameObject,System.Action leftEvent,System.Action rightEvent)
    {
		ContentDragListener dragListener = gameObject.AddComponent<ContentDragListener>();
        dragListener.SetDragMoveEvent((x,y) => 
        {
            if (x > 15)
            {
                leftEvent();
                ResetInputFrame(15);
                dragListener.OnEndDrag();
            } else
            if (x < -15)
            {
                rightEvent();
                ResetInputFrame(15);
                dragListener.OnEndDrag();
            } else
            {
                var targetObject = ObjectList[0];
                targetObject.transform.DOLocalMoveX(baseX - (x * 0.01f * (Screen.width)),0.0f);
            }
        });
        dragListener.SetDragEndEvent(() => {
            if (_animationBusy == false)
            {
                var targetObject = ObjectList[0];
                targetObject.transform.DOLocalMoveX(baseX,0.2f);
            }
        });
    }

    public void MoveActorLeft(System.Action endEvent)
    {
        MoveAnimation(baseX,-960,endEvent);
    }

    public void MoveActorRight(System.Action endEvent)
    {
        MoveAnimation(baseX * -1,960,endEvent);
    }

    private void MoveAnimation(int startX,int MoveX,System.Action endEvent)
    {
        if (_animationBusy) return;
        _animationBusy = true;
        var targetObject = ObjectList[0];
        var StatusActor = ObjectList[0].GetComponent<ActorInfoComponent>();
        Sequence sequence = DOTween.Sequence()
            .Append(targetObject.transform.DOLocalMoveX(baseX + startX,0.2f))
            .Join(StatusActor.AwakenThumb.DOFade(0,0.2f))
            .Append(targetObject.transform.DOLocalMoveX(baseX + MoveX,0.0f))
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                if (endEvent != null)
                {
                    endEvent();
                    Sequence sequence = DOTween.Sequence()
                        .Append(targetObject.transform.DOLocalMoveX(baseX,0.2f))
                        .Join(StatusActor.AwakenThumb.DOFade(1,0.2f))
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => {
                            _animationBusy = false;
                        });
                }
            });
    }
}
