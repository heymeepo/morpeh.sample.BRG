using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button samplePrimitivesButton;
    [SerializeField] private Button sampleAnimationButton;

    public void PrimitivesButtonOnClickAddListener(UnityAction action) => samplePrimitivesButton.onClick.AddListener(action);
    public void AnimationButtonOnClickAddListener(UnityAction action) => sampleAnimationButton.onClick.AddListener(action);
}
